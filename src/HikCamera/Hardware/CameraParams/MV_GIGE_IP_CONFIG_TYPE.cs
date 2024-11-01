// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.MV_GIGE_IP_CONFIG_TYPE
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

namespace MvsSharp
{
  /// <summary>GigE设备IP配置方式</summary>
  public enum MV_GIGE_IP_CONFIG_TYPE
  {
    /// <summary>LLA</summary>
    MV_IP_CFG_LLA = 67108864, // 0x04000000
    /// <summary>Static</summary>
    MV_IP_CFG_STATIC = 83886080, // 0x05000000
    /// <summary>DHCP</summary>
    MV_IP_CFG_DHCP = 100663296, // 0x06000000
  }
}
