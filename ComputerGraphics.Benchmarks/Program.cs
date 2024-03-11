using BenchmarkDotNet.Running;
using ComputerGraphics.Benchmarks;

var summary = BenchmarkRunner.Run<CopyVsAssign>();