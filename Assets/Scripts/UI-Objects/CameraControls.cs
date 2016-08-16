using UnityEngine;
using System.Collections;

public class CameraControls : MonoBehaviour {

    public Vector3 startPosition;
    public float mouseSensitivity = 0.01f;
    public float zoomSensitivity = 0.2f;

    private World world;
    private Vector3 lastPosition;
    private Camera worldCamera;

    private Vector2 clampLow;
    private Vector2 clampHigh;
    private float maxZoom;

    // Use this for initialization
    void Start () {
        world = GameObject.Find("World").GetComponent<World>();
        lastPosition = startPosition;
        worldCamera = GetComponent<Camera>();
        updateCameraClamp();

    }
	
	void Update () {
        if (Input.GetMouseButtonDown(0))//mouse is first pressed
        {
            lastPosition = Input.mousePosition;
        }
        
        if (Input.GetMouseButton(0)) //mouse is pressed
        {
            Vector3 new_position = -(Input.mousePosition - lastPosition);
            new_position.z = 0;
            new_position =  new_position * mouseSensitivity + transform.position;
            new_position.x = Mathf.Clamp(new_position.x, clampLow.x, clampHigh.x);
            new_position.y = Mathf.Clamp(new_position.y, clampLow.y, clampHigh.y);
            transform.position = new_position;
        }

        float da = Input.GetAxis("Mouse ScrollWheel");
        if (da < 0)
        {
            if (worldCamera.orthographicSize + zoomSensitivity < maxZoom)
            {
                worldCamera.orthographicSize += zoomSensitivity;
                //move towards origin in case this zoom could bring us out of clamp
                //zooming changes clamp by zoomsensitivity / 2
                transform.Translate(-zoomSensitivity / 4 * transform.position.x, -zoomSensitivity / 4 * transform.position.y, 0);
                updateCameraClamp();
            }

        } else if(da > 0)
        {
            worldCamera.orthographicSize -= zoomSensitivity;
            updateCameraClamp();
        }

    }

    private void updateCameraClamp()
    {
        float vextent = worldCamera.orthographicSize;
        float hextent = worldCamera.orthographicSize * Screen.height / Screen.width;

        /*
         * Precompute min-max for camera. This assumes world is centered at origin
         */

        clampLow = new Vector2(world.boundariesLow.x + vextent * 2, world.boundariesLow.y + hextent * 2);
        clampHigh = new Vector2(world.boundariesHigh.x - vextent * 2, world.boundariesHigh.y - hextent * 2);
        maxZoom = Mathf.Min(world.size.x, world.size.y) / 4;
        Debug.Log(maxZoom);
    }
}
