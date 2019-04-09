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
        private float temperature;
        private float volume;
        private bool is_batch;
        private string fraction_label;
        private string reactorLabel;
        private float frac_factor;

        // Use this for initialization
        void Start()
        {
            fraction_dict = new Dictionary<int, Transform>();
            r = (float)6.25;//just an eyeballed distance to outer edge of the circle
            is_batch = false;
            //update color proportions
            Renderer rend = this.GetComponent<Renderer>();
            rend.material.shader = Shader.Find("Custom/WedgeCircle");
            rend.material.SetInt("_NumWedges", 5); //default to 4 species, in quarters
            // "_Fraction#" for the mole fractions.
            rend.material.SetFloat("_Fraction1", 0.0f);
            rend.material.SetFloat("_Fraction2", 0.0f);
            rend.material.SetFloat("_Fraction3", 0.0f);
            rend.material.SetFloat("_Fraction4", 0.0f);
            rend.material.SetFloat("_Fraction5", 1.0f);
            // "_FlowRate#" for the molar flow rates.
            rend.material.SetFloat("_FlowRate1", 0.0f);
            rend.material.SetFloat("_FlowRate2", 0.0f);
            rend.material.SetFloat("_FlowRate3", 0.0f);
            rend.material.SetFloat("_FlowRate4", 0.0f);
            rend.material.SetFloat("_FlowRate5", 0.0f);

            GameObject temperatureValue = GameObject.Find("Backend/ColorKey/TemperatureValue");
            GameObject volumeValue = GameObject.Find("Backend/ColorKey/VolumeValue");
            Transform temp_canvas = this.gameObject.transform.GetChild(1).GetChild(0);
            temp_canvas.GetComponent<Text>().text = temperatureValue.GetComponent<Text>().text + volumeValue.GetComponent<Text>().text;

        }

        public void set_batch_status(bool value)
        {
            this.is_batch = value;
            //Debug.Log("set_batch_status has been called and is_batch is now " + this.is_batch);
            if(this.is_batch)
            {
                this.fraction_label = "mol%";
                this.frac_factor = 100.0f;
            }
            else
            {
                this.fraction_label = "mol/s";
                this.frac_factor = 1.0f;
            }
            Debug.Log("Fraction label has been set to " + this.fraction_label);
        }

        public void set_temp(float new_temp) //Called in CommClient.cs to set the new temp (and new vol, see below) from the dial to reactor object
        {
            temperature = new_temp;
        }

        public void set_vol(float new_vol)
        {
            volume = new_vol;
        }

        public void set_label(string label)
        {
            if(label == "pbr")
            {
                label = "batch";
            }
            reactorLabel = label.ToUpper();//all caps for display purposes
        }

        public void set_molefrac(int i, Renderer rend, float mole_frac)
        {
            rend.material.SetFloat("_Fraction" + (i+1).ToString(), value: mole_frac);
        }

        public void set_flowrate (int i, Renderer rend, float flow_rate)
        {
            rend.material.SetFloat("_FlowRate" + (i+1).ToString(), value: flow_rate);
        }

        public void set_numwedges(Renderer rend, int num)
        {
            rend.material.SetInt("_NumWedges", value: num);
        }

        // Update is called once per frame
        void Update()
        {
            Renderer rend = this.GetComponent<Renderer>();
            float temperature = this.temperature;
            float volume = this.volume;
            float frac_factor = this.frac_factor;
            bool is_batch = this.is_batch;
            string fraction_label = this.fraction_label;
            Transform temp_canvas = this.gameObject.transform.GetChild(1).GetChild(0);
            temp_canvas.GetComponent<Text>().text = "" + (int)temperature + " K | " + (int)volume + " L";
            Transform labelCanvas = this.gameObject.transform.GetChild(2).GetChild(0);
            labelCanvas.GetComponent<Text>().text = reactorLabel;//set it

            int num_wedges = rend.material.GetInt("_NumWedges");
            float frac = 0;
            float sum = 0;
            float prev_frac = 1;
            float next_frac = 1;
            float offset = Mathf.PI;//need to add half a circle to rendre in right place
            float[] flow_rates = new float[num_wedges];//to store the raw numbers for molar flow rate
            float flow_rate_sum = (float) 0.0;
            float[] mole_frac = new float[num_wedges];
            for(int i =0; i < num_wedges; i++)
            {
                mole_frac[i] = rend.material.GetFloat("_Fraction" + (i + 1).ToString());
                flow_rates[i] = rend.material.GetFloat("_FlowRate" + (i+1).ToString());
                flow_rate_sum += flow_rates[i];
            }
            for(int i = 0; i < num_wedges; i++)
            {
                offset = Mathf.PI;
                frac = mole_frac[i];
                sum += frac/(float)2.0;//get this center
                if(prev_frac *100 < 3.0 && frac * 100 < 3.0)
                {
                    offset += Mathf.PI * (float)0.03;//compensate away
                }
                if(i < num_wedges - 1)
                {
                    next_frac = mole_frac[i+1];
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
                        new_text.GetComponent<Text>().text = "" + (flow_rates[i] * frac_factor).ToString("F3") + fraction_label;
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
                        existing_text.GetComponent<Text>().text = "" + (flow_rates[i] * frac_factor).ToString("F3") + fraction_label;
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