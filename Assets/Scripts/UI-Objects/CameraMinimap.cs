using UnityEngine;
using System.Collections;

public class CameraMinimap : MonoBehaviour {

    public void Start()
    {
        Camera c = GetComponent<Camera>();
        c.orthographicSize = GameObject.Find("Main Camera").GetComponent<CameraControls>().MaxZoom;

    }

}
