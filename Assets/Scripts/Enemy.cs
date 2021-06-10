using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MovingObject
{
    public int playerDamage;
    public int hp;
    public int directionX;
    public int directionY;

    private Transform target;
    private bool skipMove;
    private Text message;
    private Text message2;

    protected override void Start()
    {
        GameManager.instance.AddEnemyToList(this);
        target = GameObject.FindGameObjectWithTag("Player").transform;
        message = GameObject.Find("MessagesText1").GetComponent<Text>();
        message.text = "";
        message2 = GameObject.Find("MessageText2").GetComponent<Text>();
        message2.text = "";
        base.Start();
    }

    protected override void AttemptMove<T>(int xDir, int yDir)
    {
        if (skipMove)
        {
            skipMove = false;
            return;

        }
        base.AttemptMove<T>(xDir, yDir);
        skipMove = true;
    }

    public void MoveEnemy()
    {
        int xDir = 0;
        int yDir = 0;
        if (Mathf.Abs(target.position.x - transform.position.x) < float.Epsilon)
            yDir = target.position.y > transform.position.y ? 1 : -1;
        else
            xDir = target.position.x > transform.position.x ? 1 : -1;
        AttemptMove<Player>(xDir, yDir);
    }

    protected override void OnCantMove<T>(T component)
    {
        if (Random.value >= 0.5f)
        {
            Player hitPlayer = component as Player;
            hitPlayer.loseHp(Random.Range(2, playerDamage));
            message.text = message2.text;
            message2.text = "Enemy hits player.";
        }
        else
        {
            message.text = message2.text;
            message2.text = "Enemy misses.";
        }
    }

    public void loseHp(int loss)
    {
        hp -= loss;
        if (hp <= 0)
        {
            message.text = message2.text;
            message2.text = "Player killed enemy.";
            gameObject.SetActive(false);
            GameManager.instance.removeEnemy(this);
        }
    }

    public void testMoveEnemy()
    {
        int xDir = 0;
        int yDir = 0;
        if (Mathf.Abs(target.position.x - transform.position.x) < float.Epsilon)
            yDir = target.position.y > transform.position.y ? 1 : -1;
        else
            xDir = target.position.x > transform.position.x ? 1 : -1;
        directionX = xDir;
        directionY = yDir;
    }
}
