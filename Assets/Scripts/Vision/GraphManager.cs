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
using UnityEngine.UI;
using Rochester.ARTable.Particles;
using System.Linq;
using Rochester.ARTable.Structures;
using System.IO;

namespace Rochester.Physics.Communication{
    public class GraphManager : MonoBehaviour {
        private Graph system;//the system of connected reactors
        private Dictionary<int, List<int>> edgeList; //to keep track of system edges
        private int edgesAdded;
        // Use this for initialization
        void Start () {
            edgesAdded = 0; //no edges when starting
        }
        //add nodes to the graph
        public void AddNode (int id, string label) {
            system.Nodes.Add(node, null);
            o = system.Nodes[id];
            o.Label = label;
        }
        //add connections between nodes
        public void ConnectNodes (int source, string sourceLabel, int target, string targetLabel) {
            edgesAdded += 1;
            system.Edges.Add(edgesAdded, null);
            system.Edges[edgesAdded].idA = source;
            system.Edges[edgesAdded].labelA = sourceLabel;
            system.Edges[edgesAdded].idB = target;
            system.Edges[edgesAdded].labelB = target;
        }
        //delete nodes from the graph
        public void DeleteNode (int node) {
            //remove the node from graph
            if (system.ContainsKey(node)){
                system.Nodes.Remove(node);
            }
            //remove edges that contain that node
            foreach(var key in system.Edges){
                if (key.idA == node || key.idB == node){
                    system.Edges.Remove(key);
                    edgesAdded -= 1;
                }
            }
        }

        public Graph GetGraph () {
            return system;
        }
        // Update is called once per frame
        void Update () {

        }
    }
}