using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text.Json.Serialization;
using Tlarc.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rcl.Logging;

namespace Tlarc.Init
{

    [Serializable]
    struct CompFiles()
    {
        public Dictionary<string, object> arg = [];
        public string type = "";
        public string assembly = "";

        public Dictionary<string, uint> input_id = [];
        public uint this_id = 0;
    }
    [Serializable]
    struct CompFilesList()
    {
        public int fps = 0;
        public CompFiles[] list = [];
    }

    static internal class ProcessInit
    {
        internal static void Init(ref Dictionary<uint, Process> processes, ref Dictionary<uint, ComponentCell> componentCells)
        {
            uint pid = 1;
            string path = TlarcSystem.ConfigurationPath;
            string[] files = Directory.GetFiles(path, "*.json");
            foreach (var i in files)
            {
                Dictionary<uint, ComponentCell> components = new()
                {
                    { 0, new IOManager() }
                };
                // try
                // {
                var fs = File.ReadAllText(i);
                var cs = JsonConvert.DeserializeObject<CompFilesList>(fs);
                foreach (var c in cs.list)
                {
                    if (components.ContainsKey(c.this_id))
                        throw new Exception("Multi ID");
                    if (c.this_id == 0)
                        throw new Exception("Could not use ID:0");

                    Type? t = Type.GetType(c.assembly + '.' + c.type);
                    if (t == null || t.FullName == null)
                        throw new Exception("type error");
                    if (!t.IsSubclassOf(typeof(Component)))
                        throw new Exception("type not a component");

                    dynamic d = t.Assembly.CreateInstance(t.FullName, false, BindingFlags.Default, null, null, null, null)
                     ?? throw new Exception("Could not create instance");
                    (d as Component).InitComponents(c.this_id, c.input_id, c.arg);
                    (d as Component).IOManager = components[0].Component as IOManager;
                    (d as Component).ProcessID = pid;
                    components.Add(c.this_id, d);
                    componentCells.Add(c.this_id, d);
                }

                processes.Add(pid, new Process() { fps = cs.fps, Components = components });
                pid++;
            }
        }

        internal static void Init(ref object components)
        {
            throw new NotImplementedException();
        }
    }
}