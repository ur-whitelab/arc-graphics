﻿using UnityEngine;
using System.Collections;

namespace Rochester.ARTable.Particles
{

    public abstract class Compute : MonoBehaviour
    {

        public abstract void SetupShader(ParticleManager pm);

        /*
        * The methods below only differ in when they are called. The nx is an optimal threadgroup number if 
        * working on a per-particle quantity. It doesn't have to be the thread number used. 
        */
        public virtual void UpdatePreIntegrate(int nx)
        {
            return;
        }

        public virtual void UpdateForces(int nx)
        {
            return;
        }

        /*
         *  Will be called at a much longer interval then others
         */
        public virtual IEnumerator SlowUpdate(int nx, float waitTime)
        {
            yield return null;
        }

        public virtual void UpdatePostIntegrate(int nx)
        {
            return;
        }

        public virtual void ReleaseBuffers()
        {
            return;
        }

        public virtual void UpdateBoundary(Vector2 low, Vector2 high)
        {
            return;
        }
    }

}