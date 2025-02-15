// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.CameraParams.MV_CC_STREAM_EXCEPTION_TYPE
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

namespace MvsSharp.CameraParams
{
  /// <summary>U3V流异常类型</summary>
  public enum MV_CC_STREAM_EXCEPTION_TYPE
  {
    /// <summary>异常的图像，该帧被丢弃</summary>
    MV_CC_STREAM_EXCEPTION_ABNORMAL_IMAGE = 16385, // 0x00004001
    /// <summary>缓存列表溢出，清除最旧的一帧</summary>
    MV_CC_STREAM_EXCEPTION_LIST_OVERFLOW = 16386, // 0x00004002
    /// <summary>缓存列表为空，该帧被丢弃</summary>
    MV_CC_STREAM_EXCEPTION_LIST_EMPTY = 16387, // 0x00004003
    /// <summary>断流恢复</summary>
    MV_CC_STREAM_EXCEPTION_RECONNECTION = 16388, // 0x00004004
    /// <summary>断流,恢复失败,取流被中止</summary>
    MV_CC_STREAM_EXCEPTION_DISCONNECTED = 16389, // 0x00004005
    /// <summary>设备异常,取流被中止</summary>
    MV_CC_STREAM_EXCEPTION_DEVICE = 16390, // 0x00004006
  }
}
