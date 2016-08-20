using UnityEngine;
using System.Collections;
using NUnit.Framework;

[TestFixture]
public class ComputeSortTest : MonoBehaviour {

    ComputeSort cs;

    [SetUp]
    public void Init()
    {
        cs = GameObject.Find("ComputeSort").GetComponent<ComputeSort>();
        cs.Awake();
    }
    [Test]
    public void GPUScan()
    {
        //make an array to test
        int[] array = new int[200];
        int[] expected = new int[200];
        for (int i = 0; i < 125; i++)
        {
            array[i] = i * i;
            expected[i] = array[i];
        }

        cs.CPUScan(ref expected);
        cs.GPUScan(ref array);

        for (int i = 0; i < 200; i++)
        {
            Assert.That(array[i], Is.EqualTo(expected[i]), "On element " + i);
        }
        
    }

    [Test]
    public void GPUCount()
    {
        //make an array to test
        uint N = 128;
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
        uint N = 128;
        int[] gpu = new int[N];
        int[] cpu = new int[N];
        
        for (int i = 0; i < N; i++)
        {
            gpu[i] = Random.Range(0, ComputeSort.MAX_SCAN_SIZE);
            cpu[i] = gpu[i];
        }

        System.Array.Sort(cpu);
        cs.GPUSort(ref gpu,true);

        for (int i = 0; i < N; i++)
        {
            Assert.That(gpu[i], Is.EqualTo(cpu[i]), "On element " + i);
        }
        
    }

    [Test]
    public void GPUInPlaceSort()
    {
        //TODO!!!
        fdsa = 3
    }

}
