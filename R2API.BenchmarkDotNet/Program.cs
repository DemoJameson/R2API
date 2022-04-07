using System.Reflection;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using R2API.Utils;

namespace R2API.BenchmarkDotNet;

public class Program {
    public static void Main(string[] args) {
        BenchmarkRunner.Run<Person>();
    }
}

[MonoJob("Mono x64", @"C:\Program Files\Mono\bin\mono.exe")]
[SimpleJob(RuntimeMoniker.Net48)]
[SimpleJob(RuntimeMoniker.Net60)]
[SimpleJob(RuntimeMoniker.NetCoreApp31)]
[MemoryDiagnoser]
public class Person {
    private static readonly FieldInfo fieldInfo;
    private static readonly Reflection.SetDelegate<string> SetDelegate;

    static Person() {
        var person = new Person();
        fieldInfo = typeof(Person).GetField("name", BindingFlags.Instance | BindingFlags.NonPublic);
        SetDelegate = fieldInfo.GetFieldSetDelegate<string>();
        person.SetFieldValue("name", "John");
        person.SetFieldValue2("name", "John");
        person.SetFieldValue3("name", "John");
        person.SetFieldValue4("name", "John");
    }

    private string name;

    [Benchmark]
    public void DirectSet() {
        name = "John";
    }

    [Benchmark]
    public void ReflectionSet() {
        typeof(Person).GetField("name", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this, "John");
    }

    [Benchmark(Baseline = true)]
    public void ReflectionCachedSet() {
        fieldInfo.SetValue(this, "John");
    }

    [Benchmark(Description = "R2API.SetFieldValue")]
    public void R2APISetFieldValue() {
        this.SetFieldValue<string>("name", "John");
    }

    [Benchmark(Description = "R2API.SetFieldValueWithoutGetOrAdd")]
    public void R2APISetFieldValueWithoutGetOrAdd() {
        this.SetFieldValue2<string>("name", "John");
    }

    [Benchmark(Description = "R2API.SetFieldValueWithoutGetFieldCached")]
    public void R2APISetFieldValueWithoutGetFieldCached() {
        this.SetFieldValue3<string>("name", "John");
    }

    [Benchmark(Description = "R2API.SetFieldValueWithoutGetFieldCachedButConcurrent")]
    public void R2APISetFieldValueWithoutGetFieldCachedButConcurrent() {
        this.SetFieldValue4<string>("name", "John");
    }

    [Benchmark(Description = "R2API.CachedSetDelegate")]
    public void R2APICachedSetDelegate() {
         SetDelegate(this, "John");
    }
}
