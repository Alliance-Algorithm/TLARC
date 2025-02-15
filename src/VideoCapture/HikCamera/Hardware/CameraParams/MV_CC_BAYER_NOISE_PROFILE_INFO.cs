// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.CameraParams.MV_CC_BAYER_NOISE_PROFILE_INFO
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

using System.Runtime.InteropServices;

namespace MvsSharp.CameraParams
{
  /// <summary>噪声基本信息</summary>
  public struct MV_CC_BAYER_NOISE_PROFILE_INFO
  {
    /// <summary>版本</summary>
    public uint nVersion;
    /// <summary>噪声特性类型</summary>
    public MV_CC_BAYER_NOISE_FEATURE_TYPE enNoiseFeatureType;
    /// <summary>图像格式</summary>
    public MvGvspPixelType enPixelType;
    /// <summary>平均噪声水平</summary>
    public int nNoiseLevel;
    /// <summary>曲线点数</summary>
    public uint nCurvePointNum;
    /// <summary>噪声曲线</summary>
    public IntPtr nNoiseCurve;
    /// <summary>亮度曲线</summary>
    public IntPtr nLumCurve;
    /// <summary>保留字节</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public uint[] nRes;
  }
}
