// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.CameraParams.MV_EVENT_OUT_INFO
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

using System.Runtime.InteropServices;

namespace MvsSharp.CameraParams
{
  /// <summary>事件信息</summary>
  public struct MV_EVENT_OUT_INFO
  {
    /// <summary>事件名</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public string EventName;
    /// <summary>Event号</summary>
    public ushort nEventID;
    /// <summary>流通到序号</summary>
    public ushort nStreamChannel;
    /// <summary>帧号高位</summary>
    public uint nBlockIdHigh;
    /// <summary>帧号低位</summary>
    public uint nBlockIdLow;
    /// <summary>时间戳高位</summary>
    public uint nTimestampHigh;
    /// <summary>时间戳低位</summary>
    public uint nTimestampLow;
    /// <summary>Event数据</summary>
    public IntPtr pEventData;
    /// <summary>Event数据长度</summary>
    public uint nEventDataSize;
    /// <summary>保留字节</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public uint[] nReserved;
  }
}
