using UnityEngine;
using System.Collections;
using System;

public class StructureAttractor : Structure {

    private ComputeAttractors ca;

    public override bool CanEnableInteractions()
    {
        return ca.ValidLocation(new Vector2(transform.position.x, transform.position.y));
    }

    public override void EnableInteractions()
    {
        ca.AddAttractor(new Vector2(this.transform.localPosition.x, this.transform.localPosition.y));
    }

    void Awake () {
        ca = GameObject.Find("ComputeAttractors").GetComponent<ComputeAttractors>();
    }

}
