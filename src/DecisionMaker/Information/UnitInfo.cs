namespace DecisionMaker.Information;

class EnemyUnitInfo : Component
{
  public const int Hero = 0;
  public const int Engineer = 1;
  public const int InfantryIII = 2;
  public const int InfantryIV = 3;
  public const int InfantryV = 4;
  public const int Sentry = 5;
  public const int Outpost = 6;
  public const int Base = 7;
  DecisionMakingInfo info;
  public bool[] Found { get; private set; } = new bool[7];
  public Vector2d[] Position { get; private set; } = new Vector2d[7];
  public int Locked { get; private set; } = -1;
  public float[] Hp { get; private set; } = [100, 100, 100, 100, 100, 100, 100, 100];
  public float[] _lastHp = [100, 100, 100, 100, 100, 100, 100, 100];
  public float[] EquivalentHp { get; private set; } = new float[7];
  public bool AirSupport { get; private set; } = false;
  private long _airSupportTimeTick = DateTime.Now.Ticks;
  private long[] _responseTime =
  [
    DateTime.Now.Ticks,
    DateTime.Now.Ticks,
    DateTime.Now.Ticks,
    DateTime.Now.Ticks,
    DateTime.Now.Ticks,
    DateTime.Now.Ticks,
    DateTime.Now.Ticks,
  ];

  private float _lastSentryHp = 400;

  private float _sentryHp => info.SentryHp;

  private IO.ROS2Msgs.Std.FloatMultiArray _positionReceiver;
  private IO.ROS2Msgs.Std.FloatMultiArray _hpReceiver;

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
      if (_sentryHp == _lastSentryHp)
        break;
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
    } while (false);

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
