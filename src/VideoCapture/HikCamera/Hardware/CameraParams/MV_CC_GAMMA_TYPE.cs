// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.MV_CC_GAMMA_TYPE
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

namespace MvsSharp
{
  /// <summary>Gamma类型</summary>
  public enum MV_CC_GAMMA_TYPE
  {
    /// <summary>不启用</summary>
    MV_CC_GAMMA_TYPE_NONE,
    /// <summary>GAMMA值</summary>
    MV_CC_GAMMA_TYPE_VALUE,
    /// <summary>
    /// GAMMA曲线，8位需要的长度：256*sizeof(unsigned char)
    /// 10位需要的长度：1024*sizeof(unsigned short)
    /// 12位需要的长度：4096*sizeof(unsigned short)
    /// 16位需要的长度：65536*sizeof(unsigned short)
    /// </summary>
    MV_CC_GAMMA_TYPE_USER_CURVE,
    /// <summary>线性RGB转非线性RGB</summary>
    MV_CC_GAMMA_TYPE_LRGB2SRGB,
    /// <summary>非线性RGB转线性RGB</summary>
    MV_CC_GAMMA_TYPE_SRGB2LRGB,
  }
}
