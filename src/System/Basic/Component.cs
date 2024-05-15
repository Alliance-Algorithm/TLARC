
using System.Runtime.Serialization;

namespace AllianceDM
{
    public interface IComponent
    {
        /// <summary>
        /// 起始调用
        /// </summary>
        public virtual void Awake() { }
        /// <summary>
        /// 状态更新调用
        /// </summary>
        public virtual void Update() { }
        /// <summary>
        /// 信息传递
        /// </summary>
        public virtual void Echo() { }
    }

    public class Component(uint uuid, uint[] revid, string[] args) : IComponent
    {
        uint _uuid = uuid;
        uint[] _revUid = revid;
        string[] _args = args;

        public uint ID => _uuid;
        public uint[] RecieveID => _revUid;
        public string[] Args => _args;

        /// <summary>
        /// 起始调用
        /// </summary>
        public virtual void Awake() { }
        /// <summary>
        /// 状态更新调用
        /// </summary>
        public virtual void Update() { }
        /// <summary>
        /// 信息传递
        /// </summary>
        public virtual void Echo(string topic, int frameRate) { }
    }

    public class ComponentCell(Component component)
    {

        uint _dim = 0;
        uint _ealy = 0;

        Component _component = component;

        public Component Component => _component;

        public uint[] RecieveID => _component.RecieveID;
        public uint Dim { get => _dim; set => _dim = value; }
        public uint Ealy { get => _ealy; set => _ealy = value; }
        List<ComponentCell> _forward = [];
        public List<ComponentCell> Forward { get => Forward = _forward; set => _forward = value; }
        public uint ID => _component.ID;

        public Action Awake => _component.Awake;

        public Action Update => _component.Update;


        public static implicit operator ComponentCell(Component component)
        {
            return new ComponentCell(component);
        }
    }
}