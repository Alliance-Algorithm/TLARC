// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.CameraParams.MV_IMAGE_BASIC_INFO
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

using System.Runtime.InteropServices;

namespace MvsSharp.CameraParams
{
  /// <summary>图像的基本信息</summary>
  public struct MV_IMAGE_BASIC_INFO
  {
    /// <summary>宽度值</summary>
    public ushort nWidthValue;
    /// <summary>宽度最小值</summary>
    public ushort nWidthMin;
    /// <summary>宽度最大值</summary>
    public uint nWidthMax;
    /// <summary>Width Inc</summary>
    public uint nWidthInc;
    /// <summary>高度值</summary>
    public uint nHeightValue;
    /// <summary>高度最小值</summary>
    public uint nHeightMin;
    /// <summary>高度最大值</summary>
    public uint nHeightMax;
    /// <summary>Height Inc</summary>
    public uint nHeightInc;
    /// <summary>帧率</summary>
    public float fFrameRateValue;
    /// <summary>最小帧率</summary>
    public float fFrameRateMin;
    /// <summary>最大帧率</summary>
    public float fFrameRateMax;
    /// <summary>当前的像素格式</summary>
    public uint enPixelType;
    /// <summary>支持的像素格式种类</summary>
    public uint nSupportedPixelFmtNum;
    /// <summary>像素列表</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
    public uint[] enPixelList;
    /// <summary>保留字节</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public uint[] nReserved;
  }
}
