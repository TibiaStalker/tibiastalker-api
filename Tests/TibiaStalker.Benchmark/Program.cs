// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Running;
using TibiaStalker.Benchmark;

Console.WriteLine("Hello, World!");

BenchmarkRunner.Run<CreateCharacterCorrelationsIfNotExistBenchmark>();