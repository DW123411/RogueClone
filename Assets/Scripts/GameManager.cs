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
        enemiesMoving = true;
        yield return new WaitForSeconds(turnDelay);

        if (enemies.Count == 0)
        {
            yield return new WaitForSeconds(turnDelay);
        }

        for (int i = 0; i < enemies.Count; i++)
        {
            enemies[i].MoveEnemy();
            yield return new WaitForSeconds(enemies[i].moveTime);
        }

        playersTurn = true;
        enemiesMoving = false;
    }
}
