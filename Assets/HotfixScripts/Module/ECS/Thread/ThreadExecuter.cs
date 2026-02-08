using System;
using System.Threading;
using UnityEngine;

namespace Framework.ECS
{

    public class ThreadWrapper
    {
        public Thread thread;
        public ManualResetEvent work;
        public ManualResetEvent done;
        public int startIndex;
        public int endIndex;
    }
    public delegate void ThreadWorkFunction(int threadId, int startIndex, int endIndex);
    public static class ThreadExecuter
    {
        static ThreadWorkFunction workFunction;
        static ThreadWrapper[] threadPools;
        static int workThreadCount;

        static void Execute(object id)
        {
            while (true)
            {
                //所处数组的下标
                var threadIndex = (int)id;
                ref var thread = ref threadPools[threadIndex];
                thread.work.WaitOne();
                thread.work.Reset();
                workFunction.Invoke(threadIndex, thread.startIndex, thread.endIndex);

                thread.done.Set();
            }

        }
        static ThreadExecuter()
        {
            workFunction = null;
            workThreadCount = Environment.ProcessorCount * 2;
            threadPools = new ThreadWrapper[workThreadCount];
            for (int i = 0; i < workThreadCount; i++)
            {
                var workThread = new ThreadWrapper();

                threadPools[i] = workThread;
                workThread.startIndex = 0;
                workThread.endIndex = 0;
                workThread.work = new ManualResetEvent(false);
                workThread.done = new ManualResetEvent(true);
                workThread.thread = new Thread(Execute)
                {
                    IsBackground = true
                };

                workThread.thread.Start(i);
            }
        }

        public static void Run(ThreadWorkFunction workFunction, int count, int ChunkSize)
        {
            if (count < ChunkSize)
            {
                throw new Exception("count < ChunkSize");
            }
            //得到理论的线程数
            int jobCount = count / ChunkSize;
            if (jobCount > workThreadCount)
            {
                jobCount = workThreadCount;
            }
            int threadDataSize = count / jobCount;
            int startIndex = 0;
            ThreadExecuter.workFunction = workFunction;
            for (int i = 0; i < jobCount - 1; i++)
            {
                ref var workThread = ref threadPools[i];
                workThread.startIndex = startIndex;
                workThread.endIndex = startIndex + threadDataSize;
                workThread.done.Reset();
                workThread.work.Set();
                startIndex += threadDataSize;
            }
            //最后一个除不尽的（即使除尽了）的工作线程
            ref var endThread = ref threadPools[jobCount - 1];
            endThread.startIndex = startIndex;
            endThread.endIndex = count;
            endThread.done.Reset();
            endThread.work.Set();

            for (int i = 0; i < jobCount; i++)
            {
                ref var thread = ref threadPools[i];
                thread.done.WaitOne();
            }
            ThreadExecuter.workFunction = null;
        }
    }


}
