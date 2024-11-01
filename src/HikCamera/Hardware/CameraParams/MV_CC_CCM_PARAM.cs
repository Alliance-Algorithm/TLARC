// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.CameraParams.MV_CC_CCM_PARAM
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

using System.Runtime.InteropServices;

namespace MvsSharp.CameraParams
{
  /// <summary>CCM参数</summary>
  public struct MV_CC_CCM_PARAM
  {
    /// <summary>[IN]     是否启用CCM</summary>
    public bool bCCMEnable;
    /// <summary>[IN]     CCM矩阵(-8192~8192)</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
    public int[] nCCMat;
    /// <summary>保留字节</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public uint[] nRes;
  }
}
