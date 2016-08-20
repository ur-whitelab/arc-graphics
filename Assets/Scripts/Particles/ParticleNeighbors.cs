using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class ParticleNeighbors : Compute {

    public float Cutoff;

    public const float Cutoff2Bin = 0.5f;
    public float MaxNListMemoryMB = 0.5f;

    private int maxNeighbors;
    public int MaxNeighbors
    {
        get { return maxNeighbors; }
        set
        {
            maxNeighbors = value;
            if (nlist != null)
                nlist.Release();
            if (sortedParticles != null)
            {
                nlist = new ComputeBuffer(value * sortedParticles.count, ShaderConstants.INT_STRIDE);
                Neighbors.SetBuffer(buildHandle, "nlist", nlist);
                Neighbors.SetInt("maxNeighbors", value);
                //reset check for exceeding neighbors since we updated max neighbor count.
                Neighbors.SetInts("exceededMaxNeighbors", 0);
            }

        }
    }
    public ComputeShader Neighbors;
    public ComputeSort Sorter;

    private ComputeBuffer bins;
    private int[] cpuBins = new int[2];
    private ComputeBuffer sortedParticles;
    private ComputeBuffer binStarts;
    private ComputeBuffer nlist;
    private ComputeBuffer binOffsets;
    private int[] cpuBinOffsets;

    private bool nlistRequested = false;

    public ComputeBuffer NeighborList
    {
        get { nlistRequested = true;  return nlist; }
    }

    private int binHandle;
    private int binStartsHandle;
    private int buildHandle;

    public override void SetupShader(ParticleManager pm)
    {
        binHandle = Neighbors.FindKernel("Bin");
        binStartsHandle = Neighbors.FindKernel("BinStarts");
        buildHandle = Neighbors.FindKernel("Build");

        int N = pm.positions.count;

        sortedParticles = new ComputeBuffer(N, ShaderConstants.INT_STRIDE);
        bins = new ComputeBuffer(N, ShaderConstants.INT_STRIDE);

        //don't want to allocate too much memory yet
        MaxNeighbors = Mathf.CeilToInt(MaxNListMemoryMB * 1024 * 1024 /  (ShaderConstants.INT_STRIDE * N)); 


        Neighbors.SetBuffer(binHandle, "positions", pm.positions);
        Neighbors.SetBuffer(buildHandle, "positions", pm.positions);

        //for visualizing
        Neighbors.SetBuffer(buildHandle, "properties", pm.properties);

        Neighbors.SetBuffer(buildHandle, "sortedParticles", sortedParticles);
        Neighbors.SetBuffer(binStartsHandle, "sortedParticles", sortedParticles);

        //find our sorter
        Sorter = GameObject.Find("ParticleManagerLib").GetComponentInChildren<ComputeSort>();
    }

    

    public override void UpdateBoundary(Vector2 low, Vector2 high)
    {
        //compute bin number
        Vector2 tmp = high - low;
        cpuBins[0] = Mathf.CeilToInt(tmp.x / (Cutoff2Bin * Cutoff));
        cpuBins[1] = Mathf.CeilToInt(tmp.y / (Cutoff2Bin * Cutoff));
        Vector2 binDim = new Vector2( tmp.x / cpuBins[0], tmp.y / cpuBins[1]);

        //compute new offsets
        List<int> offsets = new List<int>();
        int maxBinDist = Mathf.CeilToInt(Cutoff / Cutoff2Bin);
        Vector2 r1;
        for (int i = 0; i < maxBinDist; i++)
        {
            for (int j = 0; j < maxBinDist; j++)
            {
                if (i == 0 && j == 0)
                    continue;
                //see if bin is too far
                //add the bindim term for diagonals
                r1 = new Vector2(i * binDim.x, j * binDim.y);
                if (r1.magnitude + binDim.magnitude < Cutoff)
                {
                    offsets.Add(i + j * cpuBins[0]);
                    Debug.Log("Decided that " + i + " " + j + " is close enough to consider");
                }

            }
        }
        cpuBinOffsets = offsets.ToArray();


        //update these quantities with shader       
        //update cutoff because now is the only time we can update it (when we have the new offsets)
        Neighbors.SetFloat("cutoff", Cutoff);
        Neighbors.SetFloats("boundaryMax", new float[] { high.x, high.y });
        Neighbors.SetFloats("boundaryMin", new float[] { low.x, low.y });
        Neighbors.SetInts("binNumber", cpuBins);
        Neighbors.SetInts("exceededMaxNeighbors", 0); //reset chcek of max neighbors since we changed geometry

        //update buffers with new values
        if (binOffsets != null)
            binOffsets.Release();
        binOffsets = new ComputeBuffer(cpuBinOffsets.Length, ShaderConstants.INT_STRIDE);
        binOffsets.SetData(cpuBinOffsets);

        //set the buffers
        Neighbors.SetBuffer(buildHandle, "binOffsets", binOffsets);

        //now update buffer sizes that changed
        if (bins != null)
            bins.Release();
        binStarts = new ComputeBuffer(cpuBins[0] * cpuBins[1], ShaderConstants.INT_STRIDE);
        Neighbors.SetBuffer(buildHandle, "binStarts", binStarts);
        Neighbors.SetBuffer(binStartsHandle, "binStarts", binStarts);
    }

    public override void UpdatePostIntegrate(int nx)
    {
        if (!nlistRequested) //replace this with a state thing
            return;
        Neighbors.Dispatch(binHandle, nx, 1, 1);

        //sort the particles
        Sorter.GPUSortInplace(bins, sortedParticles);

        //get bin starts
        Neighbors.Dispatch(binStartsHandle, nx, 1, 1);

        //finally build neighbor list
        Neighbors.Dispatch(buildHandle, nx, 1, 1);
    }
}
