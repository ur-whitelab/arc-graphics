using UnityEngine;
using System.Collections;

public class StructureAttractor : MonoBehaviour {

    private ComputeAttractors ca;

    void Start () {
        if (ca == null)
            ca = GameObject.Find("ComputeAttractors").GetComponent<ComputeAttractors>();
        if(ca != null)
            ca.addAttractor(new Vector2(this.transform.localPosition.x, this.transform.localPosition.y));
    }
}
