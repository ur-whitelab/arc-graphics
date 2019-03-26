using UnityEngine;
using System.Collections;

namespace Rochester.ARTable.UI
{

    public abstract class Structure : MonoBehaviour
    {

        /*
         * Returns true if placement was successful/finished. Returns false if more place calls are needed. 
         */
        public abstract bool Place();

        public abstract bool CanPlace();

        public abstract void CancelPlace();

        public virtual void StartPlace()
        {
            // presumes default is to start
        }

        public virtual void TryPreview()
        {
            //pass
        }

        public virtual void SetPosition(Vector3 p)
        {
            transform.position = p;
        }
    }

}