using UnityEngine;
using System.Collections;

namespace Rochester.ARTable.UI
{

    public class CameraMinimap : MonoBehaviour
    {

        private Camera c;
        public float MinimapFPS = 15f;

        public void Start()
        {
            c = GetComponent<Camera>();
            c.orthographicSize = GameObject.Find("Main Camera").GetComponent<CameraControls>().MaxZoom;
            //someday -> render to texture every n frames and then display that.
            //c.enabled = false;
            //StartCoroutine(DelayedRendering());
        }

        public IEnumerator DelayedRendering()
        {
            while (true)
            {
                c.Render();
                yield return new WaitForSeconds(1f / MinimapFPS);
            }

        }
    }

}