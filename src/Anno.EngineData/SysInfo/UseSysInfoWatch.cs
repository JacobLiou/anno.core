﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Anno.EngineData.SysInfo
{
    /// <summary>
    /// 使用系统资源信息
    /// </summary>
    public class UseSysInfoWatch
    {
        MemoryMetricsClient memoryMetricsClient = new MemoryMetricsClient();
        private int mProcessorCount;
        public UseSysInfoWatch()
        {
            mProcessorCount = Environment.ProcessorCount;
            _mLastTime = RunTimeWatch.GetRunTimeMilliseconds();
            _mCpuMaxTime = mProcessorCount * 1000;
            mProcess = Process.GetCurrentProcess();
            _mLastTotalProcessorTime = mProcess.TotalProcessorTime.Milliseconds;
        }

        private readonly System.Diagnostics.Process mProcess;

        private readonly long _mCpuMaxTime;


        private long _mLastTime;

        private double _mLastTotalProcessorTime;

        private ServerStatus _mInfo = new ServerStatus();

        private long _mLastGetTime;

        private int _mGetStatus = 0;
        /// <summary>
        /// 获取服务状态
        /// </summary>
        /// <returns></returns>

        public ServerStatus GetServerStatus()
        {
            if (RunTimeWatch.GetRunTimeMilliseconds() - _mLastGetTime > 1000)
            {
                if (System.Threading.Interlocked.CompareExchange(ref _mGetStatus, 1, 0) == 0)
                {
                    _mLastGetTime = RunTimeWatch.GetRunTimeMilliseconds();
                    ServerStatus result = new ServerStatus();
                    TimeSpan ts = (DateTime.Now - RunTimeWatch.StartTime);
                    result.RunTime = $"{(long)ts.Days}:{(long)ts.Hours}:{(long)ts.Minutes}:{(long)ts.Seconds}";

                    long time = RunTimeWatch.GetRunTimeMilliseconds();
                    double second = (double)(time - _mLastTime) / 1000d;
                    _mLastTime = time;
                    double cpuTime = mProcess.TotalProcessorTime.TotalMilliseconds;
                    long cpuFullTime = (long)(second * _mCpuMaxTime);
                    double useTime = cpuTime - _mLastTotalProcessorTime;
                    _mLastTotalProcessorTime = cpuTime;
                    result.Cpu = (int)((useTime / cpuFullTime) * 10000) / 100d;

                    if (result.Cpu > 100)
                        result.Cpu = 100;
                    if (result.Cpu < 0)
                        result.Cpu = 0;
                    result.Memory = (Environment.WorkingSet / 1024) / 1024;
                    _mInfo = result;
                    System.Threading.Interlocked.Exchange(ref _mGetStatus, 0);
                }
            }
            var metrics = memoryMetricsClient.GetMetrics();
            _mInfo.MemoryTotal = metrics.Total;
            _mInfo.MemoryTotalUse = metrics.Used;
            _mInfo.CpuTotalUse = GetCpuTotalUse();
            _mInfo.Drives = AnnoDrives.GetDrivesInfo();
            if (_mInfo.Cpu > _mInfo.CpuTotalUse)
                _mInfo.CpuTotalUse = _mInfo.Cpu;
            return _mInfo;
        }

        private double GetCpuTotalUse()
        {
            var processes = Process.GetProcesses().Where(p => p.ProcessName != "Idle");
            int cpuFullTime = 200;

            double cpuTimeFirst = 0;
            double cpuTimeSecond = 0;
            foreach (var process in processes)
            {
                try
                {
                    cpuTimeFirst += process.TotalProcessorTime.TotalMilliseconds;
                }
                catch
                {
                    //
                }

            }
            System.Threading.Thread.Sleep(cpuFullTime);
            foreach (var process in processes)
            {
                try
                {
                    cpuTimeSecond += process.TotalProcessorTime.TotalMilliseconds;
                }
                catch
                {
                    //
                }
            }
            double useTime = cpuTimeSecond - cpuTimeFirst;
            var cpuUse = (useTime / (cpuFullTime * mProcessorCount)) * 100d;
            if (cpuUse > 100)
                cpuUse = 100;
            if (cpuUse < 0)
                cpuUse = 0;
            return cpuUse;
        }
        public class ServerStatus
        {
            public ServerStatus()
            {
            }
            public string RunTime { get; set; }
            public DateTime CurrentTime { get; set; } = DateTime.Now;
            public long Memory { get; set; }
            public double Cpu { get; set; }
            public string Tag { get; set; }

            public double MemoryTotal { get; set; }
            public double MemoryTotalUse { get; set; }
            public double CpuTotalUse { get; set; }

            public List<AnnoDrive> Drives { get; set; }

        }
    }
}
