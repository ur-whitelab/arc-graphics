using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetMQ;
using Google.Protobuf;
using NetMQ.Sockets;
using System.Threading.Tasks;

namespace Rochester.ARTable.Communication
{
    public class CommClient : MonoBehaviour
    {
        
        private Dictionary<int, Dictionary<int, GameObject>> managedObjects;
        private SubscriberSocket VisionClient, SimulationClient;
        private NetMQPoller poller;
        private TaskCompletionSource<byte[]> VisionResponseTask, SimulationResponseTask;

        [Tooltip("Follows ZeroMQ syntax")]
        public string server_uri = "tcp://127.0.0.10:5000";

        public List<int> CommObjIds;
        public List<GameObject> CommObjPrefabs;
        private Dictionary<int, GameObject> prefabs;

        // Use this for initialization
        void Start()
        {

            //build prefab dict
            prefabs = new Dictionary<int, GameObject>();
            managedObjects = new Dictionary<int, Dictionary<int, GameObject>>();
            for(int i = 0; i < CommObjIds.Count; i++) {
                prefabs[CommObjIds[i]] = CommObjPrefabs[i];
                managedObjects[CommObjIds[i]] = new Dictionary<int, GameObject>();
            }

            

            //set-up socket and poller            
            AsyncIO.ForceDotNet.Force();
            VisionClient = new SubscriberSocket();
            SimulationClient = new SubscriberSocket();
            VisionClient.Subscribe("");//vision-update, but "any" for testing
            SimulationClient.Subscribe("simulation-update");
            UnityEngine.Debug.Log("set up the subscriptions at " + server_uri);
            VisionClient.Connect(server_uri);
            SimulationClient.Connect(server_uri);
            poller = new NetMQPoller { VisionClient, SimulationClient };
            //set-up event to add to task
            VisionResponseTask = new TaskCompletionSource<byte[]>();
            SimulationResponseTask = new TaskCompletionSource<byte[]>();
            SimulationClient.ReceiveReady += (s, a) => {               
                SimulationResponseTask.SetResult(a.Socket.ReceiveFrameBytes());
            };
            VisionResponseTask = new TaskCompletionSource<byte[]>();
            VisionClient.ReceiveReady += (s, a) => {
                VisionResponseTask.SetResult(a.Socket.ReceiveFrameBytes());
            };
            //start polling thread
            poller.RunAsync();
            
        }   

        // Update is called once per frame
        void Update()
        {
            if (VisionResponseTask.Task.IsCompleted)
            {
                StructuresState state = StructuresState.Parser.ParseFrom(VisionResponseTask.Task.Result);
                UnityEngine.Debug.Log("Received message " +  state.Time + "from vision-update");
                synchronizeState(state);
                //this is how you reset?
                VisionResponseTask = new TaskCompletionSource<byte[]>();
            }


            
        }

        private void synchronizeState(StructuresState state)
        {
            foreach(var o in state.Structures) {
                var currentObjs = managedObjects[o.Type];
                GameObject existing;
                if(!currentObjs.TryGetValue(o.Id, out existing)) {
                   var placed = (GameObject) GameObject.Instantiate(prefabs[o.Type], new Vector2(o.Position[0], o.Position[1]), new Quaternion()); 
                   currentObjs[o.Id] = placed;
                   UnityEngine.Debug.Log("New object " + o.Type + ":" + o.Id);
                } else {
                    existing.transform.localPosition = new Vector2(o.Position[0], o.Position[1]);                    
                    UnityEngine.Debug.Log("Moving object " + o.Type + ":" + o.Id + " to (" + o.Position[0] + ", " + o.Position[1] + ")"); 
                }
            }            
        }

        void OnApplicationQuit()//cleanup
        {
            VisionClient.Close();
            VisionClient.Dispose();
            SimulationClient.Close();
            SimulationClient.Dispose();
            poller.StopAsync();
            poller.Dispose();
            NetMQConfig.Cleanup();
        }
    }

}
