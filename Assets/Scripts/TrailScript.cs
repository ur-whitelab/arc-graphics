using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;
#if UNITY_EDITOR
// Set up touch input propagation while using Instant Preview in the editor.
using Input = GoogleARCore.InstantPreviewInput;
#endif


public class TrailScript : MonoBehaviour
{

    public GameObject gameObject;
    //public GameObject camera;
    public List<GameObject> Points = new List<GameObject>();
    private Camera camera;

    // Use this for initialization
    void Start()
    {
        camera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetMouseButton(0) || Input.touchCount > 0)
        {
            Debug.Log("Touched");
            Touch touch = Input.GetTouch(0);
            Vector2 scaledFirstTouch = touch.position;
            Vector2 cameraDims = new Vector2(camera.pixelWidth, camera.pixelHeight);
            Vector2 unitaryFirstTouch = scaledFirstTouch / cameraDims;
            Debug.Log("Touched Unitary Position " + unitaryFirstTouch.x + "," + unitaryFirstTouch.y);
            RaycastHit hit;
            Ray ray = camera.ViewportPointToRay(new Vector3(unitaryFirstTouch.x, unitaryFirstTouch.y, 0));
            if(Physics.Raycast(ray.origin, ray.direction, out hit))
            {
                if(hit.collider.name == "coil_1")
                {
                    Debug.Log("Hit a raycast on coil_1!");
                }
                else
                {
                    Debug.Log("no raycast hit");
                }
            }
            //Vector3 camPos = camera.transform.position;
            //Vector3 camDirection = camera.transform.forward;
            //Quaternion camRotation = camera.transform.rotation;
            //float spawnDistance = 2;
            /*
            Debug.Log("Touched" + camPos.x + " " + camPos.y + " " + camPos.z);
            Vector3 spawnPos = camPos + (camDirection * spawnDistance);
            GameObject cur = Instantiate(gameObject, spawnPos, camRotation);
            cur.transform.SetParent(this.transform);*/
        }
    }
}