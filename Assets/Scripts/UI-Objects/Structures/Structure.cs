using UnityEngine;
using System.Collections;

public abstract class Structure : MonoBehaviour {

    /*
     * Returns true if placement was successful/finished. Returns false if more place calls are needed. 
     */
    public abstract bool Place();

    public abstract bool CanPlace();

    public abstract void CancelPlace();

    public virtual void TryPreview()
    {
        //pass
    }

    public virtual void SetPosition(Vector3 p)
    {
        transform.position = p;
    }
}
