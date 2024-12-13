using System.Drawing;

namespace AutoExchange.ExchangeStationDetector;

public enum LampType
{
    L1,
    L2,
    L3
}
public struct LampInfo
{
    public Vector2d center;
    public Vector2d dir1;
    public Vector2d dir2;
    public LampType Type;      // 灯条种类 (L1, L2, L3)
    public Rectangle ROI;    // ROI 矩形
}
