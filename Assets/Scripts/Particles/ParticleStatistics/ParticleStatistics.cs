using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class ParticleStatistics : Compute
{
    public ComputeShader PSShader; 
    public float StatisticsPeriod = 0.2f;

    //I had to pack up the handlers
    //into structs so I can filter the events.
    //not a great system, but I only want to calculate
    //what I need so it's important to keep track of what the callers requested.
    //I can't use structs because it interfers with the += on events
    private class ModifierHandlers
    {
        public event EventHandler h;
        public int modifierType;
        public int modifierIndex;

        
        public ModifierHandlers(int modifierType, int modifierIndex, EventHandler h)
        {
            this.modifierType = modifierType;
            this.modifierIndex = modifierIndex;
            this.h = h;
        }

        public void call(object sender, EventArgs e)
        {
            h(sender, e);
        }
    }

    private class TargetHandlers
    {
        public event EventHandler h;
        public int index;

        public TargetHandlers(int index, EventHandler h)
        {
            this.index = index;
            this.h = h;
        }
        public void call(object sender, EventArgs e)
        {
            h(sender, e);
        }
    }


    private int totalStatsNumber = 0;
    private float singleStatPeriod = 1f;
    private bool doTargetStats = false;

    private int reduceSum1Handle;
    private int reduceSum2Handle;
    private int prepareModifierSumHandle;

    private ComputeBuffer tempSum;
    private ComputeBuffer inputSum;
    private ComputeBuffer result;
    
    private ComputeTargets cTargets;

    private int particleNumber;
    private List<ModifierHandlers> modifiers = new List<ModifierHandlers>();
    private List<TargetHandlers> targets = new List<TargetHandlers>();
    private int[] lastTargetCounts = new int[1];


    public void Awake ()
    {
        cTargets = GameObject.Find("ParticleManager").GetComponentInChildren<ComputeTargets>();
    }

    public override void SetupShader(ParticleManager pm)
    {
        particleNumber = ParticleManager.ParticleNumber;

        tempSum = new ComputeBuffer(ShaderConstants.REDUCTION_BLOCKSIZE, 3 * ShaderConstants.INT_STRIDE);
        //in computes, ints and floats both are 32 bits, so shouldn't matter what I use here
        tempSum.SetData(new Vector3[ShaderConstants.REDUCTION_BLOCKSIZE]);

        inputSum = new ComputeBuffer(particleNumber, 3 * ShaderConstants.INT_STRIDE);
        inputSum.SetData(new Vector3[particleNumber]);

        result = new ComputeBuffer(1, 3 * ShaderConstants.INT_STRIDE);
        result.SetData(new Vector3[1]);

        reduceSum1Handle = PSShader.FindKernel("ReduceSum1");
        reduceSum2Handle = PSShader.FindKernel("ReduceSum2");
        prepareModifierSumHandle = PSShader.FindKernel("PrepareModifierSum");

        PSShader.SetBuffer(reduceSum1Handle, "tempSum", tempSum);
        PSShader.SetBuffer(reduceSum2Handle, "tempSum", tempSum);

        PSShader.SetBuffer(reduceSum2Handle, "result", result);

        PSShader.SetBuffer(reduceSum1Handle, "inputSum", inputSum);
        PSShader.SetBuffer(prepareModifierSumHandle, "inputSum", inputSum);

        PSShader.SetBuffer(prepareModifierSumHandle, "properties", pm.properties);

        //make sure our particle number is consistent
        if (particleNumber % (2 * ShaderConstants.REDUCTION_BLOCKSIZE) != 0)
            new Exception("Particle number is not a multiple of 2 times reduction blocksize. Compute shader will fail");

        PSShader.SetInt("dispatchDim", ShaderConstants.REDUCTION_BLOCKSIZE);

        StartCoroutine(ComputeStatistics());
    }

    public void ComputeModifierStatistics(int modifierType, int modifierIndex, EventHandler h)
    {

        //make sure we aren't doing this one already
        foreach (ModifierHandlers m in modifiers)
        {
            if (m.modifierType == modifierType && m.modifierIndex == modifierIndex)
            {
                m.h += h;
                return;
            }
                
        }

        modifiers.Add(new ModifierHandlers( modifierType, modifierIndex, h));

        totalStatsNumber++;
        singleStatPeriod = StatisticsPeriod / totalStatsNumber;
    }

    public void ComputeTargetStatistics(int targetIndex, EventHandler h)
    {
        if(!doTargetStats)
        {
            doTargetStats = true;
            totalStatsNumber++;
            singleStatPeriod = StatisticsPeriod / totalStatsNumber;
        }

        foreach(TargetHandlers t in targets)
        {
            if(t.index == targetIndex)
            {
                t.h += h;
                return;
            }

        }

        targets.Add(new TargetHandlers(targetIndex, h));
            

    }

    public IEnumerator ComputeStatistics()
    {
        for (;;)
        {
            if(totalStatsNumber == 0)
                yield return new WaitForSeconds(StatisticsPeriod);
            foreach (ModifierHandlers m in modifiers)
            {
                //see the shader for details on this
                PSShader.SetInts("modifier", new int[] { m.modifierType, m.modifierIndex });
                PSShader.Dispatch(prepareModifierSumHandle, Mathf.CeilToInt((float)particleNumber / ShaderConstants.PARTICLE_BLOCK_SIZE), 1, 1);
                PSShader.Dispatch(reduceSum1Handle, ShaderConstants.REDUCTION_BLOCKSIZE, 1, 1);
                PSShader.Dispatch(reduceSum2Handle, 1, 1, 1);

                //we wait to give time for computation
                yield return new WaitForSeconds(singleStatPeriod / 2);

                //fetch the result
                int[] resultArray = new int[3];
                result.GetData(resultArray);

                //no need to do a null check since we will have called computemodifierstatistics
                m.call(this, new ParticleStatisticsModifierEventArgs(m.modifierType, m.modifierIndex, resultArray));

                //wait the rest of the time
                yield return new WaitForSeconds(singleStatPeriod / 2);
            }
            if(doTargetStats)
            {
                int[] counts = cTargets.GetTargetCounts();
                for (int i = 0; i < targets.Count; i++)
                {
                    //only fire if we had a change
                    if(counts[targets[i].index] != lastTargetCounts[targets[i].index])
                        targets[i].call(this, new ParticleStatisticsTargetEventArgs(targets[i].index, counts[targets[i].index]));
                }
                lastTargetCounts = counts;
                yield return new WaitForSeconds(singleStatPeriod);
            }
        }
    }
}
