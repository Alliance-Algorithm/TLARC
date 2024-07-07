
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;

namespace AllianceDM
{
    public interface IComponent
    {
        /// <summary>
        /// 起始调用
        /// </summary>
        public virtual void Start() { }
        /// <summary>
        /// 状态更新调用
        /// </summary>
        public virtual void Update() { }
        /// <summary>
        /// 信息传递
        /// </summary>
        public virtual void Echo() { }
    }

    public class Component : IComponent
    {
        uint _uuid;
        Dictionary<string, uint> _revUid;
        Dictionary<string, object> _args;

        public uint ID => _uuid;
        public Dictionary<string, uint> RecieveID => _revUid;
        public Dictionary<string, object> Args => _args;


        /// <summary>
        /// 系统调用
        /// </summary>
        public void Awake()
        {
            Debug.WriteLine(this.GetType().FullName + "\t uuid:" + _uuid.ToString());
            foreach (var p in this.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (p.FieldType.IsSubclassOf(typeof(Component)))
                    p.SetValue(this, typeof(DecisionMaker).GetMethod("GetComponentWithUID",
                     BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public).MakeGenericMethod(p.FieldType).Invoke(null, [_revUid[p.Name]]));
            }
            foreach (var p in this.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!Args.ContainsKey(p.Name))
                    continue;
                if (_args[p.Name].GetType().IsSubclassOf(typeof(JContainer)))
                    p.SetValue(this, _args[p.Name].GetType().GetMethod("ToObject",
                     BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, []).MakeGenericMethod(p.FieldType).Invoke(_args[p.Name], null));
                else
                    p.SetValue(this, _args[p.Name] is double ? (float)(double)_args[p.Name] : _args[p.Name]);

            }
        }

        // /// <summary>
        // /// 起始调用
        // /// </summary>
        // public void Start() { }
        // /// <summary>
        // /// 状态更新调用
        // /// </summary>
        // public void Update() { }
        // /// <summary>
        // /// 历史遗留
        // /// </summary>
        // public void Echo() ;
        // /// <summary>
        // /// 信息传递
        // /// </summary>
        public virtual void Echo(string topic, int frameRate) { }


        public void InitComponents(uint uuid, Dictionary<string, uint> revid, Dictionary<string, object> args)
        {
            _uuid = uuid;
            _revUid = revid;
            _args = args;
        }

        public virtual void Start() { }

        public virtual void Update() { }

        public virtual void Echo() { }
    }

    public class ComponentCell(Component component)
    {
        uint _dim = 0;
        uint _ealy = 0;
        bool _flag = false;

        Component _component = component;

        public Component Component => _component;

        public Dictionary<string, uint> RecieveID => _component.RecieveID;
        public uint Dim { get => _dim; set => _dim = value; }
        public uint Ealy { get => _ealy; set => _ealy = value; }
        public bool Flag { get => _flag; set => _flag = value; }
        List<ComponentCell> _forward = [];
        public List<ComponentCell> Forward { get => Forward = _forward; set => _forward = value; }
        public uint ID => _component.ID;

        public Action Start => _component.Start;

        public Action Update => _component.Update;
        public Action Awake => _component.Awake;


        public static implicit operator ComponentCell(Component component)
        {
            return new ComponentCell(component);
        }
        public void InitComponents(uint uuid, Dictionary<string, uint> revid, Dictionary<string, object> args)
        {
            Component.InitComponents(uuid, revid, args);
        }
    }
}