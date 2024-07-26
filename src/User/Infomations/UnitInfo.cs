using System.Numerics;

namespace AllianceDM.PreInfo;

class EnemyUnitInfo : Component
{
    public string positionTopicName = "/unit_info/enemy/position";
    public string hpTopicName = "/unit_info/enemy/hp";
    public string foundedTopicName = "/unit_info/enemy/founded";

    public bool[] Found { get; private set; } = new bool[7];
    public Vector2[] Position { get; private set; } = new Vector2[7];
    public int Locked { get; private set; } = -1;
    public float[] Hp { get; private set; } = new float[7];
    public float[] EquivalentHp { get; private set; } = new float[7];
    public bool AirSupport { get; private set; } = false;

    private IO.ROS2Msgs.Std.Int8 _foundedReceiver;
    private IO.ROS2Msgs.Std.FloatMultiArray _positionReceiver;
    private IO.ROS2Msgs.Std.FloatMultiArray _hpReceiver;

    public override void Start()
    {
        _foundedReceiver = new();
        _foundedReceiver.Subscript(foundedTopicName, msg =>
        {
            for (int i = 0; i < 7; i++)
                Found[i] = ((msg >> i) & 0x1) == 1;
        });
        _positionReceiver = new();
        _positionReceiver.Subscript(positionTopicName, msg =>
        {
            for (int i = 0; i < 7; i++)
                Position[i] = new(msg[i], msg[i + 7]);
        });
        _hpReceiver = new();
        _hpReceiver.Subscript(hpTopicName, msg => Hp = msg);
    }
}