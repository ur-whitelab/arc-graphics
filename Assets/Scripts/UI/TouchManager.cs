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
    private Camera camera;
    private float lineWidth;
    private Vector3 lineStartPoint;
    private Vector3 lineEndPoint;
    //private Graph system;
    private GraphManager system;
    private int firstNodeId;
    private int secondNodeId;
    private Reactor selectedReactor;

    // Use this for initialization
    void Start()
    {
        camera = Camera.main;
        lineWidth = 0.05f;
        system = new GraphManager();
        selectedReactor = null;
        firstNodeId = -1;
    }

    // Update is called once per frame
    void Update()
    {
        LineRenderer line = this.GetComponent<LineRenderer>();
        if (Input.touchCount > 0)
        {
            foreach (Touch touch in Input.touches)
            {
                RaycastHit hit;
                Ray ray = camera.ScreenPointToRay(touch.position);
                Color color;
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        color = Color.green;
                        Debug.DrawRay(ray.origin, ray.direction * 20, color, 100.0f);
                        if (Physics.Raycast(ray.origin, ray.direction, out hit))
                        {
                            if (hit.collider.name == "pfr")
                            {
                                lineStartPoint = hit.collider.transform.position;
                                Debug.Log("in Began phase, line start point is " + lineStartPoint);
                                Reactor hitRxr = hit.collider.GetComponent<Reactor>();
                                firstNodeId = hitRxr.Id;
                                if (!system.idExists(firstNodeId))
                                {
                                    system.AddNode(firstNodeId, hit.collider.name);
                                }
                            }
                        }
                        break;
                    case TouchPhase.Moved:
                        if (Physics.Raycast(ray.origin, ray.direction, out hit))
                        {
                            if (hit.collider.name == "pfr")
                            {
                                if(lineStartPoint == Vector3.zero)
                                {
                                    lineStartPoint = hit.collider.transform.position;
                                }
                                else
                                {
                                    lineEndPoint = hit.collider.transform.position;
                                }
                                Reactor hitRxr = hit.collider.GetComponent<Reactor>();
                                if(firstNodeId == -1)
                                {
                                    firstNodeId = hitRxr.Id;
                                }
                                if (!system.idExists(firstNodeId) && firstNodeId != -1)
                                {
                                    system.AddNode(firstNodeId, hit.collider.name);
                                }
                            }
                        }
                        break;
                    case TouchPhase.Stationary:
                        if (Physics.Raycast(ray.origin, ray.direction, out hit))
                        {
                            if (hit.collider.name == "pfr")
                            {
                                if (lineStartPoint == Vector3.zero)
                                {
                                    lineStartPoint = hit.collider.transform.position;
                                }
                                else
                                {
                                    lineEndPoint = hit.collider.transform.position;
                                }
                                Reactor hitRxr = hit.collider.GetComponent<Reactor>();
                                if (firstNodeId == -1)
                                {
                                    firstNodeId = hitRxr.Id;
                                }
                                if (!system.idExists(firstNodeId) && firstNodeId != -1)
                                {
                                    system.AddNode(firstNodeId, hit.collider.name);
                                }
                            }
                        }
                        break;
                    case TouchPhase.Ended:
                        color = Color.red;
                        Debug.DrawRay(ray.origin, ray.direction * 20, Color.cyan, 100.0f);
                        if (Physics.Raycast(ray.origin, ray.direction, out hit))//TODO: this logic  will break if we put a game plane underneath everything
                        {
                            if (hit.collider.name == "pfr")//TODO: add other reactors
                            {
                                lineEndPoint = hit.collider.transform.position;
                                Reactor hitRxr = hit.collider.GetComponent<Reactor>();
                                secondNodeId = hitRxr.Id;
                                if (!system.idExists(secondNodeId))
                                {
                                    system.AddNode(secondNodeId, hit.collider.name);
                                }
                                if (firstNodeId != secondNodeId && firstNodeId != -1)
                                {
                                    system.ConnectById(firstNodeId, secondNodeId);
                                }
                                if(firstNodeId == -1 || firstNodeId == secondNodeId)
                                {
                                    hitRxr.toggleHighlight();//if not dragging, toggle tapped rxr
                                    if(selectedReactor != null && hitRxr != selectedReactor && selectedReactor.getSelected())
                                    {
                                        selectedReactor.toggleHighlight();
                                    }
                                    selectedReactor = hitRxr;
                                }
                                else if(selectedReactor != null && selectedReactor.getSelected())
                                {
                                    selectedReactor.toggleHighlight();//if dragging, toggle previously selected rxr
                                }
                                Debug.Log("In Ended phase, line points are: " + lineStartPoint + ", " + lineEndPoint);
                            }
                        }
                        else//didn't hit a reactor, so turn off selected one if it was on
                        {
                            if (selectedReactor != null && selectedReactor.getSelected())
                            {
                                selectedReactor.toggleHighlight();
                            }
                        }

                        //Even if we didn't get a raycast hit, draw a line between the last two rxrs we swiped through...

                        if (lineStartPoint != Vector3.zero)
                        {
                            line.positionCount = 2;
                            Vector3[] input = new Vector3[] { lineStartPoint, lineEndPoint };
                            line.SetPositions(input);
                            line.startWidth = lineWidth;
                            line.endWidth = lineWidth;
                        }
                        else
                        {
                            line.positionCount = 2;
                            line.SetPositions(new Vector3[] { Vector3.zero, Vector3.zero });

                        }
                        //reset
                        lineStartPoint = Vector3.zero;
                        firstNodeId = -1;
                        break;
                }
                /*if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                {
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
                    }
                }*/
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