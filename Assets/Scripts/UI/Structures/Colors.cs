using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Rochester.ARTable.UI;
using Rochester.ARTable.Particles;

namespace Rochester.ARTable.Structures
{
    public class Colors : MonoBehaviour
    {
        //TODO: make a grid of colors with a varying number of squares per side both vertically and horizontally; take in the color array
        //use buffer to send array to shader
        [Tooltip("The time in seconds between color updates")]
        public float CycleTime = 1.0f;
        [Tooltip("The number of boxes across")]
        public int NumQuadsX = 2;
        [Tooltip("The number of boxes vertically")]
        public int NumQuadsY = 2;
        public bool UseBorders = true;
        [Tooltip("How thick the boxes are relative to the borders between them")]
        [Range(2, 40)]
        public int BoxWidth = 10;
        private int numColors;
        Color[] colorArr;
        public Material ColorMaterial;

        //for timing the updates
        private float timer = 0.0f;

        private Texture2D texture;
        

        private void Awake()
        {
            texture = new Texture2D(NumQuadsX * BoxWidth + 1, NumQuadsY * BoxWidth + 1, TextureFormat.ARGB32, false);
            numColors = NumQuadsX * NumQuadsY;
            ColorMaterial.SetInt("numquadsx", NumQuadsX);
            ColorMaterial.SetInt("numquadsy", NumQuadsY);
            colorArr = new Color[numColors];
            changeColors();
        }

        private void changeColors()
        {
            for (int i = 0; i < numColors; i++)
            {
                //random colors. passing a jpg for calibraion
                colorArr[i] = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
            }
            
            // Create a new texture ARGB32 (32 bit with alpha) and no mipmaps
            if (UseBorders)
            {
                int k = 0;
                int l = 0;
                // set the pixel values
                for (int i = 0; i < NumQuadsX * BoxWidth + 1; i++)
                {
                    for (int j = 0; j < NumQuadsY * BoxWidth + 1; j++)
                    {
                        if (i % BoxWidth == 0 || j % BoxWidth == 0)
                        {
                            texture.SetPixel(i, j, new Color(0, 0, 0, 1));
                        }
                        else
                        {
                            texture.SetPixel(i, j, colorArr[k * NumQuadsY + l]);
                            //Debug.Log("Set texture pixel " + i + ", " + j + " to <color=#" + ColorUtility.ToHtmlStringRGBA(colorArr[i * NumQuadsY + j]) + "> " + colors[i * NumQuadsY + j] + "</color>");
                        }
                        k = i / BoxWidth;
                        l = j / BoxWidth;
                    }

                }
            }

            else
            {
                texture = new Texture2D(NumQuadsX, NumQuadsY, TextureFormat.ARGB32, false);
                for (int i = 0; i < NumQuadsX; i++)
                {
                    for (int j = 0; j < NumQuadsY; j++)
                    {
                        texture.SetPixel(i, j, colorArr[i * NumQuadsY + j]);
                    }

                }
            }
            
            //Make sure we're using point filter mode to get nice rectangles
            texture.filterMode = FilterMode.Point;
            // Apply all SetPixel calls
            texture.Apply();
            // connect texture to material of GameObject this script is attached to
            ColorMaterial.SetTexture("_ColorMap", texture);
        }
        void Start()
        {
        }
        void Update()
        {
            timer += Time.deltaTime;
            //change the colors
            if(timer > CycleTime)
            {
                Debug.Log("COLOR UPDATE WAS CALLED at time " + Time.fixedTime  + " and colors[0] was " + colorArr[0]);
                changeColors();
                timer -= CycleTime;
            }
        }
        private void OnRenderObject()
        {
            Graphics.DrawProcedural(MeshTopology.Points, NumQuadsX * NumQuadsY, 1);
        }
    }
}
