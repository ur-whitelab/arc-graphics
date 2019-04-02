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
        private Graph system = new Graph();//the system of connected reactors
        private Dictionary<int, List<int>> edgeList; //to keep track of system edges
        private int edgesAdded;
        // Use this for initialization
        void Start () {
            edgesAdded = 0; //no edges when starting
        }
        //add nodes to the graph
        public void AddNode (int id, string label) {
            Node o = new Node();
            o.Label = label;
            o.Id = id;
            system.Nodes.Add(id, o);
        }
        //add connections between nodes
        private void ConnectNodes (Node A, Node B){//TODO: add removal of edges that already exist.
            edgesAdded += 1;
            Edge edge = new Edge();
            edge.IdA = A.Id;
            edge.LabelA = A.Label;
            edge.IdB = B.Id;
            edge.LabelB = B.Label;
            system.Edges.Add(edgesAdded, edge);
        }

        public bool CheckConnectedById(int idA, int idB)
        {
            //checks if two graph nodes are connected by their ID values.
            if(system.Nodes.ContainsKey(idA) && system.Nodes.ContainsKey(idB))
            {
                for(int i = 0; i < system.edgesAdded; i++)
                {
                    if((system.Edges[i].IdA == idA && system.Edges[i].IdB == idB) || system.Edges[i].IdB == idA && system.Edges[i].IdA == idB)
                    {
                        return(true);
                    }
                }
            }    
            return(false);
        }

        public void ConnectById(int idA, int idB)
        {
            ConnectNodes(system.Nodes[idA], system.Nodes[idB]);
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

        public bool idExists(int nodeId)
        {
            return (system.Nodes.ContainsKey(nodeId));
        }

        public Graph GetGraph () {
            return system;
        }
        // Update is called once per frame
        void Update () {

        }
    }
}
