using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ComputeAttractors : Compute
{

    private int forceHandle;

    public ComputeShader AttractorShader;
    public float AttractorOverlapRadius = 2f;

    private ComputeBuffer attractors;
    private List<ShaderConstants.Attractor> cpu_attractors = new List<ShaderConstants.Attractor>();

    


    public override void SetupShader(ParticleManager pm)
    {

        forceHandle = AttractorShader.FindKernel("ApplyForces");

        AttractorShader.SetBuffer(forceHandle, "positions", pm.positions);
        AttractorShader.SetBuffer(forceHandle, "forces", pm.forces);
        AttractorShader.SetBuffer(forceHandle, "properties", pm.properties);
        
    }

    public override void UpdateForces(int nx)
    {
        if (cpu_attractors.Count > 0)
            AttractorShader.Dispatch(forceHandle, nx, 1, 1);
    }

    public int AddAttractor(Vector2 location, float magnitude = 7f)
    {
        int index = cpu_attractors.Count;
        cpu_attractors.Add(new ShaderConstants.Attractor(location, magnitude));
        syncBuffers();

        return index;
    }

    public void UpdateAttractor(int index, Vector2 location, float magnitude = 0)
    {
        if (magnitude == 0)
            magnitude = cpu_attractors[index].magnitude;
        cpu_attractors[index] = new ShaderConstants.Attractor(location, magnitude);
        syncBuffers();
    }

    private void syncBuffers()
    {
        if(attractors != null)
            attractors.Release();
        attractors = new ComputeBuffer(cpu_attractors.Count, ShaderConstants.ATTRACTOR_STRIDE);
        attractors.SetData(cpu_attractors.ToArray());
        AttractorShader.SetBuffer(forceHandle, "attractors", attractors);
    }

    public bool ValidLocation(Vector2 propLocation)
    {
        //this is being asked way too early
        if (cpu_attractors == null)
            return false;

        foreach (ShaderConstants.Attractor a in cpu_attractors)
            if ((propLocation - a.position).sqrMagnitude < AttractorOverlapRadius * AttractorOverlapRadius)
                return false;
        return true;

    }
}
