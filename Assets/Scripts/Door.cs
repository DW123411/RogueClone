using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public bool isClosed = true;

    public void openDoor()
    {
        gameObject.layer = 0;
        isClosed = false;
    }
}
