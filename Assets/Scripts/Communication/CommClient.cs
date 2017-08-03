using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetMQ;
using Google.Protobuf;
using NetMQ.Sockets;
using System.Threading.Tasks;

//TODO: Put namespaces everywhere, add two frames to messages. One to indicate
// type, one for content. Switch to pub/sub model. Write python server.

namespace Rochester.Physics.Communication
{
    public class CommClient : MonoBehaviour
    {
        
        private RequestSocket client;
        private NetMQPoller poller;
        private TaskCompletionSource<byte[]> responseTask;

        [Tooltip("Follows ZeroMQ syntax")]
        public string server_uri = "@tcp://*:5000";

        // Use this for initialization
        void Start()
        {
            client = new RequestSocket(server_uri);
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
                System.Console.WriteLine("Received message {0}", state.Time);
                //this is how you reset?
                responseTask = new TaskCompletionSource<byte[]>();
            }
        }
    }

}
