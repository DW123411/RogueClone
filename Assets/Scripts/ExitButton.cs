using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ExitButton : MonoBehaviour
{
    public void goToNextLevel()
    {
        Player player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        if (player.onExit)
        {
            player.Invoke("Restart", player.restartLevelDelay);
            player.enabled = false;
        }
    }
}
