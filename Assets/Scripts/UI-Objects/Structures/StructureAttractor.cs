using UnityEngine;
using System.Collections;
using System;

public class StructureAttractor : Structure {

    private ComputeAttractors ca;

    public override void CancelPlace()
    {
        Destroy(this);
    }

    public override bool CanPlace()
    {
        return ca.ValidLocation(new Vector2(transform.position.x, transform.position.y));
    }

    public override bool Place()
    {
        ca.AddAttractor(new Vector2(this.transform.localPosition.x, this.transform.localPosition.y));
        return true;
    }

    void Awake () {
        ca = GameObject.Find("ComputeAttractors").GetComponent<ComputeAttractors>();
    }

}
