using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : MovingObject
{
    public int foodPerFood = 150;
    public int hpPerFood = 5;
    public int enemyDamage = 5;
    public float restartLevelDelay = 1f;
    public Boolean onExit = false;

    private int food;
    private int hp;
    private Vector2 touchOrigin = -Vector2.one;
    private Text hpText;
    private Text foodText;
    private GameObject exitButton;
    private Text message;
    private Text message2;

    protected override void Start()
    {
        food = GameManager.instance.playerFoodPoints;
        hp = GameManager.instance.playerHpPoints;
        hpText = GameObject.Find("HpText").GetComponent<Text>();
        hpText.text = "HP=" + hp + "/20";
        foodText = GameObject.Find("FoodText").GetComponent<Text>();
        foodText.text = "Food=" + food;
        exitButton = GameObject.Find("ExitButton");
        message = GameObject.Find("MessagesText1").GetComponent<Text>();
        message.text = "";
        message2 = GameObject.Find("MessageText2").GetComponent<Text>();
        message2.text = "";

        base.Start();
    }

    private void OnDisable()
    {
        GameManager.instance.playerFoodPoints = food;
        GameManager.instance.playerHpPoints = hp;
    }

    void Update()
    {
        hpText.text = "HP=" + hp + "/20";
        foodText.text = "Food=" + food;

        if (!GameManager.instance.playersTurn) return;

        int horizontal = 0;
        int vertical = 0;

        if (!onExit)
        {
            exitButton.SetActive(false);
        }
        else
        {
            exitButton.SetActive(true);
        }

    #if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER

        horizontal = (int)Input.GetAxisRaw("Horizontal");
        vertical = (int)Input.GetAxisRaw("Vertical");

        if (horizontal != 0)
            vertical = 0;

    #else

        if(Input.touchCount > 0)
        {
            Touch myTouch = Input.touches[0];

            if (myTouch.phase == TouchPhase.Began)
            {
                touchOrigin = myTouch.position;
            }
            else if (myTouch.phase == TouchPhase.Ended && touchOrigin.x >= 0)
            {
                /*Vector2 touchEnd = myTouch.position;
                float x = touchEnd.x - touchOrigin.x;
                float y = touchEnd.y - touchOrigin.y;
                touchOrigin.x = -1;
                if (Mathf.Abs(x) > Mathf.Abs(y))
                    horizontal = x > 0 ? 1 : -1;
                else
                    vertical = y > 0 ? 1 : -1;*/
                float halfScreenHeight = Screen.height / 2;
                float halfScreenWidth = Screen.width / 2;
                float verticalDiff = Math.Abs(halfScreenHeight - touchOrigin.y);
                float horizontalDiff = Math.Abs(halfScreenWidth - touchOrigin.x);
                if (verticalDiff >= horizontalDiff)
                {
                    if (touchOrigin.y <= halfScreenHeight)
                    {
                        vertical = -1;
                    }
                    else
                    {
                        vertical = 1;
                    }
                }
                else
                {
                    if(touchOrigin.x <= halfScreenWidth)
                    {
                        horizontal = -1;
                    }
                    else
                    {
                        horizontal = 1;
                    }
                }
            }
        }

    #endif

        if (horizontal != 0 || vertical != 0)
            AttemptMove<Enemy>(horizontal, vertical);
    }

    protected override void AttemptMove<T>(int xDir, int yDir)
    {
        food--;

        base.AttemptMove<T>(xDir, yDir);

        RaycastHit2D hit;

        CheckIfGameOver();

        GameManager.instance.playersTurn = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.tag == "Exit")
        {
            onExit = true;
        }
        else if(other.tag == "Food")
        {
            food += foodPerFood;
            hp += hpPerFood;
            message.text = message2.text; 
            message2.text = "Picked up food.";
            if (hp > 20)
            {
                hp = 20;
            }
            other.gameObject.SetActive(false);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if(other.tag == "Exit")
        {
            onExit = false;
        }
    }

    protected override void OnCantMove<T>(T component)
    {
        /*Door hitDoor = component as Door;
        hitDoor.openDoor();*/
        Enemy hitEnemy = component as Enemy;
        hitEnemy.loseHp(enemyDamage);
        if (hitEnemy.isActiveAndEnabled)
        {
            message.text = message2.text;
            message2.text = "Player hits enemy.";
        }
    }

    private void Restart()
    {
        SceneManager.LoadScene(0);
    }

    public void loseHp(int loss)
    {
        hp -= loss;
        CheckIfGameOver();
    }

    private void CheckIfGameOver()
    {
        if (hp <= 0)
        {
            message.text = message2.text;
            message2.text = "Player died.";
            GameManager.instance.GameOver();
        }
    }
}
