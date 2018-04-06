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
        private int temperature;

        // Use this for initialization
        void Start()
        {
            fraction_dict = new Dictionary<int, Transform>();
            r = (float)6.25;//just an eyeballed distance to outer edge of the circle
            //update color proportions
            Renderer rend = this.GetComponent<Renderer>();
            rend.material.shader = Shader.Find("Custom/WedgeCircle");
            rend.material.SetInt("_NumWedges", 5); //default to 4 species, in quarters
            rend.material.SetFloat("_Fraction1", 0.0f);
            rend.material.SetFloat("_Fraction2", 0.0f);
            rend.material.SetFloat("_Fraction3", 0.0f);
            rend.material.SetFloat("_Fraction4", 0.0f);
            rend.material.SetFloat("_Fraction5", 1.0f);
            GameObject temperatureValue = GameObject.Find("Backend/ColorKey/TemperatureValue");
            Transform temp_canvas = this.gameObject.transform.GetChild(1).GetChild(0);
            temp_canvas.GetComponent<Text>().text = temperatureValue.GetComponent<Text>().text;//default to currently-displayed temperature -- should actually only need this?
        }

        public void set_temp(int temp)
        {
            temperature = temp;
        }

        // Update is called once per frame
        void Update()
        {
            Renderer rend = this.GetComponent<Renderer>();
            float temp = rend.material.GetFloat("_Temperature");//get the temp anyway in case we got it wrong the first time...
            Transform temp_canvas = this.gameObject.transform.GetChild(1).GetChild(0);
            temp_canvas.GetComponent<Text>().text = "" + (int)temp + " K";
            int num_wedges = rend.material.GetInt("_NumWedges");
            float frac = 0;
            float sum = 0;
            float prev_frac = 1;
            float next_frac = 1;
            float offset = Mathf.PI;//need to add half a circle to rendre in right place
            float[] flow_rates = new float[num_wedges];//to store the raw numbers for flow rates
            float flow_rate_sum = (float) 0.0;
            for(int i =0; i < num_wedges; i++){
                flow_rates[i] = rend.material.GetFloat("_Fraction" + (i + 1).ToString());
                flow_rate_sum += flow_rates[i];
            }
            for(int i = 0; i < num_wedges; i++)
            {
                offset = Mathf.PI;
                frac = flow_rates[i] / flow_rate_sum;
                sum += frac/(float)2.0;//get this center
                if(prev_frac *100 < 3.0 && frac * 100 < 3.0)
                {
                    offset += Mathf.PI * (float)0.03;//compensate away
                }
                if(i < num_wedges - 1)
                {
                    next_frac = flow_rates[i+1] / flow_rate_sum;
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
                    if(frac != 1.0 && frac != 0.0){
                        new_text.SetPositionAndRotation(new Vector3(this.transform.position.x + r * Mathf.Cos(sum * 2 * Mathf.PI + offset ), this.transform.position.y + r * Mathf.Sin(sum * 2 * Mathf.PI + offset ), 0), Quaternion.identity);
                        new_text.GetComponent<Text>().text = "" + (flow_rates[i]).ToString("F2") + "mol/s";
                    }
                    else{
                        new_text.GetComponent<Text>().text = "";
                    }
                }
                else//dict entry already exists, so just change the label
                {
                    Transform existing_text = fraction_dict[i];
                    existing_text.SetPositionAndRotation(new Vector3(this.transform.position.x + r * Mathf.Cos(sum * 2 * Mathf.PI + offset ), this.transform.position.y + r * Mathf.Sin(sum * 2 * Mathf.PI + offset ), 0), Quaternion.identity);
                    if(frac == 1.0 || frac == 0.0){
                        existing_text.GetComponent<Text>().text = "";
                    }
                    else{
                        existing_text.GetComponent<Text>().text = "" + (flow_rates[i]).ToString("F2") + "mol/s";
                    }

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