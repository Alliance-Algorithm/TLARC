using System.Reflection;
using System.Runtime.Serialization.Json;
using AllianceDM.IO;
using Rcl.Logging;

namespace AllianceDM.Init
{

    [Serializable]
    struct CompFiles()
    {
        public string[] arg = [];
        public string type = "";
        public string assembly = "";
        public uint[] input_id = [];
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
                try
                {
                    var fs = File.OpenRead(i);
                    var dj = new DataContractJsonSerializer(typeof(CompFilesList));
                    var comp = dj.ReadObject(fs) ?? throw new Exception("Null Object");
                    var cs = (CompFilesList)comp;
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

                        dynamic d = t.Assembly.CreateInstance(t.FullName, false, BindingFlags.Default, null, [c.this_id, c.input_id, c.arg], null, null)
                         ?? throw new Exception("Could not create instance");
                        components.Add(c.this_id, d);
                    }
                }
                catch (Exception e)
                {
                    Ros2Def.node.Logger.LogFatal(e.Message + "\tAt:" + i + "\twhen:InitComponent");
                    Environment.Exit(-1);
                }

            }
        }
    }
}