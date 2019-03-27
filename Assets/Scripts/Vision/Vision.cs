using System.Collections;
using System.Collections.Generic;
using System.IO;
using Rochester.Physics.Communication;
using TensorFlow;
using TensorFlow.Examples.ExampleCommon;
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
        private float cutoff;
        
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
            tensorFlowGraph.Import(model);

            //set up camera texture
            if(cameraTexture == null)
            {
                cameraTexture = new WebCamTexture(); 
            }
            boundingBoxes = new List<Vector4>();

            cutoff = 0.8;//80% cutoff
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

        TFTensor CreateTensorFromImageBytes (bytes[] bytes_in, TFDataType destinationDataType = TFDataType.Float)
        {
            /*Modified from TensorFlowSharp source code in
             * TensorFlow.Examples.ExampleCommon: ImageUtil.cs
            */
            var contents = bytes_in;

            // DecodeJpeg uses a scalar String-valued tensor as input.
            var tensor = TFTensor.CreateString (contents);

            TFOutput input, output;

            // Construct a graph to normalize the image
            using (var graph = ConstructGraphToNormalizeImage (out input, out output, destinationDataType)){
              // Execute that graph to normalize this one image
              using (var session = new TFSession (graph)) {
                  var normalized = session.Run (
                      inputs: new [] { input },
                      inputValues: new [] { tensor },
                      outputs: new [] { output });
	    				
                  return normalized [0];
                }
            }
        }
      
      List<KeyValuePair<string, Vector4>> getBoundingBoxes(TFGraph graph, byte[] rawImage, int width, int height, float cutoff)
        {
            //returns a list of reactor types paired with coordinates obtained from TF
            //use our TF model to get bounding boxes
            //convert the TF output into Vector4 items
            //put the vectors in a list and return them
          tensorFlowSession = new TFSession(graph);
          var runner = tensorFlowSession.getRunner();
          //translate raw image bytes into tensor for input
          TFTensor imageTensor = CreateTensorFromImageBytes(rawImage, TFDataType.UInt8);
          TFTensor widthTensor = width;
          TFTensor heightTensor = height;
          //give input tensors to TF graph
          runner.AddInput (graph ["image"] [0], imageTensor);//might be "image:0"? not sure
          runner.AddInput (graph ["img_width"] [0], widthTensor);//might be "image:0"? not sure
          runner.AddInput (graph ["img_height"] [0], heightTensor);//might be "image:0"? not sure
          runner.Fetch (graph ["box_output"] [0]);//get bounding boxes tensor
          runner.Fetch (graph ["confidence_output"] [0]);//get label confidences tensor
          var output = runner.Run(); //run the model
          TFTensor boxesResult = output[0]; //store outputs
          var boxesValues = boxesResult.GetValue();
          TFTensor confidencesResult = output[1];
          var confidencesValues = confidencesResult.GetValue();
          List<KeyValuePair<string, Vector4>>() retval = new List<KeyValuePair<string, Vector4>>();
          for(int i = 0; i < confidencesValues.Length; i++)
          {
              //iterate through the sorted results and get the ones above an accuracy cutoff
            for(int j = 0; j < confidencesValues[i].Length; j++)
            {
              if(confidencesValues[i][j] > cutoff)
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
                retval.Add(new keyValuePair<string, Vector4>(label,
                                                             new Vector4(
                                                               boxesValues[i][0],
                                                               boxesValues[i][1],
                                                               boxesValues[i][2],
                                                               boxesValues[i][3],
                                                               )));
              }
            }
          }
          return(retval);
        }


        // Update is called once per frame
        void Update()
        {
            //wait for frame to be done before calling TF model
            yield return new WaitForEndOfFrame();       
            //get camera's view
            cameraImage.SetPixels(cameraTexture.GetPixels());
            cameraImage.Apply();
            rawImageBytes = cameraImage.EncodeToJPG(); // can also do EncodeToPNG()
            //call model on camera's view
            boundingBoxes = getBoundingBoxes(tensorFlowGraph, rawImageBytes, cameraTexture.width, cameraTexture.height, cutoff);
            string detectedKey;
            string detectedLocation;
            foreach(KeyValuePair<string, Vector4> reactor in boundingBoxes)//get all bounding boxes//TODO: write this
            {
                detectedKey = reactor.Key;
                detectedLocation = coordinatesFromBox(bbox);//go from 2D bbox image coordinates to 3D game space coordinates

            }
            
        }
    }
}
