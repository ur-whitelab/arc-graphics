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
using Rochester.ARTable.Particles;
using System.Linq;

namespace Rochester.ARTable.Communication
{
    public class CommClient : MonoBehaviour
    {

        private Dictionary<string, Dictionary<int, GameObject>> managedObjects;
        private SubscriberSocket VisionClient, SimulationClient;
        private PairSocket ScreenshotServer;
        private NetMQPoller VisionPoller, SimulationPoller, ScreenshotPoller;
        private TaskCompletionSource<byte[]> VisionResponseTask, SimulationResponseTask;
        private TaskCompletionSource<string> ScreenshotResponseTask;//, ScreenshotStopResponseTask ;
        new private CameraControls camera;


        [Tooltip("Follows ZeroMQ syntax")]
        public string ServerUri = "tcp://127.0.0.1:8076";
        public string ScreenshotUri = "tcp://*:8079";

        public List<string> CommObjLabels;
        public List<GameObject> CommObjPrefabs;
        private Dictionary<string, GameObject> prefabs;
        public Renderer rend;

        private ParticleManager particleManager;


        private float delay;


        void Awake()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            DontDestroyOnLoad(gameObject);
        }

        // called second
        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            //get camera functions
            camera = GameObject.Find("Main Camera").GetComponent<CameraControls>();
            particleManager = null;
            if (scene.name == "detection")
            {
                particleManager = GameObject.Find("ParticleManager").GetComponent<ParticleManager>();
            }

            //clear objects if we had any
            if (scene.name != "default")
            {
                for (int i = 0; i < CommObjLabels.Count; i++)
                {
                    managedObjects[CommObjLabels[i]].Clear();
                }
            }
 
        }

        // Use this for initialization
        void Start()
        {

            //build prefab dict
            prefabs = new Dictionary<string, GameObject>();
            managedObjects = new Dictionary<string, Dictionary<int, GameObject>>();
            for(int i = 0; i < CommObjLabels.Count; i++) {
                prefabs[CommObjLabels[i]] = CommObjPrefabs[i];
                managedObjects[CommObjLabels[i]] = new Dictionary<int, GameObject>();
            }



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
            if (VisionResponseTask.Task.IsCompleted )
            {
                //UnityEngine.Debug.Log("THE MESSAGE TASK RESULT WAS " + VisionResponseTask.Task.Result);
                Graph system = Graph.Parser.ParseFrom(VisionResponseTask.Task.Result);
                UnityEngine.Debug.Log("Received message " +  system + " from graph.");
                synchronizeGraph(system);
                //this is how you reset?
                VisionResponseTask = new TaskCompletionSource<byte[]>();
            }

            if (SimulationResponseTask.Task.IsCompleted)
            {
                SystemKinetics kinetics = SystemKinetics.Parser.ParseFrom(SimulationResponseTask.Task.Result);
                // UnityEngine.Debug.Log("Received message " + kinetics + " from kinetics.");
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

        private void synchronizeGraph(Graph system)
        {
            foreach(var key in system.Nodes.Keys) {
                var o = system.Nodes[key];
                var currentObjs = managedObjects[o.Label];
                GameObject existing;
                Vector2 objectPos = new Vector2(o.Position[0], o.Position[1]);
                Vector2 viewPos = camera.UnitToWorld(objectPos);
                if (!currentObjs.TryGetValue(o.Id, out existing)) {
                    var placed = (GameObject) GameObject.Instantiate(prefabs[o.Label], new Vector2(viewPos.x, viewPos.y), new Quaternion());
                    currentObjs[o.Id] = placed;
                    UnityEngine.Debug.Log("New object " + o.Label + ":" + o.Id +" at position " + viewPos.x + ", " + viewPos.y + "(" + objectPos.x + ", " + objectPos.y + ")");
                }
                else if (o.Delete)
                {
                    currentObjs.TryGetValue(o.Id, out existing);
                    Destroy(existing);
                    currentObjs.Remove(o.Id);
                }
                else {
                    existing.transform.localPosition = viewPos;
                    // UnityEngine.Debug.Log("Moving object " + o.Label + ":" + o.Id + " to (" +viewPos.x + ", " + viewPos.y + ")");
                }
            }
            foreach(var key in system.Edges.Keys)
            {
                var edge = system.Edges[key];
                GameObject A, B;
                int IdA = edge.IdA;//first node
                int typeA = edge.TypeA;//index of node A type 
                int IdB = edge.IdB;//second node
                int typeB = edge.TypeB;//index of node B type


                //Protobuf gives ints, need string keys...
                string[] objkeys = managedObjects.Keys.ToArray();
                //Use keys to get the right dict(s) of gameobjects. Might be the same, that's ok.
                UnityEngine.Debug.Log("Type of A: " + objkeys[typeA] + " and type of B: " + objkeys[typeB]);
                A = managedObjects[(objkeys[typeA])][IdA];
                B = managedObjects[(objkeys[typeB])][IdB];
                //get the specific ones we're looking at
                //A = currentObjectsA[IdA];
                //B = currentObjectsB[IdB];

                UnityEngine.Debug.Log("Trying to draw line between GameObject type " + objkeys[typeA] + " at " + A.transform.position[0] + ", " + A.transform.position[1] + " and type " + objkeys[typeB] + " at " + B.transform.position[0] + ", " + B.transform.position[1] + ".");

                //now draw the line between them!
                LineRenderer line = A.GetComponent<LineRenderer>();
                if(line == null)
                {
                    //line doesn't yet exist. Attach a linerenderer to A
                    line = A.AddComponent<LineRenderer>();
                }
                //add the origin and the destination to its vertices.
                int oldPositionCount = line.positionCount;
                line.positionCount += 2;
                line.SetPosition(oldPositionCount, A.transform.position);
                line.SetPosition(oldPositionCount + 1, B.transform.position);

            }
        }

        private void synchronizeSimulation(SystemKinetics kinetics)
        {
            int rxrcount = 1;//the kinetics protobuffer could stand to be changed...
            int count;
            float sum;
            foreach(var rxr in kinetics.Kinetics)
            {

                var currentObjs = managedObjects["reactor"];
                GameObject existing;
                currentObjs.TryGetValue( rxrcount, out existing);
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
                    for (int i = 0; i < count; i++)
                    {
                        rend.material.SetFloat("_Fraction" + (i + 1).ToString(), value: (rxr.MoleFraction[i] / sum));
                    }
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
