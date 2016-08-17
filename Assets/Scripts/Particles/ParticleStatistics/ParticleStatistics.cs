using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class ParticleStatistics : Compute
{
    public ComputeShader PSShader;
    private event EventHandler statisticsCompleted;
    public float StatisticsPeriod = 0.2f;


    private float singleStatPeriod = 1f;

    private int reduceSum1Handle;
    private int reduceSum2Handle;
    private int prepareModifierSumHandle;

    private ComputeBuffer tempSum;
    private ComputeBuffer inputSum;
    private ComputeBuffer result;

    private int particleNumber;
    private List<int[]> modifiers = new List<int[]>();



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
        statisticsCompleted += h;

        //make sure we aren't doing this one already
        foreach (int[] i in modifiers)
            if (i[0] == modifierType && i[1] == modifierIndex)
                return;
        modifiers.Add(new int[] { modifierType, modifierIndex });

        singleStatPeriod = StatisticsPeriod / modifiers.Count;
    }

    public IEnumerator ComputeStatistics()
    {
        for (;;)
        {
            if(modifiers.Count == 0)
                yield return new WaitForSeconds(StatisticsPeriod);
            foreach (int[] i in modifiers)
            {
                //see the shader for details on this
                PSShader.SetInts("modifier", i);
                PSShader.Dispatch(prepareModifierSumHandle, Mathf.CeilToInt((float)particleNumber / ShaderConstants.PARTICLE_BLOCK_SIZE), 1, 1);
                PSShader.Dispatch(reduceSum1Handle, ShaderConstants.REDUCTION_BLOCKSIZE, 1, 1);
                PSShader.Dispatch(reduceSum2Handle, 1, 1, 1);

                //we wait to give time for computation
                yield return new WaitForSeconds(singleStatPeriod / 2);

                //fetch the result
                int[] resultArray = new int[3];
                result.GetData(resultArray);

                //no need to do a null check since we will have called computemodifierstatistics
                statisticsCompleted(this, new ParticleStatisticsModifierEventArgs(i[0], i[1], resultArray));

                //wait the rest of the time
                yield return new WaitForSeconds(singleStatPeriod / 2);
            }
        }
    }
}
