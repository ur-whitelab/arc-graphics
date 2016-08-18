using UnityEngine;
using System.Collections;

public class StructureTarget : MonoBehaviour {

	void Start () {
        ComputeTargets ct = GameObject.Find("ParticleManager").GetComponentInChildren<ComputeTargets>();
        ct.AddTarget(new Vector2(transform.position.x, transform.position.y));
    }
}
