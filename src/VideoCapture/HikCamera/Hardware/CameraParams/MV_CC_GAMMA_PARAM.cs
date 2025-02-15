// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.CameraParams.MV_CC_GAMMA_PARAM
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

using System.Runtime.InteropServices;

namespace MvsSharp.CameraParams
{
  /// <summary>Gamma参数</summary>
  public struct MV_CC_GAMMA_PARAM
  {
    /// <summary>[IN]     Gamma类型</summary>
    public MV_CC_GAMMA_TYPE enGammaType;
    /// <summary>[IN]     Gamma值</summary>
    public float fGammaValue;
    /// <summary>[IN]     Gamma曲线缓存</summary>
    public IntPtr pGammaCurveBuf;
    /// <summary>[IN]     Gamma曲线长度</summary>
    public uint nGammaCurveBufLen;
    /// <summary>保留字节</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public uint[] nRes;
  }
}
