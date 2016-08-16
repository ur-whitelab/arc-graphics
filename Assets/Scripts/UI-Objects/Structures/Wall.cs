using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class Wall : Structure {

    public float PlacementRadius = 1f;
    public int MaxPoints = 10;
    private LineRenderer lr;
    private List<Vector3> positions;

    // Use this for initialization
    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        positions = new List<Vector3>();
        positions.Add(transform.position);
        lr.SetVertexCount(positions.Count);
        lr.SetPositions(positions.ToArray());
    }

    public override bool CanPlace()
    {
        //the last one is the one being considered for placement.
        for(int i = 0; i < positions.Count - 1; i++)
            if ((positions[i] - positions[positions.Count - 1]).sqrMagnitude < PlacementRadius * PlacementRadius)
                return false;
        return true;
    }

    public override bool Place()
    {
        //The last position in array is one we're locking in.        
        if (positions.Count == MaxPoints)
        {
            CancelPlace();
            return true;
        }

        //we can place another, so get it ready
        positions.Add(transform.position);
        lr.SetVertexCount(positions.Count);
        lr.SetPositions(positions.ToArray());

        return false;
    }


    public override void CancelPlace()
    {
        //remove last point unless we finished due to having enough points
        if (positions.Count != MaxPoints)
        {
            positions.RemoveAt(positions.Count - 1);
            lr.SetVertexCount(positions.Count);
            lr.SetPositions(positions.ToArray());
        }
        //TODO: Load into buffer
    }

    public override void SetPosition(Vector3 p)
    {
        positions[positions.Count - 1] = p;
        lr.SetVertexCount(positions.Count);
        lr.SetPositions(positions.ToArray());
    }
}
