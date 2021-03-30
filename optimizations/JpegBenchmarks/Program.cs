using System;
using BenchmarkDotNet.Running;

namespace JpegBenchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<JpegBenchmark>();
        }
    }
}