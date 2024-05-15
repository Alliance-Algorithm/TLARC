using System.Runtime.Serialization.Json;
using Rcl.Logging;

namespace AllianceDM.Init
{
    static internal class InitGameObject
    {
        internal static void Init(ref Dictionary<string, GameObject> gameobjects)
        {
            string path = DecisionMakerDef.GameObjectsPath;

            string[] files = Directory.GetFiles(path, "*.json");

            foreach (var i in files)
            {
                try
                {
                    var fs = File.OpenRead(i);
                    var dj = new DataContractJsonSerializer(typeof(GameObject));
                    var go = dj.ReadObject(fs) ?? throw new Exception("Null Object");

                    var g = (GameObject)go;

                    if (string.IsNullOrEmpty(g.Name))
                        throw new Exception("Name Could Not Be Empty");

                    g.Log();
                    gameobjects.Add(g.Name, g);
                }
                catch (Exception e)
                {
                    Ros2Def.node.Logger.LogFatal(e.Message + "\tAt:" + i + "\twhen:InitGameobject");
                    Environment.Exit(-1);
                }
            }
        }
    }
}