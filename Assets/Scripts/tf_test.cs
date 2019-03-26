using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TensorFlow;

public class tf_test : MonoBehaviour
{
    private TFGraph tensorFlowGraph;//the TensorFlow graph model
    private TFSession tensorFlowSession;//the TensorFlow session for executing the graph
    // Start is called before the first frame update
    void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
            // This needs to ba called only once and will raise an exception if called multiple times
            try{
                TensorFlowSharp.Android.NativeBinding.Init();
            }
            catch{

            }
#endif
        using (tensorFlowGraph = new TFGraph())
        {
            TextAsset graphModel = Resources.Load("example_tf_model") as TextAsset;
            Debug.Log("graphmodel is " + graphModel.text + "test");
            tensorFlowGraph.Import(graphModel.bytes);
            tensorFlowSession = new TFSession(tensorFlowGraph);
            var runner = tensorFlowSession.GetRunner();
            //runner.AddInput(tensorFlowGraph["v1"][0], new TFTensor(0.0));
            runner.Fetch(tensorFlowGraph["v1"][0]);
            var output = runner.Run();
            Debug.Log("Hello! output is: " + output);
        }
            
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
