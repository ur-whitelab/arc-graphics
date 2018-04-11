using UnityEngine;
using System.Collections;
using NUnit.Framework;
using Rochester.ARTable.Particles;

[TestFixture]
public class ComputeSortTest : MonoBehaviour {

    ParticleSorter cs;

    [SetUp]
    public void Init()
    {
        cs = new ParticleSorter();
    }

    [TearDown]
    public void OnDestroy()
    {
        cs.ReleaseBuffers();
    }

    [Test]
    public void GPUScan()
    {
        //make an array to test
        int N = ParticleSorter.MAX_SCAN_SIZE;
        int[] array = new int[N];
        int[] expected = new int[N];
        for (int i = 0; i < 125; i++)
        {
            array[i] = i * i;
            expected[i] = array[i];
        }

        cs.CPUScan(ref expected);
        cs.GPUScan(ref array);

        for (int i = 0; i < N; i++)
        {
            Assert.That(array[i], Is.EqualTo(expected[i]), "On element " + i);
        }
        
    }

    [Test]
    public void GPUCount()
    {
        //make an array to test
        uint N = ParticleSorter.MAX_SCAN_SIZE;
        int[] input = new int[N];
        int[] gpu = new int[N];
        int[] cpu = new int[N];
        for (int i = 0; i < N; i++)
        {
            input[i] = (int)Random.Range(0, N);
        }


        cs.CPUCount(input, ref cpu);
        cs.GPUCount(input, ref gpu);

        for (int i = 0; i < N; i++)
        {
            Assert.That(gpu[i], Is.EqualTo(cpu[i]), "On element " + i);
        }

    }

    [Test]
    public void GPUSort()
    {
        uint N = 512;
        int[] gpu = new int[N];
        int[] cpu = new int[N];
        
        for (int i = 0; i < N; i++)
        {
            gpu[i] = Random.Range(0, ParticleSorter.MAX_SCAN_SIZE);
            cpu[i] = gpu[i];
        }

        System.Array.Sort(cpu);
        cs.GPUSort(ref gpu,true);
        //call it twice to make sure it doesn't change behavior
        cs.GPUSort(ref gpu, true);

        for (int i = 0; i < N; i++)
        {
            Assert.That(gpu[i], Is.EqualTo(cpu[i]), "On element " + i);
        }
        
    }

    [Test]
    public void GPUInPlaceSort()
    {
        int N = 256;
        int[] gpu = new int[N];
        int[] cpu = new int[N];

        ComputeBuffer input = new ComputeBuffer(N, 4);
        ComputeBuffer output = new ComputeBuffer(N, 4 * 2);

        //try doing it 5 times.
        
        for(int j = 0; j < 5; j++)
        {
            for (int i = 0; i < N; i++)
            {
                gpu[i] = Random.Range(0, ParticleSorter.MAX_SCAN_SIZE);
                cpu[i] = gpu[i];
            }

            input.SetData(gpu);
            cs.GPUSortInplace(input, output);

            int[,] result = new int[N, 2];
            output.GetData(result);

            System.Array.Sort(cpu);

            for (int i = 0; i < N; i++)
            {
                Assert.That(result[i,1], Is.EqualTo(cpu[i]), "On element " + i + " after " + j + " iterations.");
            }
        }        
    }

}
