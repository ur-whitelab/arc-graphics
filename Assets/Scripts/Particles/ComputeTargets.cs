using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Rochester.ARTable.Particles
{

    public class ComputeTargets : Compute
    {


        private int targetsHandle;

        public ComputeShader TargetsShader;

        private ComputeBuffer targets;
        private ComputeBuffer targetCounts;

        private List<ShaderConstants.Target> cpu_targets = new List<ShaderConstants.Target>();
        private List<int> cpu_targetCounts = new List<int>();



        public override void SetupShader(ParticleManager pm)
        {

            targetsHandle = TargetsShader.FindKernel("Targets");

            TargetsShader.SetBuffer(targetsHandle, "positions", pm.positions);
            TargetsShader.SetBuffer(targetsHandle, "properties", pm.properties);

        }

        public override void ReleaseBuffers()
        {
            if (targets != null)
                targets.Release();
            if (targetCounts != null)
                targetCounts.Release();
        }

        public override void UpdateForces(int nx)
        {
            if (cpu_targets.Count > 0)
                TargetsShader.Dispatch(targetsHandle, nx, 1, 1);
        }

        public void AddTarget(Vector2 location, float radius = 1.5f)
        {
            cpu_targets.Add(new ShaderConstants.Target(location, radius));
            cpu_targetCounts.Add(0);

            if (targets != null)
                targets.Release();
            targets = new ComputeBuffer(cpu_targets.Count, ShaderConstants.TARGET_STRIDE);
            targets.SetData(cpu_targets.ToArray());
            TargetsShader.SetBuffer(targetsHandle, "targets", targets);

            if (targetCounts != null)
                targetCounts.Release();
            targetCounts = new ComputeBuffer(cpu_targetCounts.Count, ShaderConstants.INT_STRIDE);
            targetCounts.SetData(cpu_targetCounts.ToArray());
            TargetsShader.SetBuffer(targetsHandle, "targetCounts", targetCounts);
        }


        public int[] GetTargetCounts()
        {
            int[] data = cpu_targetCounts.ToArray();
            targetCounts.GetData(data);
            return data;
        }
    }

}