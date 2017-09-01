using UnityEngine;
using System.Collections;
using Rochester.ARTable.Particles;

namespace Rochester.ARTable.UI
{
    public class World : MonoBehaviour
    {

        public Vector2 boundariesLow;
        public Vector2 boundariesHigh;
        private Vector2 _size;
        public Vector2 size {
            get {
                return _size;
            }
        }
        private new Collider2D collider;

        // Use this for initialization
        void Awake()
        {
            if (boundariesHigh.Equals(boundariesLow))
            {
                Vector3 bounds_min = GetComponent<Collider2D>().bounds.min;
                Vector3 bounds_max = GetComponent<Collider2D>().bounds.max;
                boundariesLow = new Vector2(bounds_min.x, bounds_min.y);
                boundariesHigh = new Vector2(bounds_max.x, bounds_max.y);
                this._size = boundariesHigh - boundariesLow;
            }

            collider = GetComponent<Collider2D>();
        }


        void Start()
        {
            GameObject pm = GameObject.Find("ParticleManager");
            if(pm)
            {
                pm.GetComponent<ParticleManager>().updateParticleBoundary(boundariesLow, boundariesHigh);
                //deal with editor placed objects
                ComputeAttractors ca = GameObject.Find("ParticleManager").GetComponentInChildren<ComputeAttractors>();
                foreach (GameObject g in GameObject.FindGameObjectsWithTag("Attractor"))
                {
                    ca.AddAttractor(new Vector2(g.transform.localPosition.x, g.transform.localPosition.y));
                }
            }


        }

        /*
         * Returns the mouse position or zero if it can't find it.
         */
        public Vector3 GetMousePosition()
        {
            /*
            RaycastHit hit;
            if (collider.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 1000.0f))
            {
                return hit.point;
            }

            //return center of screen otherwise. Rely on clamping
            if (collider.Raycast(Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0)), out hit, 1000.0f))
            {
                return hit.point;
            }
            */
            return Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

    }

}