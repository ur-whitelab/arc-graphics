using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

[ExecuteInEditMode]
public class StructureWall : Structure {

    public float PlacementRadius = 1f;
    public int MaxPoints = 10;
    private LineRenderer lr;
    [SerializeField]
    public List<Vector3> Positions;
    private ComputeWalls cw;

    public void Awake()
    {
        lr = GetComponent<LineRenderer>();
        cw = GameObject.Find("ComputeWalls").GetComponent<ComputeWalls>();

        if (Positions.Count == 0)
        {
            Positions = new List<Vector3>();
            Positions.Add(transform.position);
        }

        lr.SetVertexCount(Positions.Count);
        lr.SetPositions(Positions.ToArray());
    }

    public void Start()
    {

        if (cw != null && Positions.Count > 1)
        {
            //one was already set-up. Go ahead and place if we can find cw
            cw.AddWall(Positions.ToArray());
        }
    }


    public void AddPosition(Vector3 p)
    {
        p.z = 0;
        Debug.Log("Adding " + p.x + " " + p.y);
        Positions.Add(p);
    }

    public void DeletePosition()
    {
        Positions.RemoveAt(Positions.Count - 1);
        lr.SetVertexCount(Positions.Count);
        lr.SetPositions(Positions.ToArray());
    }


    public override bool CanPlace()
    {
        //the last one is the one being considered for placement.
        for(int i = 0; i < Positions.Count - 1; i++)
            if ((Positions[i] - Positions[Positions.Count - 1]).sqrMagnitude < PlacementRadius * PlacementRadius)
                return false;
        return true;
    }

    public override bool Place()
    {
        //The last position in array is one we're locking in.        
        if (Positions.Count == MaxPoints)
        {
            CancelPlace();
            return true;
        }

        //we can place another, so get it ready
        Positions.Add((Positions[Positions.Count - 1]));
        lr.SetVertexCount(Positions.Count);
        lr.SetPositions(Positions.ToArray());

        return false;
    }


    public override void CancelPlace()
    {
        //remove last point unless we finished due to having enough points
        if (Positions.Count != MaxPoints)
        {
            Positions.RemoveAt(Positions.Count - 1);
            lr.SetVertexCount(Positions.Count);
            lr.SetPositions(Positions.ToArray());
        }

        //now add to compute
        if(Positions.Count > 1)
            cw.AddWall(Positions.ToArray());
    }

    public override void SetPosition(Vector3 p)
    {
        p.z = 0;
        Positions[Positions.Count - 1] = p;
        lr.SetVertexCount(Positions.Count);
        lr.SetPositions(Positions.ToArray());
        //Debug.Log("Setting to " + p.x + " " + p.y);
    }


    public Vector3 LastPosition()
    {
        return Positions[Positions.Count - 1];
    }
}
