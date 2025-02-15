// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.CameraParams.MV_MATCH_INFO_NET_DETECT
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

namespace MvsSharp.CameraParams
{
  /// <summary>
  /// 
  /// </summary>
  public struct MV_MATCH_INFO_NET_DETECT
  {
    /// <summary>已接收数据大小  [统计StartGrabbing和StopGrabbing之间的数据量]</summary>
    public long nReviceDataSize;
    /// <summary>丢失的包数量</summary>
    public long nLostPacketCount;
    /// <summary>丢帧数量</summary>
    public uint nLostFrameCount;
    /// <summary>帧数</summary>
    public uint nNetRecvFrameCount;
    /// <summary>请求重发包数</summary>
    public long nRequestResendPacketCount;
    /// <summary>重发包数</summary>
    public long nResendPacketCount;
  }
}
