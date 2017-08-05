using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;


namespace Rochester.ARTable.Particles
{

    public class ParticleStatistics : Compute
    {
        public ComputeShader PSShader;       

        //I had to pack up the handlers
        //into structs so I can filter the events.
        //not a great system, but I only want to calculate
        //what I need so it's important to keep track of what the callers requested.
        //I can't use structs because it interfers with the += on events
        //modifier type/index refer to things manipulating a praticle's state
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


        private int totalStatsNumber = 0;
        private float singleStatPeriod = 1f;


        private int prepareModifierSumHandle;

        private ComputeBuffer inputSum;
        private ComputeBuffer result;


        private int particleNumber;
        private ParticleAdder adder;
        private List<ModifierHandlers> modifiers = new List<ModifierHandlers>();


        public override void ReleaseBuffers()
        {
            if (inputSum != null)
            {
                inputSum.Release();
            }
            if (result != null)
            {
                result.Release();
            }

            adder.ReleaseBuffers();
        }

        public override void SetupShader(ParticleManager pm)
        {
            particleNumber = pm.ParticleNumber;

            inputSum = new ComputeBuffer(particleNumber, 3 * ShaderConstants.INT_STRIDE);
            inputSum.SetData(new Vector3[particleNumber]);

            result = new ComputeBuffer(1, 3 * ShaderConstants.INT_STRIDE);
            result.SetData(new Vector3[1]);

            prepareModifierSumHandle = PSShader.FindKernel("PrepareModifierSum");
            PSShader.SetBuffer(prepareModifierSumHandle, "inputSum", inputSum);
            PSShader.SetBuffer(prepareModifierSumHandle, "properties", pm.properties);

            adder = new ParticleAdder(inputSum, result, particleNumber, 3);
        }

        public void ComputeModifierStatistics(EventHandler h)
        {
            this.ComputeModifierStatistics(-1, -1, h);
        }

        public void ComputeModifierStatistics(int modifierType, EventHandler h)
        {
            this.ComputeModifierStatistics(modifierType, -1, h);
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

            modifiers.Add(new ModifierHandlers(modifierType, modifierIndex, h));

            totalStatsNumber++;
        }


        public override IEnumerator SlowUpdate(int nx, float waitTime)
        {

            if (totalStatsNumber > 0)
            {
                ModifierHandlers m;
                for (int i = 0; i < modifiers.Count; i++)
                {
                    m = modifiers[i];
                    //run mask over all particles to prepare input sum
                    //see the shader for details on this
                    PSShader.SetInts("modifier", new int[] { m.modifierType, m.modifierIndex });
                    PSShader.Dispatch(prepareModifierSumHandle, Mathf.CeilToInt((float)particleNumber / ShaderConstants.PARTICLE_BLOCK_SIZE), 1, 1);
                    adder.Compute();
                    //Now sum over all particles

                    //we wait to give time for computation
                    yield return new WaitForSeconds(waitTime);

                    //fetch the result
                    int[] resultArray = new int[3];
                    result.GetData(resultArray);

                    //no need to do a null check since we will have called computemodifierstatistics
                    m.call(this, new ParticleStatisticsModifierEventArgs(m.modifierType, m.modifierIndex, resultArray));

                    //wait the rest of the time
                    yield return new WaitForSeconds(waitTime);
                }
            }
        }
    }

}