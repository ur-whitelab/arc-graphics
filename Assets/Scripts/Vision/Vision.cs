using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using GoogleARCore;
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

        public int ModelImageHeight = 416;
        public int ModelImageWidth = 416;
        public float ConfidenceCutoff = 0.8f;
        public int BoxNumber = 3;
        public TextAsset Model;
        

        private Dictionary<string, string> tracking;//dict of tracked objects.
        private Graph system;//the system of connected reactors

        private WebCamTexture cameraTexture;
        private Texture2D cameraImage;
        private byte[] rawImageBytes;
        private TFGraph tensorFlowGraph;//the TensorFlow graph model
        private TFSession tensorFlowSession;//the TensorFlow session for executing the graph        
        // Use this for initialization
        void Start()
        {
            //load model, c/o https://github.com/Unity-Technologies/ml-agents/blob/cb0bfa0382650dee2071eb415147d795721297b1/UnitySDK/Assets/ML-Agents/Scripts/InferenceBrain/TFSharpInferenceEngine.cs
#if UNITY_ANDROID && !UNITY_EDITOR
            // This needs to ba called only once and will raise an exception if called multiple times 
            try{ 
                // using TensorFlowSharp;
                // TensorFlowSharp.Android.NativeBinding.Init();
            }
            catch{

            }
#endif
            tensorFlowGraph = new TFGraph();            
            tensorFlowGraph.Import(Model.bytes);
        
        }

        Dictionary<string, string> Track(int id_num, int temperature, int volume){
            //nothing yet
            return new Dictionary<string, string>();
        }

        Vector3 coordinatesFromBox(Vector4 boundingBox)
        {
            //do some fancy geometry to convert from 2D bounding box to 3D game space coordinates
            //get lower two bbox corners
            //raycast(?) to the point halfway between them?
            //get the point on the detected plane corresponding to this location
            //return the value
            return new Vector3();
        }

      List<KeyValuePair<string, Vector4>> getBoundingBoxes(TFGraph graph, TFTensor img, int height, int width)
        {
            //returns a list of reactor types paired with coordinates obtained from TF
            //use our TF model to get bounding boxes
            //convert the TF output into Vector4 items
            //put the vectors in a list and return them
          tensorFlowSession = new TFSession(graph);
          var runner = tensorFlowSession.GetRunner();
          //translate raw image bytes into tensor for input
          //give input tensors to TF graph
          runner.AddInput (graph ["input/yuv"] [0], img);
          runner.AddInput(graph["input/box-number"][0], BoxNumber);
            runner.AddInput(graph["input/width"][0], width);
            runner.AddInput(graph["input/height"][0], height);
            runner.Fetch (graph ["output/boxes"] [0]);//get bounding boxes tensor
          runner.Fetch (graph ["output/confidences"] [0]);//get label confidences tensor            
            var output = runner.Run(); //run the model
          TFTensor boxesResult = output[0]; //store outputs
          var boxesValues = (float [,])boxesResult.GetValue();
          TFTensor confidencesResult = output[1];
          var confidencesValues = (float [,]) confidencesResult.GetValue();
          float lastConf;
          List<KeyValuePair<string, Vector4>> retval = new List<KeyValuePair<string, Vector4>>();
          for(int i = 0; i < confidencesValues.Length; i++)
          {
            lastConf = ConfidenceCutoff;
            //iterate through the sorted results and get the ones above an accuracy cutoff
            for(int j = 0; j < confidencesValues.GetLength(i); j++)
            {
              if(confidencesValues[i,j] > lastConf)
              {
                string label = "";
                switch (j)
                {
                case 0:
                  //this is the first class -- cstr? TODO: get the corresponding classes
                  label = "cstr";
                  break;
                case 1:
                  //this is the second class -- pfr? TODO: get the corresponding classes
                  label = "pfr";
                  break;
                case 2:
                  //this is the third class -- source? TODO: get the corresponding classes
                  label = "source";
                  break;
                }
                retval.Add(new KeyValuePair<string, Vector4>(label,
                                                             new Vector4(
                                                               boxesValues[i,0],
                                                               boxesValues[i,1],
                                                               boxesValues[i,2],
                                                               boxesValues[i,3]
                                                               )));
              }
            }
          }
          return(retval);
        }

        [MonoPInvokeCallback(typeof(TFTensor.Deallocator))]
        internal static void EmptyDeallocator(IntPtr data, IntPtr len, IntPtr closure)
        {
            // presumably AR Core takes care of this. 
        }

        // Update is called once per frame
        void Update()
        {
            //wait for frame to be done before calling TF model
            // yield return new WaitForEndOfFrame();       
            //get camera's view
            using (var image = Frame.CameraImage.AcquireCameraImageBytes())
            {
                if (!image.IsAvailable)
                {
                    return;
                }
                // TODO Check data type -> (?)
                TFTensor img_tensor = new TFTensor(TFDataType.Int16, new long[] { image.Height, image.Width }, image.Y, new UIntPtr((uint) (image.YRowStride * image.Height)), EmptyDeallocator, IntPtr.Zero);

                //call model on camera's view
                var boundingBoxes = getBoundingBoxes(tensorFlowGraph, img_tensor, image.Height, image.Width);
                string detectedKey;
                Vector3 detectedLocation;
                foreach (var bbox in boundingBoxes)//get all bounding boxes
                {
                    detectedKey = bbox.Key;
                    detectedLocation = coordinatesFromBox(bbox.Value);//go from 2D bbox image coordinates to 3D game space coordinates
                    Debug.Log(detectedKey + detectedLocation);
                }
            }                        
        }
    }
}
