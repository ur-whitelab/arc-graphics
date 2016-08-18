using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ComputeAttractors : Compute
{

    private int forceHandle;

    public ComputeShader AttractorShader;
    public float AttractorOverlapRadius = 5f;

    private ComputeBuffer attractors;
    private List<ShaderConstants.Attractor> cpu_attractors = null;

    


    public override void SetupShader(ParticleManager pm)
    {

        forceHandle = AttractorShader.FindKernel("ApplyForces");

        AttractorShader.SetBuffer(forceHandle, "positions", pm.positions);
        AttractorShader.SetBuffer(forceHandle, "forces", pm.forces);
        AttractorShader.SetBuffer(forceHandle, "properties", pm.properties);

        attractors = new ComputeBuffer(1, ShaderConstants.ATTRACTOR_STRIDE);
        ShaderConstants.Attractor[] dummy = new ShaderConstants.Attractor[1];
        //default values will have 0 mag
        attractors.SetData(dummy);
        cpu_attractors = new List<ShaderConstants.Attractor>();

        AttractorShader.SetBuffer(forceHandle, "attractors", attractors);
        
    }

    public override void UpdateForces(int nx)
    {
        AttractorShader.Dispatch(forceHandle, nx, 1, 1);
    }

    public void AddAttractor(Vector2 location, float magnitude = 5f)
    {
        cpu_attractors.Add(new ShaderConstants.Attractor(location, magnitude));
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
