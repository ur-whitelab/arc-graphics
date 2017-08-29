using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rochester.ARTable.Structures
{
    public class Reactor : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {

            //update color proportions
            Renderer rend = this.GetComponent<Renderer>();
            rend.material.shader = Shader.Find("Custom/WedgeCircle");
            rend.material.SetInt("_NumWedges", 4); //hard-capped at 4 for the demo... 
            rend.material.SetFloat("_Fraction1", 0.25f);
            rend.material.SetFloat("_Fraction2", 0.25f);
            rend.material.SetFloat("_Fraction3", 0.3f);
            rend.material.SetFloat("_Fraction4", 0.2f);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}