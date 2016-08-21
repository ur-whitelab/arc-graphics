using UnityEngine;
using System.Collections;

public class ComputeAlign : Compute
{

    private int forceHandle;
    public ComputeShader alignShader;
    private ComputeBuffer nlist;
    private ComputeNeighbors nlistComputer;

    public float Strength = 5;

    public override void SetupShader(ParticleManager pm)
    {
        forceHandle = alignShader.FindKernel("ApplyForces");

        alignShader.SetBuffer(forceHandle, "positions", pm.positions);
        alignShader.SetBuffer(forceHandle, "velocities", pm.velocities);
        alignShader.SetBuffer(forceHandle, "forces", pm.forces);
        alignShader.SetBuffer(forceHandle, "properties", pm.properties);
        alignShader.SetBuffer(forceHandle, "ginfo", pm.ginfo);

        alignShader.SetFloat("strength", Strength / 50);        
    }

    public override void UpdateForces(int nx)
    {
        if (nlist == null)
        {
            nlistComputer = GameObject.Find("ParticleManager").GetComponentInChildren<ComputeNeighbors>();
            nlist = nlistComputer.NeighborList;
            alignShader.SetInt("maxNeighbors", nlistComputer.MaxNeighbors);
            alignShader.SetFloat("cutoff", nlistComputer.Cutoff * 0.85f); //use 85% since nlist isn't rebuild each step
            alignShader.SetBuffer(forceHandle, "nlist", nlist);

        }
        alignShader.Dispatch(forceHandle, nx, 1, 1);
    }
}
