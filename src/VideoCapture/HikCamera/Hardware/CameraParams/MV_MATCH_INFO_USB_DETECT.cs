// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.CameraParams.MV_MATCH_INFO_USB_DETECT
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

using System.Runtime.InteropServices;

namespace MvsSharp.CameraParams
{
  /// <summary>USB</summary>
  public struct MV_MATCH_INFO_USB_DETECT
  {
    /// <summary>已接收数据大小    [统计OpenDevicce和CloseDevice之间的数据量]</summary>
    public long nReviceDataSize;
    /// <summary>已收到的帧数</summary>
    public uint nRevicedFrameCount;
    /// <summary>错误帧数</summary>
    public uint nErrorFrameCount;
    /// <summary>保留字节</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
    public uint[] nReserved;
  }
}
