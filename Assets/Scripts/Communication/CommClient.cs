using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetMQ;
using Google.Protobuf;
using NetMQ.Sockets;
using System.Threading.Tasks;
using Rochester.Physics.Communication;
using Rochester.ARTable.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Rochester.ARTable.Particles;
using System.Linq;

namespace Rochester.ARTable.Communication
{
    public class CommClient : MonoBehaviour
    {

        private Dictionary<string, Dictionary<int, GameObject>> managedObjects;
        private Dictionary<int, List<int>> edgeList;//stores OUTGOING edges of each node. index is the same as node A, then list of ints is all B's that node A goes TO.

        private Dictionary<int, Dictionary<int, GameObject>> managedLines;//the actual line gameobjects; since each node has unique index, can get a unique line this way.
        private SubscriberSocket VisionClient, SimulationClient;
        private PairSocket ScreenshotServer;
        private NetMQPoller VisionPoller, SimulationPoller, ScreenshotPoller;
        private TaskCompletionSource<byte[]> VisionResponseTask, SimulationResponseTask;
        private TaskCompletionSource<string> ScreenshotResponseTask;//, ScreenshotStopResponseTask ;
        new private CameraControls camera;

        private GameObject temperatureValue;
        private string temperatureText;
        private GameObject timeValue;
        private string timeText;

        private GameObject backend;


        [Tooltip("Follows ZeroMQ syntax")]
        public string ServerUri = "tcp://127.0.0.1:8076";
        public string ScreenshotUri = "tcp://*:8079";

        public List<string> CommObjLabels;
        public List<GameObject> CommObjPrefabs;
        private Dictionary<string, GameObject> prefabs;
        public Renderer rend;
        public bool training;

        private ParticleManager particleManager;
        private Material linemat;

        private float delay;
        private bool calibrating;

        void Awake()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            DontDestroyOnLoad(gameObject);
            calibrating = false;
        }

        // called second
        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            //get camera functions
            camera = GameObject.Find("Main Camera").GetComponent<CameraControls>();
            particleManager = null;
            if (scene.name == "detection" || scene.name == "darkflow")
            {
                particleManager = GameObject.Find("ParticleManager").GetComponent<ParticleManager>();
            }

            backend = GameObject.Find("Backend");
            DontDestroyOnLoad(backend);
            if(scene.name == "calibration")
            {
                calibrating = true;
                backend.transform.Find("ColorKey").gameObject.SetActive(false);
            }
            else
            {
                calibrating = false;
                backend.transform.Find("ColorKey").gameObject.SetActive(true);
            }
            //clear objects if we had any
            if (scene.name != "default")
            {
                for (int i = 0; i < CommObjLabels.Count; i++)
                {
                    managedObjects[CommObjLabels[i]].Clear();
                }
                foreach (var key in edgeList.Keys){
                    edgeList[key].Clear();
                }
                foreach(var key in managedLines.Keys){
                    managedLines[key].Clear();
                }
            }

        }

        // Use this for initialization
        void Start()
        {
            //these are the fixed text objects that display the next-placed reactor's temperature and pressure
            temperatureValue = GameObject.Find("TemperatureValue");
            //build prefab and edge list dicts
            prefabs = new Dictionary<string, GameObject>();
            managedObjects = new Dictionary<string, Dictionary<int, GameObject>>();
            managedLines = new Dictionary<int, Dictionary<int, GameObject>>();
            edgeList = new Dictionary<int, List<int>>();
            for (int i = 0; i < CommObjLabels.Count; i++) {
                prefabs[CommObjLabels[i]] = CommObjPrefabs[i];
                managedObjects[CommObjLabels[i]] = new Dictionary<int, GameObject>();
            }
            managedObjects["source"][0] = GameObject.Find("source");
            //For rendering lines
            linemat = Resources.Load("ReactorLineMaterial", typeof(Material)) as Material;
            //set-up socket and poller
            VisionClient = new SubscriberSocket();
            SimulationClient = new SubscriberSocket();
            ScreenshotServer = new PairSocket();
            VisionClient.Subscribe("vision-update");
            SimulationClient.Subscribe("simulation-update");
            UnityEngine.Debug.Log("set up the subscriptions at " + ServerUri);
            VisionClient.Connect(ServerUri);
            SimulationClient.Connect(ServerUri);
            ScreenshotServer.Bind(ScreenshotUri);
            Debug.Log("set up rep server at " + ScreenshotUri);
            VisionPoller = new NetMQPoller { VisionClient };//, SimulationClient };
            SimulationPoller = new NetMQPoller { SimulationClient };
            ScreenshotPoller = new NetMQPoller { ScreenshotServer };
            //set-up event to add to task
            VisionResponseTask = new TaskCompletionSource<byte[]>();
            SimulationResponseTask = new TaskCompletionSource<byte[]>();
            ScreenshotResponseTask= new TaskCompletionSource<string>();
            //ScreenshotStopResponseTask= new TaskCompletionSource<string[]>();
            SimulationClient.ReceiveReady += (s, a) => {
                var msg = a.Socket.ReceiveMultipartBytes();

                while(!SimulationResponseTask.TrySetResult(msg[1]));

            };
            VisionClient.ReceiveReady += (z, b) => {
                var msg = b.Socket.ReceiveMultipartBytes();
                while(!VisionResponseTask.TrySetResult(msg[1]));
            };
            ScreenshotServer.ReceiveReady += (s, a) =>
            {
                var msg = a.Socket.ReceiveFrameString();
                while (!ScreenshotResponseTask.TrySetResult(msg)) ;
            };
            //start polling thread
            VisionPoller.RunAsync();
            SimulationPoller.RunAsync();
            ScreenshotPoller.RunAsync();
        }

        // Update is called once per frame
        void Update()
        {
            if(Input.GetKeyDown("l"))
            {
                training = !training;
                if (training)
                {
                    backend.transform.Find("ColorKey").gameObject.SetActive(false);
                }
                else
                {
                    backend.transform.Find("ColorKey").gameObject.SetActive(true);
                }
            }
            if(Input.GetKeyDown(KeyCode.Backspace))
            {
                List<KeyValuePair<int, string>> toDelete = new List<KeyValuePair<int, string>>();
                foreach(var label in managedObjects.Keys){
                    foreach(int id in managedObjects[label].Keys){
                        toDelete.Add(new KeyValuePair<int,string>(id, label));
                    }
                }
                foreach(var pair in toDelete){
                    deleteObject(pair.Key, pair.Value);
                }
            }
            if (VisionResponseTask.Task.IsCompleted )
            {
                Graph system = Graph.Parser.ParseFrom(VisionResponseTask.Task.Result);
                synchronizeGraph(system);
                //this is how you reset?
                VisionResponseTask = new TaskCompletionSource<byte[]>();
            }

            if (SimulationResponseTask.Task.IsCompleted && !calibrating)
            {
                SystemKinetics kinetics = SystemKinetics.Parser.ParseFrom(SimulationResponseTask.Task.Result);
                synchronizeSimulation(kinetics);
                SimulationResponseTask = new TaskCompletionSource<byte[]>();
            }

            if(ScreenshotResponseTask.Task.IsCompleted)
            {
                // resolution is message
                string[] words = ScreenshotResponseTask.Task.Result.Split('-');
                if (camera != null)
                {
                    camera.ScreenshotResolution = new int[] { int.Parse(words[0]), int.Parse(words[1]) };
                    camera.TakeScreenshot += sendScreenshot;
                } else
                {
                    ScreenshotResponseTask = new TaskCompletionSource<string>();
                }
            }

        }


        private void sendScreenshot(object sender, CameraScreenshotModifierEventArgs e) {
            camera.TakeScreenshot -= sendScreenshot;
            ScreenshotServer.SendFrame(e.jpg);
            ScreenshotResponseTask = new TaskCompletionSource<string>();
        }


        private void deleteObject(int Id, string label){
            GameObject existing;
            var currentObjs = getManagedObjects(label);
            currentObjs.TryGetValue(Id, out existing);
            Destroy(existing);
            //scan dict of line gameobjects for the lines attached to this node, destroy them.
            foreach (var idx1 in managedLines.Keys)
            {
                if(managedLines[idx1].ContainsKey(Id)){
                    Destroy(managedLines[idx1][Id]);//destroy all lines that go TO deleted node
                    managedLines[idx1].Remove(Id);//take that key out of this dict.
                }
            }
            if(managedLines.ContainsKey(Id))
                {
                    foreach(var idx2 in managedLines[Id].Keys)
                    {
                        Destroy(managedLines[Id][idx2]);//destroy all lines that go FROM deleted node
                    }
                    managedLines.Remove(Id);
                }
            currentObjs.Remove(Id);
        }

        private Dictionary<int, GameObject> getManagedObjects(string label){
            if (label == "cstr" || label == "pfr")//Unity display doesn't care about reactor type, both use the "reactor" prefab.
            {
                label = "reactor";
            }
            return managedObjects[label];
        }
        private void synchronizeGraph(Graph system)
        {
            foreach(var key in system.Nodes.Keys)
            {
                var o = system.Nodes[key];

                string label = o.Label;
                if (label == "conditions" && GameObject.Find("Backend/ColorKey/TemperatureValue") != null)//temperature and pressure updates are passed as a special 'node'
                {
                    temperatureValue = GameObject.Find("Backend/ColorKey/TemperatureValue");
                    temperatureValue.GetComponent<Text>().text = "" + o.Weight[0] + " K";
                }
                else
                {
                    if (label == "cstr" || label == "pfr")//Unity display doesn't care about reactor type, both use the "reactor" prefab.
                    {
                        label = "reactor";
                    }
                    var currentObjs = getManagedObjects(o.Label);
                    GameObject existing;
                    Vector2 objectPos = new Vector2(o.Position[0], o.Position[1]);
                    Vector2 viewPos = camera.UnitToWorld(objectPos);
                    if (!currentObjs.TryGetValue(o.Id, out existing) && !o.Delete)
                    {
                        var placed = (GameObject)GameObject.Instantiate(prefabs[label], new Vector2(viewPos.x, viewPos.y), new Quaternion());
                        if(label == "reactor")
                        {
                            Renderer rend = placed.GetComponent<MeshRenderer>();
                            rend.material.SetFloat("_Temperature", value: float.Parse(temperatureValue.GetComponent<Text>().text.Split(" "[0])[0]));
                        }
                        currentObjs[o.Id] = placed;
                    }
                    else if (o.Delete)
                    {
                        deleteObject(o.Id, label);
                    }
                    else
                    {
                        double dist = Mathf.Sqrt(Mathf.Pow(Mathf.Abs(viewPos[0] - currentObjs[o.Id].transform.position[0]), 2) + Mathf.Pow(Mathf.Abs(viewPos[1] - currentObjs[o.Id].transform.position[1]), 2));
                            existing.transform.localPosition = viewPos;
                    }
                }

            }

            //first we build the edge list up
            int numEdges = system.Edges.Count;
            GameObject A, B;
            for (int i = 0; i < numEdges; i++)
            {
                var edge = system.Edges[i];
                int IdA = edge.IdA;//first node
                string labelA = edge.LabelA;//index of node A type
                int IdB = edge.IdB;//second node
                string labelB = edge.LabelB;//index of node B type
                if (labelA == "cstr" || labelA == "pfr")//Unity display doesn't care about reactor type, both use the "reactor" prefab.
                {
                    labelA = "reactor";
                }
                if (labelB == "cstr" || labelB == "pfr")//Unity display doesn't care about reactor type, both use the "reactor" prefab.
                {
                    labelB = "reactor";
                }
                //Use keys to get the right dict(s) of gameobjects. Might be the same, that's ok.
                if (IdA == 0)
                {
                    A = GameObject.Find("source");
                }
                else
                {
                    if(managedObjects[labelA].ContainsKey(IdA))
                    {
                        A = managedObjects[labelA][IdA];
                    }
                    else
                    {
                        Debug.Log("Got an edge FROM a node that doesn't exist: " + labelA + " " + IdA);
                    }
                }
                if(managedObjects[labelB].ContainsKey(IdB))
                {
                    B = managedObjects[labelB][IdB];
                }
                else
                {
                    Debug.Log("Got an edge TO a node that doesn't exist: " + labelB + " " + IdB);
                }



                if (edgeList.ContainsKey(IdA))
                {
                    edgeList[IdA].Add(IdB);
                }
                else
                {
                    edgeList[IdA] = new List<int>(IdB);
                }

                if(!managedLines.ContainsKey(IdA))
                {
                    managedLines[IdA] =  new Dictionary<int, GameObject>();
                }
                if(!managedLines[IdA].ContainsKey(IdB))
                {
                    managedLines[IdA][IdB] = new GameObject();
                }

            }
            string BLabel;
            int BIndex;
            Vector3 AtoB;
            float leftBoxSize;//size of the gameObject A (can sometimes be "source" node)
            float rightBoxSize;//size of the gameObject B
            if(edgeList.Keys.Count > 0) {
                foreach (var IdA in edgeList.Keys)//iterate through nodes with OUTGOING edges
                {
                    foreach (int IdB in edgeList[IdA])//iterate through all nodes this node has edges TO
                    {
                        if(managedObjects["reactor"].ContainsKey(IdA) || IdA == 0)
                        {
                            if(IdA != 0)//special case: "source" always has ID 0
                            {
                                A = managedObjects["reactor"][IdA];//only reactors (and source) get edges between them
                            }
                            else
                            {
                                A = GameObject.Find("source");
                            }
                            if(!managedLines.ContainsKey(IdA))
                            {
                                managedLines[IdA] = new Dictionary<int, GameObject>();
                            }
                            if(managedLines[IdA].ContainsKey(IdB))
                            {
                                if(managedLines[IdA][IdB] != null)
                                {
                                    GameObject line = managedLines[IdA][IdB];//get the gameobject we're using for this line
                                    LineRenderer renderer = line.GetComponent<LineRenderer>();
                                    if (renderer == null)
                                    {
                                        //renderer doesn't yet exist. Attach a linerenderer
                                        renderer = line.AddComponent<LineRenderer>();
                                        renderer.positionCount = 2;//Need only 2 line coords each.
                                        renderer.material = linemat;
                                        renderer.startColor = new Color(110f/255f,110f/255f,200f/255f,0.5f);
                                        renderer.endColor = renderer.startColor;
                                    }
                                    if(managedObjects["reactor"].ContainsKey(IdB)){
                                        B = managedObjects["reactor"][IdB];

                                        AtoB = B.transform.position - A.transform.position;//get vector between A and B
                                        //get offset based on box size (so lines don't *quite* hit reactors)
                                        leftBoxSize = A.GetComponent<Renderer>().bounds.size.x;
                                        rightBoxSize = B.GetComponent<Renderer>().bounds.size.x;
                                        //now draw the line between them!
                                        renderer.SetPosition(0, A.transform.position + (AtoB * leftBoxSize / (float)1.5)/(AtoB.magnitude));//this 1/1.5 prefactor is just an empirically-found scaling that works well
                                        renderer.SetPosition(1, B.transform.position - (AtoB * rightBoxSize / (float)1.5)/(AtoB.magnitude));//  to give space between end of lines and reactors
                                    }
                                }
                                else//key is there but gameobject isn't. Re-draw line.
                                {
                                    managedLines[IdA][IdB] = new GameObject();
                                    GameObject line = managedLines[IdA][IdB];
                                    LineRenderer renderer = line.GetComponent<LineRenderer>();
                                    if (renderer == null)
                                    {
                                        //renderer doesn't yet exist. Attach a linerenderer
                                        renderer = line.AddComponent<LineRenderer>();
                                        renderer.positionCount = 2;//Need only 2 line coords each.
                                        renderer.startColor = Color.white;
                                        renderer.endColor = Color.white;
                                        renderer.material = linemat;
                                    }
                                    if(managedObjects["reactor"].ContainsKey(IdB)){
                                        B = managedObjects["reactor"][IdB];

                                        AtoB = B.transform.position - A.transform.position;//get vector between A and B
                                        //get offset based on box size (so lines don't *quite* hit reactors)
                                        leftBoxSize = A.GetComponent<Renderer>().bounds.size.x;
                                        rightBoxSize = B.GetComponent<Renderer>().bounds.size.x;
                                        //now draw the line between them!
                                        renderer.SetPosition(0, A.transform.position + (AtoB * leftBoxSize / (float)1.5)/(AtoB.magnitude));//this 1/1.5 prefactor is just an empirically-found scaling that works well
                                        renderer.SetPosition(1, B.transform.position - (AtoB * rightBoxSize / (float)1.5)/(AtoB.magnitude));//  to give space between end of lines and reactors
                                    }
                                }

                            }

                        }

                    }

                }
            }

        }

        private void synchronizeSimulation(SystemKinetics kinetics)
        {
            int rxrcount = 1;//start at 1 because "source" has special ID 0
            int count;
            float sum;
            float system_time = (float)kinetics.Time / (float) 3.25;//divide by 3.25 to go from FPS to accelerated seconds
            timeValue = GameObject.Find("Backend/ColorKey/TimeValue");
            timeValue.GetComponent<Text>().text = System.String.Format("{0:0.00} s", system_time);
            foreach(var rxr in kinetics.Kinetics)
            {
                var currentObjs = managedObjects["reactor"];
                GameObject existing;
                currentObjs.TryGetValue( rxr.Id, out existing);
                if(existing)
                {
                    rend = existing.GetComponent<MeshRenderer>();
                    rend.material.shader = Shader.Find("Custom/WedgeCircle");
                    count = 0;
                    sum = 0;
                    foreach (var molefrac in rxr.MoleFraction)
                    {
                        count++;
                        sum += molefrac;
                    }
                    if(sum > 0)
                    {
                        rend.material.SetInt("_NumWedges", value:count);
                        for (int i = 0; i < count; i++)
                        {
                        rend.material.SetFloat("_Fraction" + (i + 1).ToString(), value: (rxr.MoleFraction[i]));
                        }
                    }
                    else
                    {
                        rend.material.SetInt("_NumWedges", value:5);
                        for (int i = 0; i < count; i++)
                        {
                            rend.material.SetFloat("_Fraction" + (i + 1).ToString(), value: ((float)0));
                        }
                        rend.material.SetFloat("_Fraction5", value: ((float)1));
                    }
                }
                else
                {
                    Debug.Log("Got a reactor that doesn't exist! rxrcount = " + rxrcount +", rxr ID = " + rxr.Id);
                }

                rxrcount++;
            }
        }

        void OnApplicationQuit()//cleanup
        {
            VisionClient.Close();
            VisionClient.Dispose();
            SimulationClient.Close();
            SimulationClient.Dispose();
            ScreenshotServer.Close();
            ScreenshotServer.Dispose();
            try
            {
                VisionPoller.StopAsync();
                SimulationPoller.StopAsync();
                ScreenshotPoller.StopAsync();
            }

            catch (System.Exception e)
            {
                UnityEngine.Debug.Log("Tried to stopasync while the poller wasn't running! Oops.");
            }
            VisionPoller.Dispose();
            SimulationPoller.Dispose();
            ScreenshotPoller.Dispose();
        }
    }

}
