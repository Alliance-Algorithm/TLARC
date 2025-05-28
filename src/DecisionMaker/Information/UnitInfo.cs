using System.Diagnostics;
using ALPlanner.Interfaces.ROS;
using Maps;

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
  PoseTransform2D sentry;
  [ComponentReferenceFiled]
  IMap map;

  string carFoundTopicName = "/rmcs/auto_aim/car_found";
  IO.ROS2Msgs.Std.UInt8 carFoundConn;

  string carPositionTopicName = "/rmcs/auto_aim/car_positions";
  IO.ROS2Msgs.Nav.Path carPositionConn;
  string debugCarPositionDebugTopicName = "/tlarc/car_positions";
  IO.ROS2Msgs.Nav.Path debugCarPositionConn;
  string lockTopicName = "/rmcs/auto_aim/car_lock";
  IO.ROS2Msgs.Std.UInt8 carLockConn;
  string whitelistTopicName = "/tlarc/control/auto_aim/whitelist";
  IO.ROS2Msgs.Std.FloatMultiArray equalityHpPub;
  string equalityHpTopicName = "/tlarc/equality_hp";
  IO.ROS2Msgs.Std.UInt8 autoAimWhitelistConn;

  public class FoundHelper(byte data)
  {
    byte data_ = data;
    public bool this[int i]
    {
      get => ((data_ >> i) & 1) == 0;
      set => data_ = (byte)(value ? (data_ | (byte)(1 << i)) : (data_ & (byte)(~1 << i)));
    }
  }
  public FoundHelper Found { get; private set; } = new(0b11111111);
  public Vector3d[] Position { get; private set; } = new Vector3d[7];
  public int Locked { get; private set; } = -1;
  public ushort[] Hp { get; private set; } = [100, 100, 100, 100, 100, 100, 100, 100];
  public ushort[] _lastHp = [100, 100, 100, 100, 100, 100, 100, 100];
  public float[] EquivalentHp { get; private set; } = new float[7];
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

  private Whitelist whitelist;
  private float _sentryHp => info.SentryHp;

  public override void Start()
  {
    carFoundConn = new(IOManager);
    carFoundConn.Subscript(carFoundTopicName, msg => Found = new(msg));
    carPositionConn = new(IOManager);
    carPositionConn.Subscript(carPositionTopicName, msg =>
    {
      for (int i = 0; i < 5; i++)
      {
        Position[i] = new(msg[i].X, msg[i].Y, msg[i].Z);
        Position[i] = sentry.Position + Quaterniond.AxisAngleR(Vector3d.AxisZ, sentry.AngleR) * Position[i];
      }
    });
    carLockConn = new(IOManager);
    carLockConn.Subscript(lockTopicName, msg => Locked = msg);
    debugCarPositionConn = new(IOManager);
    debugCarPositionConn.RegistryPublisher(debugCarPositionDebugTopicName);
    autoAimWhitelistConn = new(IOManager);
    autoAimWhitelistConn.RegistryPublisher(whitelistTopicName);
    equalityHpPub = new(IOManager);
    equalityHpPub.RegistryPublisher(equalityHpTopicName);
  }
  public override void Update()
  {
    whitelist = 0;
    Hp = info.EnemiesHp;
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
        // if (!map.CheckAccessibility(Position[i], 1))
        //   EquivalentHp[i] = float.PositiveInfinity;
        if (EquivalentHp[i] > 5000)
          whitelist |= (Whitelist)(1 << i);
        // TlarcSystem.LogInfo($"{EquivalentHp[i]}");
      }
    } while (false);
    equalityHpPub.Publish(EquivalentHp);
    debugCarPositionConn.Publish(Position);
    autoAimWhitelistConn.Publish((byte)whitelist);
    _lastHp = Hp.Copy();
  }
}
