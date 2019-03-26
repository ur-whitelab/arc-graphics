using UnityEngine;
using System.Collections;
using Rochester.ARTable.Particles;

namespace Rochester.ARTable.UI
{

    public class StructureTarget : MonoBehaviour
    {

        void Start()
        {
            ComputeTargets ct = GameObject.Find("ParticleManager").GetComponentInChildren<ComputeTargets>();
            ct.AddTarget(new Vector2(transform.position.x, transform.position.y));
        }
    }

}