using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Rochester.ARTable.Particles
{

    //NOTE: These are half-neighbor lists (neighbor index j < index of i)
    public class ComputeNeighbors : Compute
    {

        public float Cutoff = 20f;

        public const float Cutoff2Bin = 1f;
        public float MaxNListMemoryMB = 8f;
        //this is a guess. It will be changed to an even multiple 
        public int BuildDelay = 10;

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
                    nlist = new ComputeBuffer(value * bins.count, ShaderConstants.INT_STRIDE);
                    Neighbors.SetBuffer(buildHandle, "nlist", nlist);
                    Neighbors.SetInt("maxNeighbors", value);
                    //reset check for exceeding neighbors since we updated max neighbor count.
                    Neighbors.SetInts("exceededMaxNeighbors", 0);
                }

            }
        }
        public ComputeShader Neighbors;
        private ParticleSorter sorter;

        private ComputeBuffer bins;
        private int[] cpuBins = new int[2];
        private ComputeBuffer sortedParticles;
        private ComputeBuffer binStarts;
        private ComputeBuffer nlist;
        private ComputeBuffer binOffsets;
        private ComputeBuffer debugPositions;
        private int[] cpuBinOffsets;

        private bool nlistRequested = false;

        public ComputeBuffer NeighborList
        {
            get { nlistRequested = true; return nlist; }
        }

        private int binHandle;
        private int binStartsHandle;
        private int buildHandle;

        private enum States { bin, binStarts, sort, build, pause };
        private States computeState = States.bin;
        private int buildDispatchOffset = 0;
        private int buildPeriod = 0;

        public override void SetupShader(ParticleManager pm)
        {
            binHandle = Neighbors.FindKernel("Bin");
            binStartsHandle = Neighbors.FindKernel("BinStarts");
            buildHandle = Neighbors.FindKernel("Build");

            int N = pm.positions.count;

            sortedParticles = new ComputeBuffer(N, 2 * ShaderConstants.INT_STRIDE);
            bins = new ComputeBuffer(N, ShaderConstants.INT_STRIDE);

            debugPositions = pm.positions;

            //don't want to allocate too much memory yet
            MaxNeighbors = Mathf.Min(256, Mathf.CeilToInt(MaxNListMemoryMB * 1024 * 1024 / (ShaderConstants.INT_STRIDE * N)));
            UnityEngine.Debug.Log("Max Neighbors = " + MaxNeighbors);

            Neighbors.SetBuffer(binHandle, "positions", pm.positions);
            Neighbors.SetBuffer(buildHandle, "positions", pm.positions);

            //for visualizing
            Neighbors.SetBuffer(buildHandle, "properties", pm.properties);
            //for finding dead ones
            Neighbors.SetBuffer(binHandle, "properties", pm.properties);

            Neighbors.SetBuffer(buildHandle, "sortedParticles", sortedParticles);
            Neighbors.SetBuffer(binStartsHandle, "sortedParticles", sortedParticles);

            Neighbors.SetBuffer(binHandle, "bins", bins);
            Neighbors.SetBuffer(buildHandle, "bins", bins);
            Neighbors.SetBuffer(binStartsHandle, "bins", bins);

            //make a sorter
            sorter = new ParticleSorter();

        }



        public override void UpdateBoundary(Vector2 low, Vector2 high)
        {
            //compute bin number

            //try a nice one
            Vector2 tmp = high - low;
            cpuBins[0] = Mathf.CeilToInt(tmp.x / (Cutoff2Bin * Cutoff));
            cpuBins[1] = Mathf.CeilToInt(tmp.y / (Cutoff2Bin * Cutoff));

            //make sure it's not too large for the sorting algorithms
            //add the  + 1 because we save one bin for putting dead particles in.
            while (cpuBins[0] * cpuBins[1] + 1 > ParticleSorter.MAX_SCAN_SIZE)
            {
                cpuBins[0]--;
                cpuBins[1]--;
            }

            Vector2 binDim = new Vector2(tmp.x / cpuBins[0], tmp.y / cpuBins[1]);

            //compute new offsets
            List<int> offsets = new List<int>();
            int maxBinDist = Mathf.CeilToInt(Cutoff / Cutoff2Bin);
            Vector2 r1;
            for (int i = 0; i < maxBinDist; i++)
            {
                for (int j = 0; j < maxBinDist; j++)
                {
                    //see if bin is too far
                    //subtract 1 because the particles could be at the edge of both boxes
                    r1 = new Vector2(Mathf.Max(0, i - 1) * binDim.x, Mathf.Max(0, j - 1) * binDim.y);
                    if ((r1.magnitude) <= Cutoff)
                    {
                        offsets.Add(i + j * cpuBins[0]);
                        //UnityEngine.Debug.Log("Decided that " + i + " " + j + " is close enough to consider");
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
            if (binStarts != null)
                binStarts.Release();
            //Add one because we have extra bin (and so we can mark end of bins).
            binStarts = new ComputeBuffer(cpuBins[0] * cpuBins[1] + 1, ShaderConstants.INT_STRIDE);
            Neighbors.SetBuffer(buildHandle, "binStarts", binStarts);
            Neighbors.SetBuffer(binStartsHandle, "binStarts", binStarts);

        }

        //These just force the CPU/GPU to sync
        private void profiler1()
        {
            binOffsets.GetData(cpuBinOffsets);
        }

        private void profiler2()
        {
            binOffsets.GetData(cpuBinOffsets);
        }

        private void profiler3()
        {
            binOffsets.GetData(cpuBinOffsets);
        }

        private void debugGetAll()
        {
            binOffsets.GetData(cpuBinOffsets);
            var b = new int[bins.count];
            var sp = new int[bins.count, 2];
            var o = new int[binStarts.count];

            bins.GetData(b);
            sortedParticles.GetData(sp);
            binStarts.GetData(o);

            return;
        }


        //We use states here to amoritize the calcualtion.
        //Obviously it would be better to use yields, but that breaks
        //the inheritance. 
        public override void UpdatePostIntegrate(int nx)
        {
            if (!nlistRequested)
                return;

            //calculate how many frames we can spread our particle list building over
            if (buildPeriod == 0)
            {
                //find a divisor of nx that is less than the amount of time we can wait
                // - 3 because we have bin, sort, binstart phase as well
                buildPeriod = 1;
                for (int i = 2; i < BuildDelay - 3; i++)
                    if (nx % i == 0)
                        buildPeriod = i;
            }

            switch (computeState)
            {
                case States.bin:
                    Neighbors.Dispatch(binHandle, nx, 1, 1);
                    computeState = States.sort;
                    break;
                case States.sort:
                    sorter.GPUSortInplace(bins, sortedParticles);
                    //profiler1();
                    computeState = States.binStarts;
                    break;
                case States.binStarts:
                    Neighbors.Dispatch(binStartsHandle, nx, 1, 1);
                    //profiler2();
                    computeState = States.build;
                    break;
                case States.build:
                    //we spread this out over as much of the delay as possible                
                    int n = nx / buildPeriod;

                    Stopwatch stopwatch = new Stopwatch();
                    //profiler3();
                    stopwatch.Start();
                    Neighbors.SetInt("buildOffset", n * buildDispatchOffset * ShaderConstants.PARTICLE_BLOCK_SIZE);
                    Neighbors.Dispatch(buildHandle, n, 1, 1);
                    //profiler3();
                    stopwatch.Stop();
                    if (stopwatch.ElapsedMilliseconds > 40)
                    {
                        UnityEngine.Debug.Log(stopwatch.ElapsedMilliseconds + " " + buildDispatchOffset);
                        UnityEngine.Debug.Log(" " + n * buildDispatchOffset * ShaderConstants.PARTICLE_BLOCK_SIZE + " to " + n * (buildDispatchOffset + 1) * ShaderConstants.PARTICLE_BLOCK_SIZE);
                    }

                    buildDispatchOffset++;
                    //computeState = States.pause;
                    if (buildDispatchOffset == buildPeriod)
                    {
                        buildDispatchOffset = 0;
                        computeState = States.bin;
                    }
                    break;
                case States.pause:
                    computeState = States.build;
                    break;
            }
            return;
        }

        public override void ReleaseBuffers()
        {
            if (sortedParticles != null)
                sortedParticles.Release();

            if (binStarts != null)
                binStarts.Release();

            if (nlist != null)
                nlist.Release();

            if (binOffsets != null)
                binOffsets.Release();

            if (debugPositions != null)
                debugPositions.Release();

            if (bins != null)
                bins.Release();

            sorter.ReleaseBuffers();
        }
    }

}