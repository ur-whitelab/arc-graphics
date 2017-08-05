using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetMQ;
using Google.Protobuf;
using NetMQ.Sockets;
using System.Threading.Tasks;

//TODO: Put namespaces everywhere, add two frames to messages. One to indicate
//.Type, one for content. Switch to pub/sub model. Write python server. Should talk to bluehive
//and unity. Bluehive will also pub/sub for unity and possible monitors/multiple table.s
//have an issue with velocity update in source object. should investigate. Maybe not disposing buffer..?

namespace Rochester.ARTable.Communication
{
    public class CommClient : MonoBehaviour
    {
        
        private Dictionary<int, Dictionary<int, GameObject>> managedObjects;
        private SubscriberSocket client;
        private NetMQPoller poller;
        private TaskCompletionSource<byte[]> responseTask;

        [Tooltip("Follows ZeroMQ syntax")]
        public string server_uri = "tcp://localhost:5000";

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
            client = new SubscriberSocket();
            client.Subscribe("");
            client.Connect(server_uri);
            poller = new NetMQPoller { client };
            //set-up event to add to task
            responseTask = new TaskCompletionSource<byte[]>();
            client.ReceiveReady += (s, a) => {               
                responseTask.SetResult(a.Socket.ReceiveFrameBytes());
            };
            //start polling thread
            poller.RunAsync();
            
        }   

        // Update is called once per frame
        void Update()
        {
            if (responseTask.Task.IsCompleted)
            {
                StructuresState state = StructuresState.Parser.ParseFrom(responseTask.Task.Result);
                UnityEngine.Debug.Log("Received message " +  state.Time);
                synchronizeState(state);
                //this is how you reset?
                responseTask = new TaskCompletionSource<byte[]>();
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

        void OnApplicationQuit()
        {
            client.Close();
            client.Dispose();
            poller.StopAsync();
            poller.Dispose();
            NetMQConfig.Cleanup();
        }
    }

}
