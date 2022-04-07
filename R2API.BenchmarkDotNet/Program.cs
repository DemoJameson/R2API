using System.Reflection;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using R2API.Utils;

namespace R2API.BenchmarkDotNet;

public class Program {
    public static void Main(string[] args) {
        BenchmarkRunner.Run<Person>();
    }
}

[MonoJob("Mono x64", @"C:\Program Files\Mono\bin\mono.exe")]
[MemoryDiagnoser]
public class Person {
    private static readonly FieldInfo fieldInfo;
    private static readonly Reflection.SetDelegate<string> SetDelegate;

    static Person() {
        var person = new Person();
        fieldInfo = typeof(Person).GetField("name", BindingFlags.Instance | BindingFlags.NonPublic);
        SetDelegate = fieldInfo.GetFieldSetDelegate<string>();
        person.R2APISetFieldValue();
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

    [Benchmark]
    public void ReflectionCachedSet() {
        fieldInfo.SetValue(this, "John");
    }

    [Benchmark]
    public void R2APISetFieldValue() {
        this.SetFieldValue<string>("name", "John");
    }

    [Benchmark]
    public void R2APISetFieldValueWithoutGetOrAddExtension() {
        this.SetFieldValue2<string>("name", "John");
    }

    [Benchmark]
    public void R2APICachedSetDelegate() {
         SetDelegate(this, "John");
    }
}
