using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetMQ;
using NetMQ.Sockets;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Rochester.ARTable.Communication
{
    public class SceneListener : MonoBehaviour
    {

        private SubscriberSocket SceneClient;
        private NetMQPoller ScenePoller;
        private TaskCompletionSource<string> SceneResponseTask;

        [Tooltip("Follows ZeroMQ syntax")]
        public string server_uri = "tcp://127.0.0.1:5000";

        void Awake()
        {
            AsyncIO.ForceDotNet.Force();
            DontDestroyOnLoad(gameObject);
        }

        // Use this for initialization
        void Start()
        {

            //set-up socket and poller
            SceneClient = new SubscriberSocket();
            SceneClient.Subscribe("vision-mode");//vision-update, but "any" for testing
            UnityEngine.Debug.Log("set up the subscription at " + server_uri);
            SceneClient.Connect(server_uri);
            ScenePoller = new NetMQPoller { SceneClient };
            //set-up event to add to task
            SceneResponseTask = new TaskCompletionSource<string>();
            SceneClient.ReceiveReady += (s, a) =>
            {
                List<string> msg = a.Socket.ReceiveMultipartStrings();

                SceneResponseTask.TrySetResult(msg[1]);

            };

            //start polling thread
            ScenePoller.RunAsync();
        }


        void OpenScene(string name)
        {
            //name = "_Scenes/" + name;
            Scene current_scene = SceneManager.GetActiveScene();
            if (name == current_scene.name)
            {
                UnityEngine.Debug.Log("Received message to change to currently active scene. Ignoring.");
            }
            else
            {

                //else //it's a valid scene, so switch to it

                {
                    SceneManager.LoadScene(name, LoadSceneMode.Single);
                }
            }
        }


        // Update is called once per frame
        void Update()
        {
            bool SwitchToDetection = false;
            SwitchToDetection|= Input.GetKeyDown("k");
            bool SwitchToTarget = false;
            SwitchToTarget |= Input.GetKeyDown("t");
            if (SwitchToDetection)
            {
                OpenScene("detection");
            }

            if (SwitchToTarget)
            {
                OpenScene("target");
            }
            //get scene we need to go to
            if (SceneResponseTask.Task.IsCompleted)
            {
                //UnityEngine.Debug.Log("THE MESSAGE TASK RESULT WAS " + SceneResponseTask.Task.Result);
                string newscene = SceneResponseTask.Task.Result;//I think this is how it works?
                UnityEngine.Debug.Log("Received message " + newscene + " from scene server.");
                if(newscene == "darkflow"){
                    newscene = "detection";//just use detection for darkflow scene
                }
                OpenScene(newscene);
                //this is how you reset?
                SceneResponseTask = new TaskCompletionSource<string>();
            }
            if (Input.GetKey("escape"))//actually exit if we need to...
                Application.Quit();
        }

        void OnApplicationQuit()//cleanup
        {
            SceneClient.Close();
            SceneClient.Dispose();
            try
            {
                ScenePoller.StopAsync();
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.Log("Tried to stopasync while the poller wasn't running! Oops.");
            }
            ScenePoller.Dispose();
            NetMQConfig.Cleanup();
        }
    }
}