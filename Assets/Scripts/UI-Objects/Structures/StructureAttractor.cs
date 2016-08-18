﻿using UnityEngine;
using System.Collections;
using System;

public class StructureAttractor : Structure {

    private ComputeAttractors ca;
    private int aIndex = -1;

    public override void CancelPlace()
    {        
        Destroy(gameObject);
    }

    public override bool CanPlace()
    {

        return ca.ValidLocation(new Vector2(transform.position.x, transform.position.y));
    }

    public override void TryPreview()
    {
        Vector2 loc = new Vector2(transform.position.x, transform.position.y);
        bool valid = ca.ValidLocation(loc);
        if (valid)
        {
            //preview it
            if (aIndex < 0)
                Place();
            else
                ca.UpdateAttractor(aIndex, loc);
        }
    }

    public override bool Place()
    {
        aIndex = ca.AddAttractor(new Vector2(this.transform.localPosition.x, this.transform.localPosition.y));
        return true;
    }

    void Awake () {
        ca = GameObject.Find("ParticleManager").GetComponentInChildren<ComputeAttractors>();
    }

}
