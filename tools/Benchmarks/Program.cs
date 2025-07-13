using BenchmarkDotNet.Running;
using System.Reflection;
using Benchmarks.Importing;

// BenchmarkSwitcher.FromAssembly(Assembly.GetExecutingAssembly());
BenchmarkRunner.Run<SimpleResourceImporting>();