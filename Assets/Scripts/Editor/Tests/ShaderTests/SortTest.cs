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
        int[] input = new int[200];
        int[] gpu = new int[200];
        int[] cpu = new int[200];
        for (int i = 0; i < 200; i++)
        {
            input[i] = (int) Random.Range(0, 200);
        }

            
        cs.CPUCount(input, ref cpu);
        cs.GPUCount(input, ref gpu);

        for (int i = 0; i < 200; i++)
        {
            Assert.That(gpu[i], Is.EqualTo(cpu[i]), "On element " + i);
        }

    }

    [Test]
    public void GPUSort()
    {
        uint N = 10000;
        int[] input = new int[N];
        int[] gpu = new int[N];
        int[] cpu = new int[N];
        for (int i = 0; i < N; i++)
        {
            input[i] = Random.Range(0, ComputeSort.MAX_SCAN_SIZE);
            cpu[i] = input[i];
        }

        System.Array.Sort(cpu);

        for (int i = 0; i < N; i++)
        {
            Assert.That(gpu[i], Is.EqualTo(cpu[i]), "On element " + i);
        }
        
    }
}
