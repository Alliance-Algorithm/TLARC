using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text.Json.Serialization;
using AllianceDM.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rcl.Logging;

namespace AllianceDM.Init
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
        public CompFiles[] list = [];
    }

    static internal class InitComponent
    {
        internal static void Init(ref Dictionary<uint, ComponentCell> components)
        {
            string path = DecisionMakerDef.ComponentsPath;
            components.Add(0, new IOManager());
            string[] files = Directory.GetFiles(path, "*.json");
            foreach (var i in files)
            {
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
                    components.Add(c.this_id, d);
                }

            }
            foreach (var i in components)
                i.Value.Awake();
        }
    }
}