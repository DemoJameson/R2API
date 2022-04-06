using System.Diagnostics;
using System.Reflection;
using R2API.Utils;

namespace R2API.Benchmark {
    public class Benchmark {
        #region Sample Classes

        private class Person {
            internal static int counter;
            internal string name;

            internal Person() {
            }

            internal Person(int age, string name) {
                Age = age;
                this.name = name;
            }

            public object this[int a, int b] {
                get { return null; }
                set { }
            }

            internal int Age { get; set; }
            internal static int Counter { get; set; }

            internal void Walk() {
            }

            internal void Walk(int speed) {
            }

            internal static void Generate() {
            }

            internal static void Generate(int seed) {
            }

            internal string GetName() {
                return name;
            }

            internal string GetName(string prefix) {
                return prefix + " " + name;
            }
        }

        #endregion

        private static readonly int[] Iterations = new[] {20000, 200000, 2000000};
        private static readonly object[] NoArgArray = new object[0];
        private static readonly object[] ArgArray = new object[] {10};
        private static readonly Type TargetType = typeof(Person);
        private static readonly Person TargetPerson = new Person();
        private static readonly Person[] PeopleArray = new Person[100];
        private static readonly Stopwatch Watch = new Stopwatch();

        public static void Main(string[] args) {
            Prepare();
            Console.SetOut(new StreamWriter("benchmark.txt"));
            RunFieldBenchmark();
            Console.Out.Flush();
        }

        private static void RunFieldBenchmark() {
            FieldInfo fieldInfo = null;
            Reflection.GetDelegate<string> getDelegate = null;
            Reflection.SetDelegate<string> setDelegate = null;
            var initMap = new Dictionary<string, Action> {
                {
                    "Init info",
                    () => {
                        fieldInfo = TargetType.GetField("name",
                            BindingFlags.NonPublic | BindingFlags.Instance);
                        getDelegate = fieldInfo.GetFieldGetDelegate<string>();
                        setDelegate = fieldInfo.GetFieldSetDelegate<string>();
                    }
                },
            };

            dynamic tmp = TargetPerson;
            var actionMap = new Dictionary<string, Action> {
                {"Direct set", () => { TargetPerson.name = "John"; }}, {
                    "Direct get", () => {
                        string name = TargetPerson.name;
                    }
                },
                {"dynamic set", () => { tmp.name = "John"; }}, {
                    "dynamic get", () => {
                        string name = tmp.name;
                    }
                },

                {"Delegate get", () => TargetPerson.GetFieldValue<string>("name")},
                {"Delegate set", () => TargetPerson.SetFieldValue("name", "John")},

                {"Cached Delegate get", () => getDelegate(TargetPerson)},
                {"Cached Delegate set", () => setDelegate(TargetPerson, "John")}, {
                    "Reflection set", () => TargetType.GetField("name",
                        BindingFlags.NonPublic | BindingFlags.Instance).SetValue(TargetPerson, "John")
                }, {
                    "Reflection get", () => TargetType.GetField("name",
                        BindingFlags.NonPublic | BindingFlags.Instance).GetValue(TargetPerson)
                },

                {"Cached Reflection set", () => fieldInfo.SetValue(TargetPerson, "John")},
                {"Cached Reflection get", () => fieldInfo.GetValue(TargetPerson)},
            };
            Execute("Benchmark for Field Access", initMap, actionMap);
        }

        private static void Execute(string name, Dictionary<string, Action> initMap,
            Dictionary<string, Action> actionMap) {
            Console.WriteLine("!!!! {0}", name);

            Console.WriteLine("||Initialization|| ||");
            Measure(Watch, initMap, 1);
            Console.WriteLine();

            foreach (int iterationCount in Iterations) {
                Console.WriteLine("||Executing for {0} iterations|| ||", iterationCount);
                Measure(Watch, actionMap, iterationCount);
                Console.WriteLine();
            }

            Console.WriteLine();
        }

        private static void Measure(Stopwatch watch, Dictionary<string, Action> actionMap,
            int iterationCount) {
            foreach (var entry in actionMap) {
                watch.Start();
                for (int i = 0; i < iterationCount; i++) {
                    entry.Value();
                }

                watch.Stop();
                Console.WriteLine("|{0,-35} | {1,6} ms|", entry.Key + ":", watch.ElapsedMilliseconds);
                watch.Reset();
            }
        }

        private static void Prepare() {
            CollectGarbage();
            IncreaseThreadAndProcessPriority();
        }

        private static void CollectGarbage() {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForFullGCComplete();
        }

        private static void IncreaseThreadAndProcessPriority() {
            Thread.CurrentThread.Priority = ThreadPriority.Highest;
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;
            Process.GetCurrentProcess().ProcessorAffinity = new IntPtr(1);
        }
    }
}
