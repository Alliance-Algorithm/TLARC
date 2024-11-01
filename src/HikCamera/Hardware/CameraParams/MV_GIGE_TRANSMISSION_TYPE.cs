// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.MV_GIGE_TRANSMISSION_TYPE
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

namespace MvsSharp
{
  /// <summary>GigE传输类型</summary>
  public enum MV_GIGE_TRANSMISSION_TYPE
  {
    /// <summary>表示单播(默认)</summary>
    MV_GIGE_TRANSTYPE_UNICAST = 0,
    /// <summary>表示组播</summary>
    MV_GIGE_TRANSTYPE_MULTICAST = 1,
    /// <summary>表示局域网内广播，暂不支持</summary>
    MV_GIGE_TRANSTYPE_LIMITEDBROADCAST = 2,
    /// <summary>表示子网内广播，暂不支持</summary>
    MV_GIGE_TRANSTYPE_SUBNETBROADCAST = 3,
    /// <summary>表示从相机获取，暂不支持</summary>
    MV_GIGE_TRANSTYPE_CAMERADEFINED = 4,
    /// <summary>表示用户自定义应用端接收图像数据Port号</summary>
    MV_GIGE_TRANSTYPE_UNICAST_DEFINED_PORT = 5,
    /// <summary>表示设置了单播，但本实例不接收图像数据</summary>
    MV_GIGE_TRANSTYPE_UNICAST_WITHOUT_RECV = 65536, // 0x00010000
    /// <summary>表示组播模式，但本实例不接收图像数据</summary>
    MV_GIGE_TRANSTYPE_MULTICAST_WITHOUT_RECV = 65537, // 0x00010001
  }
}
