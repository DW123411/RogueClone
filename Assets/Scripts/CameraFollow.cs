using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private void Update()
    {
        Vector3 offest = new Vector3(0, 0, -10);
        Vector3 cameraPos = Camera.main.transform.position;//position of camera
        Camera.main.transform.position = Vector3.Lerp(new Vector3(cameraPos.x, cameraPos.y, cameraPos.z), transform.position + offest, 0.8f * Time.deltaTime);
        Debug.Log(Camera.main.transform.position);
    }
}
