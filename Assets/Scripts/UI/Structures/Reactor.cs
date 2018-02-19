using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Rochester.ARTable.Structures
{
    public class Reactor : MonoBehaviour
    {
        private Dictionary<int, Transform> fraction_dict;
        private float r;

        // Use this for initialization
        void Start()
        {
            fraction_dict = new Dictionary<int, Transform>();
            r = (float)6.25;//just an eyeballed distance to outer edge of the circle
            //update color proportions
            Renderer rend = this.GetComponent<Renderer>();
            rend.material.shader = Shader.Find("Custom/WedgeCircle");
            rend.material.SetInt("_NumWedges", 4); //default to 4 species, in quarters 
            rend.material.SetFloat("_Fraction1", 0.25f);
            rend.material.SetFloat("_Fraction2", 0.25f);
            rend.material.SetFloat("_Fraction3", 0.25f);
            rend.material.SetFloat("_Fraction4", 0.25f);
        }

        // Update is called once per frame
        void Update()
        {
            Renderer rend = this.GetComponent<Renderer>();
            int num_wedges = rend.material.GetInt("_NumWedges");
            float frac = 0;
            float sum = 0;
            float prev_frac = 1;
            float next_frac = 1;
            float offset = Mathf.PI;//need to add half a circle to rendre in right place
            for(int i = 0; i < num_wedges; i++)
            {
                offset = Mathf.PI;
                frac = rend.material.GetFloat("_Fraction" + (i + 1));
                sum += frac/(float)2.0;//get this center
                if(prev_frac *100 < 3.0 && frac * 100 < 3.0)
                {
                    offset += Mathf.PI * (float)0.03;//compensate away
                }
                if(i < num_wedges - 1)
                {
                    next_frac = rend.material.GetFloat("_Fraction" + (i + 2));
                }
                if(frac * 100 < 3.0 && next_frac * 100 < 3.0)
                {
                    offset -= Mathf.PI * (float)0.03;//compensate away and treat the triple-sandwich
                }
                //SET POSITIONS OF TEXTS HERE
                if (!fraction_dict.ContainsKey(i))
                {
                    Transform new_text = Instantiate(this.gameObject.transform.GetChild(0).transform.GetChild(0), this.gameObject.transform.GetChild(0));//get the text grandchild and clone it, making sure to put it in the canvas
                    fraction_dict.Add(i, new_text);//keep track of it -- need both transform and its text for positioning...
                    new_text.SetPositionAndRotation(new Vector3(this.transform.position.x + r * Mathf.Cos(sum * 2 * Mathf.PI + offset ), this.transform.position.y + r * Mathf.Sin(sum * 2 * Mathf.PI + offset ), 0), Quaternion.identity);
                    Debug.Log("Set position of new label to " + new_text.transform.position.x + ", " + new_text.transform.position.y + ".\n");
                    new_text.GetComponent<Text>().text = "" + (frac * 100).ToString("F2") + "%";
                    Debug.Log("Set mole fraction of new label to " + (frac * 100).ToString("F2") + "%.");
                }
                else//dict entry already exists, so just change the label
                {
                    Transform existing_text = fraction_dict[i];
                    existing_text.SetPositionAndRotation(new Vector3(this.transform.position.x + r * Mathf.Cos(sum * 2 * Mathf.PI + offset ), this.transform.position.y + r * Mathf.Sin(sum * 2 * Mathf.PI + offset ), 0), Quaternion.identity);
                    existing_text.GetComponent<Text>().text = "" + (frac * 100).ToString("F2") + "%";
                }
                sum += frac / (float)2.0;
                prev_frac = frac;
                next_frac = 1;
            }
            for(int i = 0; i < fraction_dict.Count; i++)
            {
                if (i >= num_wedges)
                {
                    Destroy(fraction_dict[i].gameObject);
                    fraction_dict.Remove(i);
                }
            }
        }
    }
}