using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rochester.ARTable.Particles
{

    public class ParticleAdder
    {

        private ComputeShader SumShader;
        private int reduceSum1Handle;
        private int reduceSum2Handle;
        private int threadGroupsX;

        private ComputeBuffer input;
        private ComputeBuffer tempSum;
        private ComputeBuffer result;

        /*
         *  Sums input, a set of N M-tuple integers and outputs an M-tuple integer.
         */
        public ParticleAdder(ComputeBuffer input, ComputeBuffer result, int N, int M)
        {
            SumShader = (ComputeShader) Resources.Load("ComputeShaders/Reduce");

            if (N % ShaderConstants.REDUCTION_BLOCKSIZE != 0 && N > 2 * ShaderConstants.REDUCTION_BLOCKSIZE)
                new Exception("Particle number is not a multiple of 2 times reduction blocksize. Compute shader will fail");            
            if(M > 3)
                new Exception("Reduction Shader is hardcoded to int3");
            this.input = input;
            this.result = result;
            threadGroupsX = Math.Min(N / ShaderConstants.REDUCTION_BLOCKSIZE / 2, ShaderConstants.REDUCTION_BLOCKSIZE);

            tempSum = new ComputeBuffer(threadGroupsX, 3 * ShaderConstants.INT_STRIDE);
            //in computes, ints and floats both are 32 bits, so shouldn't matter what I use here
            tempSum.SetData(new Vector3[threadGroupsX]);

            reduceSum1Handle = SumShader.FindKernel("ReduceSum1");
            reduceSum2Handle = SumShader.FindKernel("ReduceSum2");
            SumShader.SetBuffer(reduceSum1Handle, "inputSum", input);
            SumShader.SetBuffer(reduceSum1Handle, "tempSum", tempSum);
            SumShader.SetBuffer(reduceSum2Handle, "tempSum", tempSum);
            SumShader.SetBuffer(reduceSum2Handle, "result", result);            
            SumShader.SetInt("dispatchDim", threadGroupsX);
        }

        public void Compute()
        {
            SumShader.Dispatch(reduceSum1Handle, threadGroupsX, 1, 1);            
            SumShader.Dispatch(reduceSum2Handle, 1, 1, 1);
        }

        public int[,] HalfCompute()
        {
            SumShader.Dispatch(reduceSum1Handle, threadGroupsX, 1, 1);
            int[,] result = new int[threadGroupsX, 3];
            tempSum.GetData(result);
            return result;
        }

        public void ReleaseBuffers()
        {
            tempSum.Release();
        }
    }
}
