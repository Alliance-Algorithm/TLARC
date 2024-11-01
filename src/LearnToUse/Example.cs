using System.Diagnostics;
using TlarcKernel;

namespace LearnToUse
{
    public class Example1 : Component
    {
        Example2 example;

        public override void Update()
        {
            Thread.Sleep(Random.Shared.Next(0, 100));
        }
    };
    public class Example2 : Component
    {
        public Vector3d vector3;


        public override void Update()
        {
            vector3.x += 0.01;
            TlarcSystem.LogInfo($"{vector3.x}");
        }
    };
    public class Example3 : Component
    {

        Example2 example;
        public override void Update()
        {
            TlarcSystem.LogInfo($"same process {example.vector3.x}");
        }
    };
    public class Example4 : Component
    {
        Example2 example;
        public override void Update()
        {
            TlarcSystem.LogInfo($"diff process {example.vector3.x}");
        }
    };
    public class Example5 : Component
    {
        public Example6 example;
    };
    public class Example6 : Component
    {
        public Example7 example;
        public override void Update()
        {
            Thread.Sleep(Random.Shared.Next(500, 1100));
        }

    };
    public class Example7 : Component
    {
        Example8 example;
    };
    public class Example8 : Component
    {
        public override void Update()
        {
            Thread.Sleep(Random.Shared.Next(500, 1100));
        }
    };
}