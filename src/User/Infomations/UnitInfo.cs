using System.Numerics;

namespace AllianceDM.PreInfo;

class EnemyUnitInfo : Component
{
    public string positionTopicName = "/unit_info/enemy/position";
    public string hpTopicName = "/unit_info/enemy/hp";
    public string foundedTopicName = "/unit_info/enemy/founded";

    public bool[] Found { get; private set; }
    public Vector2[] Position { get; private set; }
    public int Locked { get; private set; } = -1;
    public float[] Hp { get; private set; }
    public float[] _lastHp = new float[7];
    public float[] EquivalentHp { get; private set; } = new float[7];
    public bool AirSupport { get; private set; } = false;
    private long _airSupportTimeTick = DateTime.Now.Ticks;
    private long[] _responseTime = [DateTime.Now.Ticks, DateTime.Now.Ticks, DateTime.Now.Ticks, DateTime.Now.Ticks, DateTime.Now.Ticks, DateTime.Now.Ticks, DateTime.Now.Ticks];

    private float _lastSentryHp = 400;

    private float SentryHp { get; set; } = 400;


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
    private static bool CheckPosition(Vector2 position)
    {
        if (position.Y > 0)
            return position.X * 1.5f - position.Y + 7.5f < 0;
        else
            return position.X * 1.5f + position.Y + 7.5f < 0;
    }
    public override void Update()
    {
        do
        {
            if ((DateTime.Now.Ticks - _airSupportTimeTick) / 1e7f > 30)
                AirSupport = false;
            if (SentryHp == _lastSentryHp) break;
            if (AirSupport == true)
                break;
            int i = 0;
            for (i = 0; i < 7; i++)
            {
                if (!Found[i])
                    continue;
                if (!CheckPosition(Position[i]))
                    continue;
                break;
            }
            if (i == 7)
            {
                AirSupport = true;
                _airSupportTimeTick = DateTime.Now.Ticks;
            }
        }
        while (false);

        do
        {
            for (int i = 0; i < 7; i++)
            {
                if (_lastHp[i] == 0 && Hp[i] != 0)
                    _responseTime[i] = DateTime.Now.Ticks;

                if (Hp[i] == 0 || (DateTime.Now.Ticks - _responseTime[i]) / 1e7f < 10)
                    EquivalentHp[i] = float.PositiveInfinity;

            }
            if (Position[1].X > 11 && Position[1].Y > 3)
                EquivalentHp[1] = float.PositiveInfinity;
        } while (false);

        _lastSentryHp = SentryHp;
        Hp.CopyTo(_lastHp, 0);
    }
}