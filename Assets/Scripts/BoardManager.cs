using System.Collections;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;
using UnityEngine;


public class BoardManager : MonoBehaviour
{
    [Serializable]
    public class Count
    {
        public int minimum;
        public int maximum;

        public Count(int min, int max)
        {
            minimum = min;
            maximum = max;
        }
    }

    //Oryginalna wielkość - 78x22
    //Maksymalna wielkość poziomu - 78x30
    public int columns = 78;
    public int rows = 30;
    public Count roomWidthCount = new Count(4, 25); //Maksymalna i minimalna szerokość pomieszczenia
    public Count roomHeightCount = new Count(4, 9);//Maksymalna i minimalna wysokość pomieszczenia
    public Count foodCount = new Count(1, 5);
    public GameObject player;
    public GameObject exit;
    //Tablice przechowujące obiekty Unity
    public GameObject[] floorTiles;
    public GameObject[] horizontalWallTiles;
    public GameObject[] verticalWallTiles;
    public GameObject[] foodTiles;
    public GameObject[] enemyTiles;
    public GameObject[] doorTiles;
    public GameObject[] corridorTiles;
    public GameObject[] emptyTiles;

    private Transform boardHolder;
    private List<Vector3> gridPositions = new List<Vector3>();
    private GameObject[,] emptyVectors = new GameObject[78, 30];
    private int[] roomsWidth = new int[9];
    private int[] roomsHeight = new int[9];
    private int[] bottomLeftCornersX = new int[9];
    private int[] bottomLeftCornersY = new int[9];
    private int[] playerPosition = new int[3];
    private int[] exitPosition = new int[3];

    void InitialiseList()
    {
        //Wyczyść wszystkie pozycje siatki
        gridPositions.Clear();

        for(int i = 0; i < columns; i++)
        {
            for(int j = 0; j < rows; j++)
            {
                gridPositions.Add(new Vector3(i, j, 0f));

                GameObject toInstantiate = emptyTiles[Random.Range(0, doorTiles.Length)];

                GameObject instance = Instantiate(toInstantiate, new Vector3(i, j, 0f), Quaternion.identity) as GameObject;

                emptyVectors[i, j] = instance;

                instance.transform.SetParent(boardHolder);
            }
        }
    }

    void BoardSetup()
    {
        boardHolder = new GameObject("Board").transform;

        int YAxisIterator = -1;

        //Wylosuj dla każdego pomieszczenia wymiar oraz jego pozycję (biorąc pod uwagę wymaganą przerwę pomiedzy pomieszczeniami na korytarze)
        for(int i = 0; i < 9; i++)
        {
            if((double)i % 3 == 0)
            {
                YAxisIterator++;
            }
            roomsWidth[i] = Random.Range(roomWidthCount.minimum, roomWidthCount.maximum);
            roomsHeight[i] = Random.Range(roomHeightCount.minimum, roomHeightCount.maximum);
            int bottomLeftCornerX = (int)((((double)i % 3)) * ((double)columns / 3) + 1);
            int bottomLeftCornerY = (int)((((double)YAxisIterator % 3)) * ((double)rows / 3) + 1);
            bottomLeftCornersX[i] = Random.Range(bottomLeftCornerX, bottomLeftCornerX + (int)((double)columns / 3) - roomsWidth[i]);
            bottomLeftCornersY[i] = Random.Range(bottomLeftCornerY, bottomLeftCornerY + (int)((double)rows / 3) - roomsHeight[i]);
        }

        //Tablica "doors" będzie określać współrzędne położenia drzwi
        int[,] doors = new int[columns, rows];
        for(int i = 0; i < columns; i++)
        {
            for(int j = 0; j < rows; j++)
            {
                doors[i, j] = 0;
            }
        }

        //Utwórz graf z 9 wierzchołkami (pomieszczeniami) i 12 możliwymi krawędziami (korytarzami)
        int V = 9;
        int E = 12;
        Graph graph = new Graph(V, E);

        //Określenie źródła i celu krawędzi oraz jej wagi (jest to odległość pomiędzy pomieszczeniami)
        graph.edge[0].src = 0;
        graph.edge[0].dest = 1;
        graph.edge[0].weight = bottomLeftCornersX[1] - roomsWidth[0];

        graph.edge[1].src = 1;
        graph.edge[1].dest = 2;
        graph.edge[1].weight = bottomLeftCornersX[2] - (bottomLeftCornersX[1] + roomsWidth[1]);

        graph.edge[2].src = 0;
        graph.edge[2].dest = 3;
        graph.edge[2].weight = bottomLeftCornersY[3] - roomsHeight[0];

        graph.edge[3].src = 1;
        graph.edge[3].dest = 4;
        graph.edge[3].weight = bottomLeftCornersY[4] - roomsHeight[1];

        graph.edge[4].src = 2;
        graph.edge[4].dest = 5;
        graph.edge[4].weight = bottomLeftCornersY[5] - roomsHeight[2];

        graph.edge[5].src = 3;
        graph.edge[5].dest = 4;
        graph.edge[5].weight = bottomLeftCornersX[4] - roomsWidth[3];

        graph.edge[6].src = 4;
        graph.edge[6].dest = 5;
        graph.edge[6].weight = bottomLeftCornersX[5] - (bottomLeftCornersX[4] + roomsWidth[4]);

        graph.edge[7].src = 3;
        graph.edge[7].dest = 6;
        graph.edge[7].weight = bottomLeftCornersY[6] - (bottomLeftCornersY[3] + roomsHeight[3]);

        graph.edge[8].src = 4;
        graph.edge[8].dest = 7;
        graph.edge[8].weight = bottomLeftCornersY[7] - (bottomLeftCornersY[4] + roomsHeight[4]);

        graph.edge[9].src = 5;
        graph.edge[9].dest = 8;
        graph.edge[9].weight = bottomLeftCornersY[8] - (bottomLeftCornersY[5] + roomsHeight[5]);

        graph.edge[10].src = 6;
        graph.edge[10].dest = 7;
        graph.edge[10].weight = bottomLeftCornersX[7] - roomsWidth[6];

        graph.edge[11].src = 7;
        graph.edge[11].dest = 8;
        graph.edge[11].weight = bottomLeftCornersX[8] - (bottomLeftCornersX[7] + roomsWidth[7]);

        //Wykonaj algorytm Kruskala tworzący minimalne drzewo rozpinające dla grafu. Zwrócona zostaje macierz sąsiedztwa dla grafu
        int[,] matrix = graph.KruskalMST();

        //Dla każdego połączenia w macierzy sąsiedztwa wylosuj położenie drzwi i narysuj korytarze pomiędzy pomieszczeniami 
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                if (j > i)
                {
                    if (matrix[i, j] != 0)
                    {
                        //Jeżeli pomieszczenie j jest nad pomieszczeniem i
                        if ((j >= 3 && j <= 5 && i < 3) || (j >= 6 && j <= 8 && i < 6))
                        {
                            GameObject toInstantiate = doorTiles[Random.Range(0, doorTiles.Length)];

                            int randomXj = Random.Range(bottomLeftCornersX[j] + 1, bottomLeftCornersX[j] + roomsWidth[j] - 1);

                            if (emptyVectors[randomXj, bottomLeftCornersY[j]] != null)
                            {
                                emptyVectors[randomXj, bottomLeftCornersY[j]].SetActive(false);
                            }

                            GameObject instance = Instantiate(toInstantiate, new Vector3(randomXj, bottomLeftCornersY[j], 0f), Quaternion.identity) as GameObject;

                            doors[randomXj, bottomLeftCornersY[j]] = 1;

                            instance.transform.SetParent(boardHolder);

                            toInstantiate = doorTiles[Random.Range(0, doorTiles.Length)];

                            int randomXi = Random.Range(bottomLeftCornersX[i] + 1, bottomLeftCornersX[i] + roomsWidth[i] - 1);

                            if (emptyVectors[randomXi, bottomLeftCornersY[i] + roomsHeight[i] - 1] != null)
                            {
                                emptyVectors[randomXi, bottomLeftCornersY[i] + roomsHeight[i] - 1].SetActive(false);
                            }

                            instance = Instantiate(toInstantiate, new Vector3(randomXi, bottomLeftCornersY[i] + roomsHeight[i] - 1, 0f), Quaternion.identity) as GameObject;

                            doors[randomXi, bottomLeftCornersY[i] + roomsHeight[i] - 1] = 1;

                            instance.transform.SetParent(boardHolder);

                            LayoutCorridor(randomXj, bottomLeftCornersY[j], randomXi, bottomLeftCornersY[i] + roomsHeight[i] - 1, j, i);
                        }
                        //Jeżeli pomieszczenie i jest po prawej stronie pomieszczenia j
                        else if (bottomLeftCornersX[i] - bottomLeftCornersX[j] > 0)
                        {
                            GameObject toInstantiate = doorTiles[Random.Range(0, doorTiles.Length)];

                            int randomYi = Random.Range(bottomLeftCornersY[i] + 1, bottomLeftCornersY[i] + roomsHeight[i] - 1);

                            if (emptyVectors[bottomLeftCornersX[i], randomYi] != null)
                            {
                                emptyVectors[bottomLeftCornersX[i], randomYi].SetActive(false);
                            }

                            GameObject instance = Instantiate(toInstantiate, new Vector3(bottomLeftCornersX[i], randomYi, 0f), Quaternion.identity) as GameObject;

                            doors[bottomLeftCornersX[i], randomYi] = 1;

                            instance.transform.SetParent(boardHolder);

                            toInstantiate = doorTiles[Random.Range(0, doorTiles.Length)];

                            int randomYj = Random.Range(bottomLeftCornersY[j] + 1, bottomLeftCornersY[j] + roomsHeight[j] - 1);

                            if (emptyVectors[bottomLeftCornersX[j] + roomsWidth[j] - 1, randomYj] != null)
                            {
                                emptyVectors[bottomLeftCornersX[j] + roomsWidth[j] - 1, randomYj].SetActive(false);
                            }

                            instance = Instantiate(toInstantiate, new Vector3(bottomLeftCornersX[j] + roomsWidth[j] - 1, randomYj, 0f), Quaternion.identity) as GameObject;

                            doors[bottomLeftCornersX[j] + roomsWidth[j] - 1, randomYj] = 1;

                            instance.transform.SetParent(boardHolder);

                            LayoutCorridor(bottomLeftCornersX[i], randomYi, bottomLeftCornersX[j] + roomsWidth[j] - 1, randomYj, i, j);
                        }
                        //Jeżeli pomieszczenie i jest po lewej stronie pomieszczenia j
                        else
                        {
                            GameObject toInstantiate = doorTiles[Random.Range(0, doorTiles.Length)];

                            int randomYj = Random.Range(bottomLeftCornersY[j] + 1, bottomLeftCornersY[j] + roomsHeight[j] - 1);

                            if (emptyVectors[bottomLeftCornersX[j], randomYj] != null)
                            {
                                emptyVectors[bottomLeftCornersX[j], randomYj].SetActive(false);
                            }

                            GameObject instance = Instantiate(toInstantiate, new Vector3(bottomLeftCornersX[j], randomYj, 0f), Quaternion.identity) as GameObject;

                            doors[bottomLeftCornersX[j], randomYj] = 1;

                            instance.transform.SetParent(boardHolder);

                            toInstantiate = doorTiles[Random.Range(0, doorTiles.Length)];

                            int randomYi = Random.Range(bottomLeftCornersY[i] + 1, bottomLeftCornersY[i] + roomsHeight[i] - 1);

                            if (emptyVectors[bottomLeftCornersX[i] + roomsWidth[i] - 1, randomYi] != null)
                            {
                                emptyVectors[bottomLeftCornersX[i] + roomsWidth[i] - 1, randomYi].SetActive(false);
                            }

                            instance = Instantiate(toInstantiate, new Vector3(bottomLeftCornersX[i] + roomsWidth[i] - 1, randomYi, 0f), Quaternion.identity) as GameObject;

                            doors[bottomLeftCornersX[i] + roomsWidth[i] - 1, randomYi] = 1;

                            instance.transform.SetParent(boardHolder);

                            LayoutCorridor(bottomLeftCornersX[j], randomYj, bottomLeftCornersX[i] + roomsWidth[i] - 1, randomYi, j, i);
                        }
                    }
                }
            }
        }

        //Dla każdego pomieszczenia wyłóż na planszy tekstury podłogi oraz ścian
        for (int i = 0; i < 9 ; i++)
        {
            for (int j = bottomLeftCornersX[i]; j < roomsWidth[i] + bottomLeftCornersX[i]; j++)
            {
                for(int k = bottomLeftCornersY[i]; k < roomsHeight[i] + bottomLeftCornersY[i]; k++)
                {
                    if (doors[j, k] != 1)
                    {
                        if(emptyVectors[j, k] != null)
                        {
                            emptyVectors[j, k].SetActive(false);
                        }
                        GameObject toInstantiate = floorTiles[Random.Range(0, floorTiles.Length)];
                        if (j == bottomLeftCornersX[i] || j == roomsWidth[i] + bottomLeftCornersX[i] - 1)
                            toInstantiate = horizontalWallTiles[Random.Range(0, horizontalWallTiles.Length)];
                        if (k == bottomLeftCornersY[i] || k == roomsHeight[i] + bottomLeftCornersY[i] - 1)
                            toInstantiate = verticalWallTiles[Random.Range(0, verticalWallTiles.Length)];

                        GameObject instance = Instantiate(toInstantiate, new Vector3(j, k, 0f), Quaternion.identity) as GameObject;

                        instance.transform.SetParent(boardHolder);
                    }
                }
            }
        }
    }

    void LayoutCorridor(int srcX, int srcY, int destX, int destY, int srcRoom, int destRoom)
    {
        if ((srcRoom >= 3 && srcRoom <= 5 && destRoom < 3) || (srcRoom >= 6 && srcRoom <= 8 && destRoom < 6))
        {
            if(srcX - destX > 0)
            {
                for (int i = srcX; i > destX; i--)
                {
                    LayoutCorridorTile(i, srcY - 1);
                }
            }
            else
            {
                for (int i = srcX; i < destX; i++)
                {
                    LayoutCorridorTile(i, srcY - 1);
                }
            }

            if (srcY - destY > 0)
            {
                for (int i = srcY - 1; i > destY; i--)
                {
                    LayoutCorridorTile(destX, i);
                }
            }
            else
            {
                for (int i = srcY - 1; i < destY; i++)
                {
                    LayoutCorridorTile(destX, i);
                }
            }
        }
        else if(srcX - destX > 0)
        {
            for (int i = srcX - 1; i > destX; i--)
            {
                LayoutCorridorTile(i, srcY);
            }

            if (srcY - destY > 0)
            {
                for (int i = srcY; i >= destY; i--)
                {
                    LayoutCorridorTile(destX + 1, i);
                }
            }
            else
            {
                for (int i = srcY; i <= destY; i++)
                {
                    LayoutCorridorTile(destX + 1, i);
                }
            }
        }
        else
        {
            for (int i = destY; i > srcY; i--)
            {
                LayoutCorridorTile(destX - 1, i);
            }

            if (destY - srcY > 0)
            {
                for (int i = srcY; i >= destY; i--)
                {
                    LayoutCorridorTile(destX + 1, i);
                }
            }
            else
            {
                for (int i = srcY; i <= destY; i++)
                {
                    LayoutCorridorTile(destX + 1, i);
                }
            }
        }
    }

    void LayoutCorridorTile(int x, int y)
    {
        if (emptyVectors[x, y] != null)
        {
            emptyVectors[x, y].SetActive(false);
        }

        GameObject toInstantiate = corridorTiles[Random.Range(0, corridorTiles.Length)];

        GameObject instance = Instantiate(toInstantiate, new Vector3(x, y, 0f), Quaternion.identity) as GameObject;

        instance.transform.SetParent(boardHolder);
    }

    void LayoutExit()
    {
        int randomRoom = Random.Range(0, 8);
        int randomPositionX = Random.Range(bottomLeftCornersX[randomRoom] + 1, bottomLeftCornersX[randomRoom] + roomsWidth[randomRoom] - 1);
        int randomPositionY = Random.Range(bottomLeftCornersY[randomRoom] + 1, bottomLeftCornersY[randomRoom] + roomsHeight[randomRoom] - 1);

        exitPosition[0] = randomRoom;
        exitPosition[1] = randomPositionX;
        exitPosition[2] = randomPositionY;

        GameObject toInstantiate = exit;

        GameObject instance = Instantiate(toInstantiate, new Vector3(randomPositionX, randomPositionY, 0f), Quaternion.identity) as GameObject;

        instance.transform.SetParent(boardHolder);
    }

    void LayoutFood()
    {
        for (int i = 0; i < 2; i++)
        {
            int randomRoom = Random.Range(0, 8);
            int randomPositionX = Random.Range(bottomLeftCornersX[randomRoom] + 1, bottomLeftCornersX[randomRoom] + roomsWidth[randomRoom] - 1);
            int randomPositionY = Random.Range(bottomLeftCornersY[randomRoom] + 1, bottomLeftCornersY[randomRoom] + roomsHeight[randomRoom] - 1);

            if ((randomPositionX == playerPosition[1] && randomPositionY == playerPosition[2]) || (randomPositionX == exitPosition[1] && randomPositionY == exitPosition[2]))
            {
                if (randomPositionX != bottomLeftCornersX[randomRoom] + roomsWidth[randomRoom] - 1)
                    randomPositionX++;
                else if (randomPositionY != bottomLeftCornersY[randomRoom] + roomsHeight[randomRoom] - 1)
                    randomPositionY++;
                else if (randomPositionX != bottomLeftCornersX[randomRoom] + 1)
                    randomPositionX--;
                else if (randomPositionY != bottomLeftCornersY[randomRoom] + 1)
                    randomPositionY--;
            }

            GameObject toInstantiate = foodTiles[Random.Range(0, foodTiles.Length)];

            GameObject instance = Instantiate(toInstantiate, new Vector3(randomPositionX, randomPositionY, 0f), Quaternion.identity) as GameObject;

            instance.transform.SetParent(boardHolder);
        }
    }

    void LayoutPlayer()
    {
        int randomRoom = Random.Range(0, 8);
        int randomPositionX = Random.Range(bottomLeftCornersX[randomRoom] + 1, bottomLeftCornersX[randomRoom] + roomsWidth[randomRoom] - 1);
        int randomPositionY = Random.Range(bottomLeftCornersY[randomRoom] + 1, bottomLeftCornersY[randomRoom] + roomsHeight[randomRoom] - 1);

        playerPosition[0] = randomRoom;
        playerPosition[1] = randomPositionX;
        playerPosition[2] = randomPositionY;

        GameObject toInstantiate = player;

        GameObject instance = Instantiate(toInstantiate, new Vector3(randomPositionX, randomPositionY, 0f), Quaternion.identity) as GameObject;

        instance.transform.SetParent(boardHolder);
    }

    void LayoutEnemies(int level)
    {
        int random = Random.Range(5, 10);
        for (int i = 0; i < random; i++)
        {
            int randomRoom = Random.Range(0, 8);
            int randomPositionX = Random.Range(bottomLeftCornersX[randomRoom] + 1, bottomLeftCornersX[randomRoom] + roomsWidth[randomRoom] - 1);
            int randomPositionY = Random.Range(bottomLeftCornersY[randomRoom] + 1, bottomLeftCornersY[randomRoom] + roomsHeight[randomRoom] - 1);

            if (randomPositionX == playerPosition[1] && randomPositionY == playerPosition[2])
            {
                if (randomPositionX != bottomLeftCornersX[randomRoom] + roomsWidth[randomRoom] - 1)
                    randomPositionX++;
                else if (randomPositionY != bottomLeftCornersY[randomRoom] + roomsHeight[randomRoom] - 1)
                    randomPositionY++;
                else if (randomPositionX != bottomLeftCornersX[randomRoom] + 1)
                    randomPositionX--;
                else if (randomPositionY != bottomLeftCornersY[randomRoom] + 1)
                    randomPositionY--;
            }

            GameObject toInstantiate = enemyTiles[Random.Range(0, enemyTiles.Length)];

            GameObject instance = Instantiate(toInstantiate, new Vector3(randomPositionX, randomPositionY, 0f), Quaternion.identity) as GameObject;

            instance.transform.SetParent(boardHolder);
        }
    }

    Vector3 RandomPosition()
    {
        int randomIndex = Random.Range(0, gridPositions.Count);
        Vector3 randomPosition = gridPositions[randomIndex];
        gridPositions.RemoveAt(randomIndex);
        return randomPosition;
    }

    public void SetupScene(int level)
    {
        InitialiseList();
        BoardSetup();
        //InitialiseList();
        LayoutPlayer();
        LayoutEnemies(level);
        LayoutExit();
        LayoutFood();
        //LayoutObjectAtRandom(foodTiles, foodCount.minimum, foodCount.maximum);
        //int enemyCount = (int)Mathf.Log(level, 2f);
        //LayoutObjectAtRandom(enemyTiles, enemyCount, enemyCount);
        //Instantiate(exit, new Vector3(columns - 1, rows - 1, 0f), Quaternion.identity);
    }
}
