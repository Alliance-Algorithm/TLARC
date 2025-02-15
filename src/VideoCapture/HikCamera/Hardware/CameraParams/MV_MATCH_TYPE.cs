// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.MV_MATCH_TYPE
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

namespace MvsSharp
{
  /// <summary>信息类型</summary>
  public enum MV_MATCH_TYPE
  {
    /// <summary>网络流量和丢包信息</summary>
    MV_MATCH_TYPE_NET_DETECT = 1,
    /// <summary>host接收到来自U3V设备的字节总数</summary>
    MV_MATCH_TYPE_USB_DETECT = 2,
  }
}
