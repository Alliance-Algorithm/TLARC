using System.Drawing;

namespace AutoExchange.ExchangeStationDetector;

enum LLightBarType
{
    ForwardLong,
    ForwardShort,
    Beside
}
struct LLightBar
{
    Point center;
    Vector2d forward;
    LLightBarType type;
}


class LLightBarWithKF
{

}