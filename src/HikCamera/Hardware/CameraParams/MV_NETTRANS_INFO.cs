// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.CameraParams.MV_NETTRANS_INFO
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

namespace MvsSharp.CameraParams
{
  /// <summary>Net Trans Info</summary>
  public struct MV_NETTRANS_INFO
  {
    /// <summary>已接收数据大小  [统计StartGrabbing和StopGrabbing之间的数据量]</summary>
    public long nReviceDataSize;
    /// <summary>丢帧数量</summary>
    public int nThrowFrameCount;
    /// <summary>接收帧数</summary>
    public uint nNetRecvFrameCount;
    /// <summary>请求重发包数</summary>
    public long nRequestResendPacketCount;
    /// <summary>重发包数</summary>
    public long nResendPacketCount;
  }
}
