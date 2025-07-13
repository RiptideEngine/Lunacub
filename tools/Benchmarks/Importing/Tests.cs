using BenchmarkDotNet.Attributes;
using System.Numerics;

namespace Benchmarks.Importing;

public class Tests {
    [Benchmark]
    public Vector3 Add() {
        return Vector3.One + Vector3.One;
    }
}