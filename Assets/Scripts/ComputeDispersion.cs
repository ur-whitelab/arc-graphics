using UnityEngine;
using System.Collections;

public class ComputeDispersion : Compute {

    private int _forceHandle;
    public ComputeShader dispersionShader;


    public override void setupShader(ParticleManager pm)
    {

        dispersionShader.SetBuffer(_forceHandle, "positions", pm._positions);
        dispersionShader.SetBuffer(_forceHandle, "forces", pm._forces);
        dispersionShader.SetBuffer(_forceHandle, "properties", pm._properties);

        dispersionShader.SetFloat("epsilon", 10.0f);
        dispersionShader.SetFloat("sigma", 1.0f);

        _forceHandle = dispersionShader.FindKernel("ApplyForces");
    }

    public override void updateForces(int nx)
    {
        dispersionShader.Dispatch(_forceHandle, nx, 1, 1);
    }
}
