// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.CameraParams.MV_FRAME_OUT_INFO
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

using System.Runtime.InteropServices;

namespace MvsSharp.CameraParams
{
  /// <summary>Frame Out Info</summary>
  public struct MV_FRAME_OUT_INFO
  {
    /// <summary>图像宽</summary>
    public ushort nWidth;
    /// <summary>图像高</summary>
    public ushort nHeight;
    /// <summary>像素格式</summary>
    public MvGvspPixelType enPixelType;
    /// <summary>帧号</summary>
    public uint nFrameNum;
    /// <summary>时间戳高32位</summary>
    public uint nDevTimeStampHigh;
    /// <summary>时间戳低32位</summary>
    public uint nDevTimeStampLow;
    /// <summary>保留，8字节对齐</summary>
    public uint nReserved0;
    /// <summary>主机生成的时间戳</summary>
    public long nHostTimeStamp;
    /// <summary>帧数据大小</summary>
    public uint nFrameLen;
    /// <summary>丢包数量</summary>
    public uint nLostPacket;
    /// <summary>保留字节</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
    public uint[] nReserved;
  }
}
