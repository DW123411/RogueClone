using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;

    void FixedUpdate()
    {
        transform.position = new Vector3(player.position.x, player.position.y, player.position.z);
    }
}
