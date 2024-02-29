using System;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;

public static class JobUtility
{
    public static int numberOfThread;

    [StructLayout(LayoutKind.Sequential, Size = 1)]
    internal struct ParallelForJobStruct<T> where T : struct, IJobParallelFor
    {
        public delegate void ExecuteJobFunction(ref T data, IntPtr additionalPtr, IntPtr bufferRangePatchData, ref JobRanges ranges, int jobIndex);

        public static readonly IntPtr jobReflectionData = JobsUtility.CreateJobReflectionData(typeof(T), new ExecuteJobFunction(Execute));

        public unsafe static void Execute(ref T jobData, IntPtr additionalPtr, IntPtr bufferRangePatchData, ref JobRanges ranges, int jobIndex)
        {
            int beginIndex;
            int endIndex;
            while (JobsUtility.GetWorkStealingRange(ref ranges, jobIndex, out beginIndex, out endIndex))
            {
                JobsUtility.PatchBufferMinMaxRanges(bufferRangePatchData, UnsafeUtility.AddressOf(ref jobData), beginIndex, endIndex - beginIndex);
                int num = endIndex;
                for (int i = beginIndex; i < num; i++)
                {
                    jobData.Execute(i);
                }
            }
        }
    }


    public unsafe static void RunParralel<T>(this T jobData, int arrayLength) where T : struct, IJobParallelFor
    {
        JobsUtility.JobScheduleParameters parameters = new JobsUtility.JobScheduleParameters(UnsafeUtility.AddressOf(ref jobData), ParallelForJobStruct<T>.jobReflectionData, default(JobHandle), ScheduleMode.Parallel);
        JobsUtility.ScheduleParallelFor(ref parameters, arrayLength, arrayLength / numberOfThread).Complete();
    }
}
