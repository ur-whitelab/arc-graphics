using UnityEngine;
using System.Collections;

public class ComputeGravity : Compute
{

    
    public ComputeShader gravityShader;
    public float GravityStrength;

    private int forceHandle;
    private ComputeBuffer nlist;
    private ComputeNeighbors nlistComputer;

    public override void SetupShader(ParticleManager pm)
    {
        forceHandle = gravityShader.FindKernel("ApplyForces");

        gravityShader.SetBuffer(forceHandle, "positions", pm.positions);
        gravityShader.SetBuffer(forceHandle, "forces", pm.forces);
        gravityShader.SetBuffer(forceHandle, "properties", pm.properties);
        gravityShader.SetBuffer(forceHandle, "ginfo", pm.ginfo);
        gravityShader.SetFloat("strength", GravityStrength * 1E0f);
    }

    public override void UpdateForces(int nx)
    {
        if(nlist == null)
        {
            nlistComputer = GameObject.Find("ParticleManager").GetComponentInChildren<ComputeNeighbors>();
            nlist = nlistComputer.NeighborList;
            gravityShader.SetInt("maxNeighbors", nlistComputer.MaxNeighbors);
            gravityShader.SetFloat("cutoff", nlistComputer.Cutoff * 0.85f); //use 85% since nlist isn't rebuild each step
            gravityShader.SetBuffer(forceHandle, "nlist", nlist);
            
        }
        gravityShader.Dispatch(forceHandle, nx, 1, 1);
    }
}
