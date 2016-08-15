using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ComputeAttractors : Compute
{

    private int _forceHandle;
    public ComputeShader AttractorShader;

    private ComputeBuffer _attractors;
    private List<ShaderConstants.Attractor> _cpu_attractors;


    public override void setupShader(ParticleManager pm)
    {

        _forceHandle = AttractorShader.FindKernel("ApplyForces");

        AttractorShader.SetBuffer(_forceHandle, "positions", pm._positions);
        AttractorShader.SetBuffer(_forceHandle, "forces", pm._forces);
        AttractorShader.SetBuffer(_forceHandle, "properties", pm._properties);

        _attractors = new ComputeBuffer(1, ShaderConstants.ATTRACTOR_STRIDE);
        ShaderConstants.Attractor[] dummy = new ShaderConstants.Attractor[1];
        //default values will have 0 mag
        _attractors.SetData(dummy);
        _cpu_attractors = new List<ShaderConstants.Attractor>();

        AttractorShader.SetBuffer(_forceHandle, "attractors", _attractors);
        
    }

    public override void updateForces(int nx)
    {
        AttractorShader.Dispatch(_forceHandle, nx, 1, 1);
    }

    public void addAttractor(Vector2 location, float magnitude = 5f)
    {
        _cpu_attractors.Add(new ShaderConstants.Attractor(location, magnitude));
        _attractors.Release();
        _attractors = new ComputeBuffer(_cpu_attractors.Count, ShaderConstants.ATTRACTOR_STRIDE);
        _attractors.SetData(_cpu_attractors.ToArray());
        AttractorShader.SetBuffer(_forceHandle, "attractors", _attractors);
    }
}
