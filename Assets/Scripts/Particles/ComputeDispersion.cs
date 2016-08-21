using UnityEngine;
using System.Collections;

public class ComputeDispersion : Compute {

    private int forceHandle;
    public ComputeShader dispersionShader;


    public override void SetupShader(ParticleManager pm)
    {
        forceHandle = dispersionShader.FindKernel("ApplyForces");

        dispersionShader.SetBuffer(forceHandle, "positions", pm.positions);
        dispersionShader.SetBuffer(forceHandle, "forces", pm.forces);
        dispersionShader.SetBuffer(forceHandle, "properties", pm.properties);

        dispersionShader.SetFloat("epsilon", 50.0f);
        dispersionShader.SetFloat("sigma", 0.5f);

    }

    public override void UpdateForces(int nx)
    {
        dispersionShader.Dispatch(forceHandle, nx, 1, 1);
    }
}
