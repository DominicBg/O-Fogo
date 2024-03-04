using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Profiling;

public static class JobUtility
{
    public static int numberOfThread;
    public static Dictionary<Type, ProfilerMarker> cachedProfileMarker = new Dictionary<Type, ProfilerMarker>();

    [StructLayout(LayoutKind.Sequential, Size = 1)]
    internal struct JobStructParralelFor<T> where T : struct, IJobParallelFor
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
    [StructLayout(LayoutKind.Sequential, Size = 1)]
    internal struct JobStructRun<T> where T : struct, IJob
    {
        public delegate void ExecuteJobFunction(ref T data, IntPtr additionalPtr, IntPtr bufferRangePatchData, ref JobRanges ranges, int jobIndex);

        public static readonly IntPtr jobReflectionData = JobsUtility.CreateJobReflectionData(typeof(T), new ExecuteJobFunction(Execute));

        public static void Execute(ref T data, IntPtr additionalPtr, IntPtr bufferRangePatchData, ref JobRanges ranges, int jobIndex)
        {
            data.Execute();
        }
    }

    public unsafe static void RunParralel<T>(this T jobData, int arrayLength) where T : struct, IJobParallelFor
    {
        JobsUtility.JobScheduleParameters parameters = new JobsUtility.JobScheduleParameters(UnsafeUtility.AddressOf(ref jobData), JobStructParralelFor<T>.jobReflectionData, default(JobHandle), ScheduleMode.Parallel);
        JobsUtility.ScheduleParallelFor(ref parameters, arrayLength, arrayLength / numberOfThread).Complete();
    }

    public unsafe static void RunParralelAndProfile<T>(this T jobData, int arrayLength) where T : struct, IJobParallelFor
    {
        using (GetPooledMarker<T>().Auto())
        {
            RunParralel(jobData, arrayLength);
        }
    }

    public static ProfilerMarker GetPooledMarker<T>()
    {
        ProfilerMarker marker;
        if (!cachedProfileMarker.TryGetValue(typeof(T), out marker))
        {
            marker = new ProfilerMarker(typeof(T).Name);
            cachedProfileMarker.Add(typeof(T), marker);
        }

        return marker;
    }

    public unsafe static void RunAndProfile<T>(this T jobData) where T : struct, IJob
    {
        using (GetPooledMarker<T>().Auto())
        {
            JobsUtility.JobScheduleParameters parameters = new JobsUtility.JobScheduleParameters(UnsafeUtility.AddressOf(ref jobData), JobStructRun<T>.jobReflectionData, default(JobHandle), ScheduleMode.Run);
            JobsUtility.Schedule(ref parameters);
        }
    }
}
