namespace DecisionMaker.Information;

class EnemyUnitInfo : Component
{
    string positionTopicName = "/unit_info/enemy/position";
    string hpTopicName = "/unit_info/enemy/hp";
    DecisionMakingInfo info;
    public bool[] Found { get; private set; } = new bool[7];
    public Vector2d[] Position { get; private set; } = new Vector2d[7];
    public int Locked { get; private set; } = -1;
    public float[] Hp { get; private set; } = [100, 100, 100, 100, 100, 100, 100];
    public float[] _lastHp = [100, 100, 100, 100, 100, 100, 100];
    public float[] EquivalentHp { get; private set; } = new float[7];
    public bool AirSupport { get; private set; } = false;
    private long _airSupportTimeTick = DateTime.Now.Ticks;
    private long[] _responseTime = [DateTime.Now.Ticks, DateTime.Now.Ticks, DateTime.Now.Ticks, DateTime.Now.Ticks, DateTime.Now.Ticks, DateTime.Now.Ticks, DateTime.Now.Ticks];

    private float _lastSentryHp = 400;

    private float _sentryHp => info.SentryHp;

    private IO.ROS2Msgs.Std.FloatMultiArray _positionReceiver;
    private IO.ROS2Msgs.Std.FloatMultiArray _hpReceiver;

    public override void Start()
    {

        _positionReceiver = new(IOManager);
        _positionReceiver.Subscript(positionTopicName, msg =>
        {
            for (int i = 0; i < 7; i++)
            {
                if (msg[i] != -114514)
                {
                    Found[i] = true;
                    Position[i] = new(msg[i], msg[i + 7]);
                }
                else Found[i] = false;
            }
        });
        _hpReceiver = new(IOManager);
        _hpReceiver.Subscript(hpTopicName, msg => { if (msg != null) Hp = msg; });
    }
    private static bool CheckPosition(Vector2d position)
    {
        if (position.y > 0)
            return position.x * 1.5f - position.y + 7.5f < 0;
        else
            return position.x * 1.5f + position.y + 7.5f < 0;
    }
    public override void Update()
    {
        do
        {
            if ((DateTime.Now.Ticks - _airSupportTimeTick) / 1e7f > 30)
                AirSupport = false;
            if (_sentryHp == _lastSentryHp) break;
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

                EquivalentHp[i] = Hp[i];
                if (Hp[i] == 0 || (DateTime.Now.Ticks - _responseTime[i]) / 1e7f < 10)
                    EquivalentHp[i] = float.PositiveInfinity;

                if (EquivalentHp[i] > 1e5)
                    EquivalentHp[i] = float.PositiveInfinity;

            }
            if (Position[1].y > 10.5 && Position[1].y > 2.5)
                EquivalentHp[1] = float.PositiveInfinity;
        } while (false);

        _lastSentryHp = _sentryHp;
        Hp.CopyTo(_lastHp, 0);
    }
}