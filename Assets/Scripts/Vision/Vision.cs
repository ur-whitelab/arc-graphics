using System.Collections;
using System.Collections.Generic;
using System.IO;
using Rochester.Physics.Communication;
using TensorFlow;
using UnityEngine;
using UnityEngine.UI;

/*
** This is a reimplementation of the tracker_processor.py file from ur-whitelab/arc-vision for use in Unity.
 */

namespace Rochester.ARTable.Structures
{
    public class Vision : MonoBehaviour
    {

        private Dictionary<string, string> tracking;//dict of tracked objects.
        private Graph system;//the system of connected reactors

        private WebCamTexture cameraTexture;
        private Texture2D cameraImage;
        private byte[] rawImageBytes;
        private TFGraph tensorFlowGraph;//the TensorFlow graph model
        private TFSession tensorFlowSession;//the TensorFlow session for executing the graph
        private List<Vector4> boundingBoxes;//the list of bounding boxes is kept as Vector4 objects for convenience
        
        // Use this for initialization
        void Start()
        {
            //load model, c/o https://github.com/Unity-Technologies/ml-agents/blob/cb0bfa0382650dee2071eb415147d795721297b1/UnitySDK/Assets/ML-Agents/Scripts/InferenceBrain/TFSharpInferenceEngine.cs
#if UNITY_ANDROID && !UNITY_EDITOR
            // This needs to ba called only once and will raise an exception if called multiple times 
            try{
                TensorFlowSharp.Android.NativeBinding.Init();
            }
            catch{

            }
#endif
            tensorFlowGraph = new TFGraph();
            //tensorFlowGraph.Import(model);
            tensorFlowSession = new TFSession(tensorFlowGraph);

            //set up camera texture
            if(cameraTexture == null)
            {
                cameraTexture = new WebCamTexture();
                cameraImage = new Texture2D(cameraTexture.width, cameraTexture.height);
            }
            boundingBoxes = new List<Vector4>();
        }

        Dictionary<string, string> Track(){
            //nothing yet
            return new Dictionary<string, string>();
        }

        Vector3 coordinatesFromBox(Vector4 boudingBox)
        {
            //do some fancy geometry to convert from 2D bounding box to 3D game space coordinates
            //nothing yet
            return new Vector3();
        }

        List<Vector4> getBoundingBoxes(TFGraph graph, byte[] rawImage)
        {
            //nothing yet
            return new List<Vector4>();
        }


        // Update is called once per frame
        void Update()
        {
            //wait for frame to be done
            //yield return new WaitForEndOfFrame();       
            //get camera's view
            cameraImage.SetPixels(cameraTexture.GetPixels());
            cameraImage.Apply();
            rawImageBytes = cameraImage.EncodeToJPG(); // can also do EncodeToPNG()
            //call model on camera's view
            boundingBoxes = getBoundingBoxes(tensorFlowGraph, rawImageBytes);
            foreach(Vector4 bbox in boundingBoxes)//get all bounding boxes//TODO: write this
            {
                Vector3 detectedLocation = coordinatesFromBox(bbox);//go from 2D bbox image coordinates to 3D game space coordinates

            }
        }
    }
}