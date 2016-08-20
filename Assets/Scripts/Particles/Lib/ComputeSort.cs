using UnityEngine;
using System.Collections;
using System;

public class ComputeSort : MonoBehaviour {

    public ComputeShader SortShader;
    private ComputeBuffer scanInput;
    private ComputeBuffer scanOutput;
    private ComputeBuffer sortInput;
    private ComputeBuffer sortOutput;
    private int scanHandle;
    private int countHandle;
    private int finishHandle;
    private int zeroHandle;

    public const int SCAN_BLOCKSIZE = 128;
    public const int SORT_BLOCKSIZE = 128;
    public const int MAX_SCAN_SIZE = 2 * 128;



    // Make public for unit testing

    //NOTE: We do everything with fixed sizes, so no need to change buffers
    public void Awake () {
        scanHandle = SortShader.FindKernel("Scan");
        countHandle = SortShader.FindKernel("Count");
        zeroHandle = SortShader.FindKernel("Zero");
        finishHandle = SortShader.FindKernel("Finish");

        scanInput = new ComputeBuffer(MAX_SCAN_SIZE, ShaderConstants.INT_STRIDE);
        scanOutput = new ComputeBuffer(MAX_SCAN_SIZE, ShaderConstants.INT_STRIDE);


        SortShader.SetBuffer(zeroHandle, "scanInput", scanInput);
        SortShader.SetBuffer(scanHandle, "scanInput", scanInput);

        SortShader.SetBuffer(scanHandle, "scanOutput", scanOutput);
        SortShader.SetBuffer(countHandle, "scanInput", scanInput);
        SortShader.SetBuffer(finishHandle, "scanOutput", scanOutput);

    }

    private void setupInputBuffers(int size)
    {

        if (sortInput != null && size == sortInput.count)
            return;

        if (sortInput != null)
            sortInput.Release();
        if (sortOutput != null)
            sortOutput.Release();

        sortInput = new ComputeBuffer(size, ShaderConstants.INT_STRIDE);
        sortOutput = new ComputeBuffer(size, ShaderConstants.INT_STRIDE);


        SortShader.SetBuffer(countHandle, "sortInput", sortInput);
        SortShader.SetBuffer(finishHandle, "sortInput", sortInput);

        SortShader.SetBuffer(finishHandle, "sortOutput", sortOutput);
    }
	
    public void CPUScan(ref int[] array)
    {
        for (int i = 1; i < array.Length; i++)
            array[i] += array[i - 1];
    }

    public void CPUCount(int[] input, ref int[] output)
    {
        for (int i = 0; i < output.Length; i++)
            output[i] = 0;
       for (int i = 0; i < input.Length; i++)
            output[input[i]]++;

    }

    public void GPUCount(int[] input, ref int[] output)
    {
        //pad the buffer if needed
        extend(ref output, MAX_SCAN_SIZE);
        setupInputBuffers(input.Length);

        sortInput.SetData(input);
        SortShader.Dispatch(countHandle, 1, 1, 1);
        scanInput.GetData(output);
    }

    private static void extend<T>(ref T[] array, int max)
    {
        //pad the buffer if needed and check size
        if (array.Length < max)
        {
            T[] temp = array;
            array = new T[max];
            for (int i = 0; i < temp.Length; i++)
                array[i] = temp[i];
        }
        else if (array.Length > max)
        {
            new ArgumentException("Scan array larger than max");
        }
    }

    public void GPUScan(ref int[] array)
    {

        //pad the buffer if needed and check size
        extend(ref array, MAX_SCAN_SIZE);
        

        scanInput.SetData(array);
        SortShader.Dispatch(scanHandle, 1, 1, 1);
       
        scanOutput.GetData(array);

    }


    public void GPUSort(ref int[] array, bool checkData)
    {

        setupInputBuffers(array.Length);

        //make sure we don't exceed the max scan size
        if(checkData)
        {
            foreach (var a in array)
                if (a > MAX_SCAN_SIZE)
                    throw new ArgumentException("Values to large in input array");
        }

        sortInput.SetData(array);
        SortShader.Dispatch(zeroHandle, 1, 1, 1);
        SortShader.Dispatch(countHandle, 1, 1, 1);
        SortShader.Dispatch(scanHandle, 1, 1, 1);
        SortShader.Dispatch(finishHandle, 1, 1, 1);
        scanOutput.GetData(array);
    }
}
