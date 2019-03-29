using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;
using Rochester.Physics.Communication;
#if UNITY_EDITOR
// Set up touch input propagation while using Instant Preview in the editor.
using Input = GoogleARCore.InstantPreviewInput;
using Rochester.ARTable.Structures;
#endif


public class TouchManager : MonoBehaviour
{

    public GameObject gameObject;
    //public GameObject camera;
    public List<GameObject> Points = new List<GameObject>();
    private Camera camera;
    private float lineWidth;
    private Vector3 lineStartPoint;
    private Vector3 lineEndPoint;
    private List<Vector3> points;
    //private Graph system;
    private GraphManager system;
    private int firstNodeId;
    private int secondNodeId;

    // Use this for initialization
    void Start()
    {
        camera = Camera.main;
        lineWidth = 0.05f;
        points = new List<Vector3>();
        system = new GraphManager();
    }

    // Update is called once per frame
    void Update()
    {
        LineRenderer line = this.GetComponent<LineRenderer>();
        if (Input.GetMouseButton(0) || Input.touchCount > 0)
        {
            Touch touch;
            if (Input.touches.Length > 0)
            {
                touch = Input.GetTouch(0);
                RaycastHit hit;
                Ray ray = camera.ScreenPointToRay(touch.position);
                Vector3 targetPoint = new Vector3(touch.position.x, touch.position.y, camera.nearClipPlane);
                if (touch.phase == TouchPhase.Began)
                {
                    if (Physics.Raycast(ray.origin, ray.direction, out hit))
                    {
                        if (hit.collider.name == "pfr")
                        {
                            Debug.Log("Hit a raycast on pfr!");
                            lineStartPoint = hit.collider.transform.position;
                            points.Add(lineStartPoint);//append the start point
                            Debug.Log("in Began phase, line start point is " + lineStartPoint);
                            Reactor thisRxr = hit.collider.GetComponent< Reactor >();
                            firstNodeId = thisRxr.Id;
                            if (!system.idExists(firstNodeId))
                            {
                                system.AddNode(firstNodeId, hit.collider.name);
                            }
                        }
                        else
                        {
                            Debug.Log("no pfr hit");
                        }
                    }
                }
                else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                {/*
                    Debug.Log("Touch phase: " + touch.phase);
                    if (Physics.Raycast(ray.origin, ray.direction, out hit))
                    {
                        if (hit.collider.name == "pfr")//TODO: add other reactors
                        {
                            Debug.Log("Hit a raycast on pfr!");
                            //lineEndPoint = hit.collider.transform.position;
                            //this.points = new Vector3[] { lineStartPoint, lineEndPoint };//put first point as first clicked object's center
                            line.positionCount = this.points.Length;
                            line.SetPositions(this.points);
                            line.startWidth = lineWidth;
                            line.endWidth = lineWidth;
                        }
                        else
                        {
                            Debug.Log("no raycast hit");
                        }
                    }*/
                }
                else if (touch.phase == TouchPhase.Ended)
                {
                    if (Physics.Raycast(ray.origin, ray.direction, out hit))
                    {
                        if (hit.collider.name == "pfr")//TODO: add other reactors
                        {
                            Debug.Log("Hit a raycast on pfr!");
                            lineEndPoint = hit.collider.transform.position;
                            while(points.Count >= 2)
                            {
                                points.RemoveAt(0);//remove oldest point(s)
                            }
                            points.Add(lineEndPoint);
                            Reactor thisRxr = hit.collider.GetComponent<Reactor>();
                            secondNodeId = thisRxr.Id;
                            if (!system.idExists(secondNodeId))
                            {
                                system.AddNode(secondNodeId, hit.collider.name);
                            }
                            if(firstNodeId != secondNodeId)
                            {
                                system.ConnectById(firstNodeId, secondNodeId);
                            }
                            Debug.Log("In Ended phase, line points are: " + lineStartPoint + ", " + lineEndPoint);
                            line.positionCount = 2;
                            Vector3[] input = new Vector3[] { this.points[0], this.points[1] };
                            line.SetPositions(input);
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
}