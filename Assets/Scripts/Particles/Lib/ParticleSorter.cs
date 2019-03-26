using UnityEngine;
using System.Collections;
using System;

namespace Rochester.ARTable.Particles
{

    /*
     *  Sorts ints. The output is an int2, where the first value corresponds to the index of the input array and the second is 
     *  the value of the input array.
     */
    public class ParticleSorter
    {

        private ComputeShader SortShader;
        private ComputeBuffer scanInput;
        private ComputeBuffer scanOutput;
        private ComputeBuffer sortInput;
        private ComputeBuffer sortOutput;
        private bool OwnBuffer; //Used to keep track of if the compute buffer may be used in another shader

        private int scanHandle;
        private int countHandle;
        private int finishHandle;
        private int zeroHandle;

        public const int SCAN_BLOCKSIZE = 256;
        public const int SORT_BLOCKSIZE = 128;
        public const int MAX_SCAN_SIZE = 2 * SCAN_BLOCKSIZE;



        // Make public for unit testing

        //NOTE: We do everything with fixed sizes, so no need to change buffers
        public ParticleSorter()
        {

            SortShader =  (ComputeShader) Resources.Load("ComputeShaders/Sort");
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

        public void ReleaseBuffers()
        {

            scanInput.Release();
            scanOutput.Release();
        }

        private void setupSortBuffers(int[] data)
        {
            int size = data.Length;
            if (size % SORT_BLOCKSIZE != 0)
                throw new ArgumentException("Data length must be multiple of SORT_BLOCKSIZE");
            if (sortInput == null || !OwnBuffer || size != sortInput.count)
            {
                if (sortInput != null)
                    sortInput.Release();
                if (sortOutput != null)
                    sortOutput.Release();

                sortInput = new ComputeBuffer(size, ShaderConstants.INT_STRIDE);
                sortOutput = new ComputeBuffer(size, 2 * ShaderConstants.INT_STRIDE);
                OwnBuffer = true;
            }

            sortInput.SetData(data);

            SortShader.SetBuffer(countHandle, "sortInput", sortInput);
            SortShader.SetBuffer(finishHandle, "sortInput", sortInput);
            SortShader.SetBuffer(finishHandle, "sortOutput", sortOutput);


        }

        public void CPUScan(ref int[] array)
        {
            int[] input = array;
            int[] output = new int[array.Length];
            output[0] = 0;
            for (int i = 1; i < array.Length; i++)
                output[i] = output[i - 1] + input[i - 1];
            array = output;
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
            setupSortBuffers(input);

            SortShader.Dispatch(zeroHandle, 1, 1, 1);
            SortShader.Dispatch(countHandle, Mathf.CeilToInt((float)input.Length / SORT_BLOCKSIZE), 1, 1);
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


        public void GPUSort(ref int[] array, bool checkData = false)
        {
            //make sure we don't exceed the max scan size
            if (checkData)
            {
                foreach (var a in array)
                    if (a >= MAX_SCAN_SIZE)
                        throw new ArgumentException("Values too large in input array");
            }
            setupSortBuffers(array);
            SortShader.Dispatch(zeroHandle, 1, 1, 1);
            SortShader.Dispatch(countHandle, Mathf.CeilToInt((float)array.Length / SORT_BLOCKSIZE), 1, 1);
            SortShader.Dispatch(scanHandle, 1, 1, 1);
            SortShader.Dispatch(finishHandle, Mathf.CeilToInt((float)array.Length / SORT_BLOCKSIZE), 1, 1);

            int[,] output = new int[array.Length, 2];
            sortOutput.GetData(output);
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = output[i, 1]; //get sorted value. x contains sorted index
            }
        }

        public void GPUSortInplace(ComputeBuffer input, ComputeBuffer output)
        {

            int size = input.count;
            //check if we have to set the buffers. 
            if (input != sortInput || output != sortOutput)
            {
                //have to shuffle buffers
                if (size % SORT_BLOCKSIZE != 0)
                    throw new ArgumentException("Data length must be multiple of SORT_BLOCKSIZE");

                if (sortInput != null)
                    sortInput.Release();
                if (sortOutput != null)
                    sortOutput.Release();

                sortInput = input;
                sortOutput = output;

                SortShader.SetBuffer(countHandle, "sortInput", sortInput);
                SortShader.SetBuffer(finishHandle, "sortInput", sortInput);
                SortShader.SetBuffer(finishHandle, "sortOutput", sortOutput);
                OwnBuffer = false;

            }

            SortShader.Dispatch(zeroHandle, 1, 1, 1);
            SortShader.Dispatch(countHandle, Mathf.CeilToInt((float)size / SORT_BLOCKSIZE), 1, 1);
            SortShader.Dispatch(scanHandle, 1, 1, 1);
            SortShader.Dispatch(finishHandle, Mathf.CeilToInt((float)size / SORT_BLOCKSIZE), 1, 1);
        }
    }
}