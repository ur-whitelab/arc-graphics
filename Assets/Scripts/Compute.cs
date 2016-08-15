using UnityEngine;
using System.Collections;

public abstract class Compute : MonoBehaviour
{

    public abstract void setupShader(ParticleManager pm);

    /*
    * The methods below only differ in when they are called. The nx is an optimal threadgroup number if 
    * working on a per-particle quantity. It doesn't have to be the thread number used. 
    */
    public virtual void updatePreIntegrate(int nx)
    {
        return;
    }

    public virtual void updateForces(int nx)
    {
        return;
    }

    public virtual void updatePostIntegrate(int nx)
    {
        return;
    }

    public virtual void releaseBuffers()
    {
        return;
    }
}
