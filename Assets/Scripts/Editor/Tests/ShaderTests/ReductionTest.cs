using UnityEngine;
using System.Collections;
using NUnit.Framework;
using Rochester.ARTable.Particles;

[TestFixture]
public class ParticleReductionTest : MonoBehaviour
{

    ParticleStatistics ps;
    ParticleManager pm;

    [SetUp]
    public void Init()
    {
        
    }

    [Test]
    public void Reduce1()
    {
        int N = 2 * ShaderConstants.REDUCTION_BLOCKSIZE;
        ComputeBuffer input = new ComputeBuffer(N, 3 * ShaderConstants.INT_STRIDE);
        ComputeBuffer output = new ComputeBuffer(1, 3 * ShaderConstants.INT_STRIDE);
        int[,] array = new int[N, 3];
        for (int i = 0; i < N; i++)
            array[i,0] = 1;
        input.SetData(array);

        ParticleAdder pa = new ParticleAdder(input, output, N, 3);
        int[,] result = pa.HalfCompute();
        UnityEngine.Debug.Log("" + result[0, 0]);
        //sum it manually
        int[] final_sum = { 0, 0, 0 };
        for (int i = 0; i < 1; i++)
            for (int j = 0; j < 3; j++)
                final_sum[j] += result[i, j];
        Assert.That(final_sum[0], Is.EqualTo(N));

        input.Release();
        output.Release();
    }

    [Test]
    public void Sum2Pow()
    {
        int[] Ms = { 1, 2, 3 };
        int[] Ns = { 2 * ShaderConstants.REDUCTION_BLOCKSIZE, 4 * ShaderConstants.REDUCTION_BLOCKSIZE, 6 * ShaderConstants.REDUCTION_BLOCKSIZE };
        //make an array to test
        foreach (var M in Ms)
        {
            foreach (var N in Ns)
            {
                int[,] array = new int[N, M];
                int[] expected = new int[M];
                for (int i = 0; i < N; i++)
                {
                    for (int j = 0; j < M; j++)
                    {
                        array[i,j] = i * i;
                        expected[j] += i * i;
                    }
                }
                
                ComputeBuffer input = new ComputeBuffer(N, M * ShaderConstants.INT_STRIDE);
                ComputeBuffer output = new ComputeBuffer(1, M * ShaderConstants.INT_STRIDE);


                ParticleAdder pa = new ParticleAdder(input, output, N, M);
                input.SetData(array);
                pa.Compute();
                int[] result = { -1, -1, -1 };
                output.GetData(result);

                for (int i = 0; i < M; i++)
                {
                    Assert.That(result[i], Is.EqualTo(expected[i]), "On element " + i + " with N = " + N + ", M = " + M);
                }

                input.Release();
                output.Release();
            }
        }        
    }

}
