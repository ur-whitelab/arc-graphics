using UnityEngine;
using System.Collections;

namespace Rochester.ARTable.Particles
{

    public class ComputeDispersion : Compute
    {

        private int forceHandle;
        public ComputeShader dispersionShader;
        private ComputeBuffer nlist;
        private ComputeNeighbors nlistComputer;

        public float Strength = 5f;
        public float Cutoff = 0.5f;

        public override void SetupShader(ParticleManager pm)
        {
            forceHandle = dispersionShader.FindKernel("ApplyForces");

            dispersionShader.SetBuffer(forceHandle, "positions", pm.positions);
            dispersionShader.SetBuffer(forceHandle, "forces", pm.forces);
            dispersionShader.SetBuffer(forceHandle, "properties", pm.properties);
            dispersionShader.SetBuffer(forceHandle, "ginfo", pm.ginfo);

            dispersionShader.SetFloat("strength", 100 * Strength);
            dispersionShader.SetFloat("cutoff", Cutoff);

        }

        public override void UpdateForces(int nx)
        {
            if (nlist == null)
            {
                nlistComputer = GameObject.Find("ParticleManager").GetComponentInChildren<ComputeNeighbors>();
                nlist = nlistComputer.NeighborList;
                dispersionShader.SetInt("maxNeighbors", nlistComputer.MaxNeighbors);
                dispersionShader.SetBuffer(forceHandle, "nlist", nlist);

            }
            dispersionShader.Dispatch(forceHandle, nx, 1, 1);
        }
    }
}