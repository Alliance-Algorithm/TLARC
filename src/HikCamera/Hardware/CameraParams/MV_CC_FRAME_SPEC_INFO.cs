// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.CameraParams.MV_CC_FRAME_SPEC_INFO
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

using System.Runtime.InteropServices;

namespace MvsSharp.CameraParams
{
  /// <summary>帧特殊信息</summary>
  public struct MV_CC_FRAME_SPEC_INFO
  {
    /// <summary>[OUT]     秒数</summary>
    public uint nSecondCount;
    /// <summary>[OUT]     周期数</summary>
    public uint nCycleCount;
    /// <summary>[OUT]     周期偏移量</summary>
    public uint nCycleOffset;
    /// <summary>[OUT]     增益</summary>
    public float fGain;
    /// <summary>[OUT]     曝光时间</summary>
    public float fExposureTime;
    /// <summary>[OUT]     平均亮度</summary>
    public uint nAverageBrightness;
    /// <summary>[OUT]     红色</summary>
    public uint nRed;
    /// <summary>[OUT]     绿色</summary>
    public uint nGreen;
    /// <summary>[OUT]     蓝色</summary>
    public uint nBlue;
    /// <summary>[OUT]     总帧数</summary>
    public uint nFrameCounter;
    /// <summary>[OUT]     触发计数</summary>
    public uint nTriggerIndex;
    /// <summary>[OUT]     输入</summary>
    public uint nInput;
    /// <summary>[OUT]     输出</summary>
    public uint nOutput;
    /// <summary>[OUT]     水平偏移量</summary>
    public ushort nOffsetX;
    /// <summary>[OUT]     垂直偏移量</summary>
    public ushort nOffsetY;
    /// <summary>[OUT]     水印宽</summary>
    public ushort nFrameWidth;
    /// <summary>[OUT]     水印高</summary>
    public ushort nFrameHeight;
    /// <summary>保留字节</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public uint[] nRes;
  }
}
