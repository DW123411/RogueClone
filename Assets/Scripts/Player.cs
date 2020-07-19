using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MovingObject
{
    public int foodPerPoints = 150;
    public float restartLevelDelay = 1f;

    private int food;
    private int hp;
    private Vector2 touchOrigin = -Vector2.one;

    // Start is called before the first frame update
    protected override void Start()
    {
        food = GameManager.instance.playerFoodPoints;
        hp = GameManager.instance.playerHpPoints;

        base.Start();
    }

    private void OnDisable()
    {
        GameManager.instance.playerFoodPoints = food;
        GameManager.instance.playerHpPoints = hp;
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameManager.instance.playersTurn) return;

        int horizontal = 0;
        int vertical = 0;

    #if UNITY_STANDALONE || UNITY_WEBPLAYER

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
                Vector2 touchEnd = myTouch.position;
                float x = touchEnd.x - touchOrigin.x;
                float y = touchEnd.y - touchOrigin.y;
                touchOrigin.x = -1;
                if (Mathf.Abs(x) > Mathf.Abs(y))
                    horizontal = x > 0 ? 1 : -1;
                else
                    vertical = y > 0 ? 1 : -1;
            }
        }

    #endif

        if (horizontal != 0 || vertical != 0)
            AttemptMove<Door>(horizontal, vertical);
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
            Invoke("Restart", restartLevelDelay);
            enabled = false;
        }
        else if(other.tag == "Food")
        {
            food += foodPerPoints;
            other.gameObject.SetActive(false);
        }
    }

    protected override void OnCantMove<T>(T component)
    {
        Door hitDoor = component as Door;
        hitDoor.openDoor();
    }

    private void Restart()
    {
        Application.LoadLevel(Application.loadedLevel);
    }

    public void loseHp(int loss)
    {
        hp -= loss;
        CheckIfGameOver();
    }

    private void CheckIfGameOver()
    {
        if (hp <= 0)
            GameManager.instance.GameOver();
    }
}
