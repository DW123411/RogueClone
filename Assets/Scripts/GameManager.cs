using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;
    public BoardManager boardScript;
    public int playerFoodPoints = 200;
    public float turnDelay = 0.1f;
    public int playerHpPoints = 20;
    [HideInInspector] public bool playersTurn = true;
    public float[] reservedFieldX;
    public float[] reservedFieldY;

    private int level = 1;
    private List<Enemy> enemies;
    private bool enemiesMoving;
    private Text levelText;
    private GameObject levelImage;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != null)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
        enemies = new List<Enemy>();
        boardScript = GetComponent<BoardManager>();
        InitGame();
    }

    void OnLevelWasLoaded(int index)
    {
        level++;
        InitGame();
    }

    void InitGame()
    {
        levelText = GameObject.Find("LevelText").GetComponent<Text>();
        levelText.text = "Level=" + level;
        enemies.Clear();
        boardScript.SetupScene(level);
    }

    public void GameOver()
    {
        levelText.text = "Game Over";
        levelImage.SetActive(true);
        enabled = false;
    }

    void Update()
    {
        if (playersTurn || enemiesMoving)
            return;
        StartCoroutine(MoveEnemies());
    }

    public void AddEnemyToList(Enemy script)
    {
        enemies.Add(script);
    }

    IEnumerator MoveEnemies()
    {
        reserveFields();

        enemiesMoving = true;
        yield return new WaitForSeconds(turnDelay + 0.05f);

        if (enemies.Count == 0)
        {
            yield return new WaitForSeconds(turnDelay);
        }

        for (int i = 0; i < enemies.Count; i++)
        {
            //yield return new WaitForSeconds(0.03f);
            if (checkReservedFields(i, enemies[i].transform.position.x + enemies[i].directionX, enemies[i].transform.position.y + enemies[i].directionY))
            {
                enemies[i].MoveEnemy();
            }
            //yield return new WaitForSeconds(enemies[i].moveTime);
        }

        yield return new WaitForSeconds(turnDelay * 2f);

        playersTurn = true;
        enemiesMoving = false;
    }

    public void removeEnemy(Enemy enemy)
    {
        enemies.Remove(enemy);
    }

    public void reserveFields()
    {
        reservedFieldX = new float[enemies.Count];
        reservedFieldY = new float[enemies.Count];

        for (int i = 0; i < reservedFieldX.Length; i++)
        {
            enemies[i].testMoveEnemy();
            reservedFieldX[i] = enemies[i].transform.position.x + enemies[i].directionX;
            reservedFieldY[i] = enemies[i].transform.position.y + enemies[i].directionY;
        }
    }

    public bool checkReservedFields(int enemy, float positionX, float positionY)
    {
        for (int i = 0; i < reservedFieldX.Length; i++)
        {
            if(i != enemy && positionX.Equals(reservedFieldX[i]) && positionY.Equals(reservedFieldY[i]))
            {
                return false;
            }
        }
        return true;
    }
}
