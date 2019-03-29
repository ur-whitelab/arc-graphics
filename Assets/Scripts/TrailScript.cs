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
    private float lineWidth;
    private Vector3 lineStartPoint;
    private Vector3 lineEndPoint;
    private Vector3[] points;
    // Use this for initialization
    void Start()
    {
        camera = Camera.main;
        lineWidth = 0.05f;
        lineStartPoint = new Vector3(0, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        LineRenderer line = this.GetComponent<LineRenderer>();
        if (Input.GetMouseButton(0) || Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Debug.Log("Touched Position " + touch.position.x + "," + touch.position.y);
            RaycastHit hit;
            Ray ray = camera.ScreenPointToRay(touch.position);
            Debug.Log("Touched");
            Vector3 targetPoint = new Vector3(touch.position.x, touch.position.y, camera.nearClipPlane);
            if (touch.phase == TouchPhase.Began)
            {
                if (Physics.Raycast(ray.origin, ray.direction, out hit))
                {
                    if (hit.collider.name == "pfr")
                    {
                        Debug.Log("Hit a raycast on pfr!");
                        lineStartPoint = hit.collider.transform.position;
                        
                        this.points = new Vector3[] { lineStartPoint, lineStartPoint };//put first point as first clicked object's center
                        line.positionCount = this.points.Length;
                        line.SetPositions(this.points);
                        line.startWidth = lineWidth;
                        line.endWidth = lineWidth;
                    }
                    else
                    {
                        Debug.Log("no raycast hit");
                    }
                }
            }
            else if(touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
            {
                if (Physics.Raycast(ray.origin, ray.direction, out hit))
                {
                    if (hit.collider.name == "pfr")//TODO: add other reactors
                    {
                        Debug.Log("Hit a raycast on pfr!");
                        lineEndPoint = hit.collider.transform.position;

                        this.points = new Vector3[] { lineStartPoint, lineEndPoint };//put first point as first clicked object's center
                        line.positionCount = this.points.Length;
                        line.SetPositions(this.points);
                        line.startWidth = lineWidth;
                        line.endWidth = lineWidth;
                    }
                    else
                    {
                        Debug.Log("no raycast hit");
                    }
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