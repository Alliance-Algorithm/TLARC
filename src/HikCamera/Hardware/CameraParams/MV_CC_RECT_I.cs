// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.CameraParams.MV_CC_RECT_I
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

namespace MvsSharp.CameraParams
{
  /// <summary>矩形ROI参数</summary>
  public struct MV_CC_RECT_I
  {
    /// <summary>[IN]     矩形左上角X轴坐标</summary>
    public uint nX;
    /// <summary>[IN]     矩形左上角Y轴坐标</summary>
    public uint nY;
    /// <summary>[IN]     矩形宽度</summary>
    public uint nWidth;
    /// <summary>[IN]     矩形高度</summary>
    public uint nHeight;
  }
}
