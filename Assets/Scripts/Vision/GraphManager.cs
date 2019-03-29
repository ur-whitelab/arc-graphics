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
            system = new Graph();
            edgesAdded = 0; //no edges when starting
        }
        //add nodes to the graph
        public void AddNode (int id, string label) {
            system.Nodes.Add(id, new Node());
            Node o = system.Nodes[id];
            o.Label = label;
        }
        //add connections between nodes
        public void ConnectNodes (Node A, Node B){
            edgesAdded += 1;
            system.Edges.Add(edgesAdded, new Edge());
            system.Edges[edgesAdded].IdA = A.Id;
            system.Edges[edgesAdded].LabelA = A.Label;
            system.Edges[edgesAdded].IdB = B.Id;
            system.Edges[edgesAdded].LabelB = B.Label;
        }
        //delete nodes from the graph
        public void DeleteNode (int nodeId) {
            //remove the node from graph
            if (system.Nodes.ContainsKey(nodeId)){
                system.Nodes.Remove(nodeId);
            }
            //remove edges that contain that node
            foreach(var key in system.Edges.Keys){
                if (system.Edges[key].IdA == nodeId || system.Edges[key].IdB == nodeId){
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
