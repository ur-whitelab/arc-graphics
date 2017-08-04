using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetMQ;
using Google.Protobuf;
using NetMQ.Sockets;
using System.Threading.Tasks;

//TODO: Put namespaces everywhere, add two frames to messages. One to indicate
// type, one for content. Switch to pub/sub model. Write python server. Should talk to bluehive
//and unity. Bluehive will also pub/sub for unity and possible monitors/multiple table.s

namespace Rochester.Physics.Communication
{
    public class CommClient : MonoBehaviour
    {
        
        private SubscriberSocket client;
        private NetMQPoller poller;
        private TaskCompletionSource<byte[]> responseTask;

        [Tooltip("Follows ZeroMQ syntax")]
        public string server_uri = "tcp://localhost:5000";

        // Use this for initialization
        void Start()
        {

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
                GameState state = GameState.Parser.ParseFrom(responseTask.Task.Result);
                 UnityEngine.Debug.Log("Received message " +  state.Time);
                //this is how you reset?
                responseTask = new TaskCompletionSource<byte[]>();
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
