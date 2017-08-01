using UnityEngine;
using System.Collections;

public class ComputeGame : Compute
{

    public float IntersectionRadius;
    public ComputeShader intersectionShader;

    private int intersectionsHandle;
    private int explodeHandle;
    private ComputeBuffer nlist;
    private ComputeNeighbors nlistComputer;

    public override void SetupShader(ParticleManager pm)
    {
        
        intersectionsHandle = intersectionShader.FindKernel("Intersections");
        explodeHandle = intersectionShader.FindKernel("TreatExplosions");


	intersectionShader.SetBuffer(intersectionsHandle, "positions", pm.positions);;
        intersectionShader.SetBuffer(intersectionsHandle, "properties", pm.properties);
        intersectionShader.SetBuffer(intersectionsHandle, "ginfo", pm.ginfo);
        intersectionShader.SetBuffer(explodeHandle, "properties", pm.properties);
        intersectionShader.SetBuffer(explodeHandle, "ginfo", pm.ginfo);
        intersectionShader.SetFloat("cutoff", IntersectionRadius);
        intersectionShader.SetFloat("explodeTime", pm.ExplodeTime);
    }

    public override void UpdatePostIntegrate(int nx)
    {
        if (nlist == null)
        {
            nlistComputer = GameObject.Find("ParticleManager").GetComponentInChildren<ComputeNeighbors>();
            nlist = nlistComputer.NeighborList;
            intersectionShader.SetInt("maxNeighbors", nlistComputer.MaxNeighbors);
            intersectionShader.SetBuffer(intersectionsHandle, "nlist", nlist);

        }
        intersectionShader.Dispatch(intersectionsHandle, nx, 1, 1);
        intersectionShader.Dispatch(explodeHandle, nx, 1, 1);
    }
}
