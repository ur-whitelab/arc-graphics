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

namespace Rochester.ARTable.Communication
{
    public class CommClient : MonoBehaviour
    {
        
        private Dictionary<string, Dictionary<int, GameObject>> managedObjects;
        private SubscriberSocket VisionClient, SimulationClient;
        private NetMQPoller VisionPoller, SimulationPoller;
        private TaskCompletionSource<byte[]> VisionResponseTask, SimulationResponseTask ;
        private CameraControls camera;


        [Tooltip("Follows ZeroMQ syntax")]
        public string server_uri = "tcp://127.0.0.1:5000";

        public List<string> CommObjLabels;
        public List<GameObject> CommObjPrefabs;
        private Dictionary<string, GameObject> prefabs;
        public Renderer rend;


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
            VisionClient.Subscribe("vision-update");//vision-update, but "any" for testing
            SimulationClient.Subscribe("simulation-update");
            UnityEngine.Debug.Log("set up the subscriptions at " + server_uri);
            VisionClient.Connect(server_uri);
            SimulationClient.Connect(server_uri);
            VisionPoller = new NetMQPoller { VisionClient };//, SimulationClient };
            SimulationPoller = new NetMQPoller { SimulationClient };
            //set-up event to add to task
            VisionResponseTask = new TaskCompletionSource<byte[]>();
            SimulationResponseTask = new TaskCompletionSource<byte[]>();
            SimulationClient.ReceiveReady += (s, a) => {
                var msg = a.Socket.ReceiveMultipartBytes();

                SimulationResponseTask.TrySetResult(msg[1]);

            };
            VisionClient.ReceiveReady += (z, b) => {
                var msg = b.Socket.ReceiveMultipartBytes();
                VisionResponseTask.TrySetResult(msg[1]);
            };
            //start polling thread
            VisionPoller.RunAsync();
            SimulationPoller.RunAsync();
            
        }   

        // Update is called once per frame
        void Update()
        {
            if (VisionResponseTask.Task.IsCompleted )
            {
                //UnityEngine.Debug.Log("THE MESSAGE TASK RESULT WAS " + VisionResponseTask.Task.Result);
                Graph system = Graph.Parser.ParseFrom(VisionResponseTask.Task.Result);
                // UnityEngine.Debug.Log("Received message " +  system + " from graph.");
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
            
        }

        private void synchronizeGraph(Graph system)
        {
            foreach(var key in system.Nodes.Keys) {
                var o = system.Nodes[key];
                var currentObjs = managedObjects[o.Label];
                GameObject existing;
                Vector2 objectPos = new Vector2(1-o.Position[0], 1-o.Position[1]);
                Vector2 viewPos = camera.UnitToWorld(objectPos);
                if (!currentObjs.TryGetValue(o.Id, out existing)) {
                    var placed = (GameObject) GameObject.Instantiate(prefabs[o.Label], new Vector2(viewPos.x, viewPos.y), new Quaternion()); 
                    currentObjs[o.Id] = placed;
                    UnityEngine.Debug.Log("New object " + o.Label + ":" + o.Id);
                }
                else if (o.Delete)
                {
                    currentObjs.TryGetValue(o.Id, out existing);
                    Destroy(existing);
                }
                else {
                    existing.transform.localPosition = viewPos;                    
                    // UnityEngine.Debug.Log("Moving object " + o.Label + ":" + o.Id + " to (" +viewPos.x + ", " + viewPos.y + ")"); 
                }
                if(system.Time % 100 == 0)
                {
                    camera.TakeHiResShot();
                }
            }            
        }

        private void synchronizeSimulation(SystemKinetics kinetics)
        {
            int rxrcount = 0;//the kinetics protobuffer could stand to be changed...
            int count;
            foreach(var rxr in kinetics.Kinetics)
            {
                
                var currentObjs = managedObjects["1"];//hard-coded reactor type.
                GameObject existing;
                currentObjs.TryGetValue( rxrcount, out existing);
                Renderer rend = existing.GetComponent<Renderer>();
                rend.material.shader = Shader.Find("Custom/WedgeCircle");
                count = 0;
                foreach(var molefrac in rxr.MoleFraction)
                {
                    count++;
                }
                for(int i = 0; i < count; i++)
                {
                    rend.material.SetFloat("_Fraction" + (i + 1).ToString(), rxr.MoleFraction[i]);
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
            try
            {
                VisionPoller.StopAsync();
                SimulationPoller.StopAsync();
            }
                
            catch (System.Exception e)
            {
                UnityEngine.Debug.Log("Tried to stopasync while the poller wasn't running! Oops.");
            }
            VisionPoller.Dispose();
            SimulationPoller.Dispose();
        }
    }

}
