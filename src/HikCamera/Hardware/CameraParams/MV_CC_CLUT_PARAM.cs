// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.CameraParams.MV_CC_CLUT_PARAM
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

using System.Runtime.InteropServices;

namespace MvsSharp.CameraParams
{
  /// <summary>CLUT参数</summary>
  public struct MV_CC_CLUT_PARAM
  {
    /// <summary>[IN]     是否启用CLUT</summary>
    public bool bCLUTEnable;
    /// <summary>[IN]     量化系数(2的整数幂)</summary>
    public uint nCLUTScale;
    /// <summary>[IN]     CLUT大小，建议值17</summary>
    public uint nCLUTSize;
    /// <summary>[OUT]    量化CLUT</summary>
    public IntPtr pCLUTBuf;
    /// <summary>
    /// [IN]     量化CLUT缓存大小（nCLUTSize*nCLUTSize*nCLUTSize*sizeof(int)*3）
    /// </summary>
    public uint nCLUTBufLen;
    /// <summary>保留字节</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public uint[] nRes;
  }
}
