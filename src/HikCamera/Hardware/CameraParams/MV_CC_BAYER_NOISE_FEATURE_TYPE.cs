// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.CameraParams.MV_CC_BAYER_NOISE_FEATURE_TYPE
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

namespace MvsSharp.CameraParams
{
  /// <summary>噪声特性类型</summary>
  public enum MV_CC_BAYER_NOISE_FEATURE_TYPE
  {
    /// <summary>无效</summary>
    MV_CC_BAYER_NOISE_FEATURE_TYPE_INVALID = 0,
    /// <summary>噪声曲线</summary>
    MV_CC_BAYER_NOISE_FEATURE_TYPE_PROFILE = 1,
    /// <summary>默认值</summary>
    MV_CC_BAYER_NOISE_FEATURE_TYPE_DEFAULT = 2,
    /// <summary>噪声水平</summary>
    MV_CC_BAYER_NOISE_FEATURE_TYPE_LEVEL = 2,
  }
}
