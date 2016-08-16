using UnityEngine;
using System.Collections;

public class ComputeDispersion : Compute {

    private int _forceHandle;
    public ComputeShader dispersionShader;


    public override void SetupShader(ParticleManager pm)
    {
        _forceHandle = dispersionShader.FindKernel("ApplyForces");

        dispersionShader.SetBuffer(_forceHandle, "positions", pm.positions);
        dispersionShader.SetBuffer(_forceHandle, "forces", pm.forces);
        dispersionShader.SetBuffer(_forceHandle, "properties", pm.properties);

        dispersionShader.SetFloat("epsilon", 50.0f);
        dispersionShader.SetFloat("sigma", 0.5f);

    }

    public override void UpdateForces(int nx)
    {
        dispersionShader.Dispatch(_forceHandle, nx, 1, 1);
    }
}
