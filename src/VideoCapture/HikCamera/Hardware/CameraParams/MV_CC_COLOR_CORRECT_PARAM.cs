// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.CameraParams.MV_CC_COLOR_CORRECT_PARAM
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

using System.Runtime.InteropServices;

namespace MvsSharp.CameraParams
{
  /// <summary>色彩校正参数（包括CCM和CLUT）</summary>
  public struct MV_CC_COLOR_CORRECT_PARAM
  {
    /// <summary>[IN]     图像宽度</summary>
    public uint nWidth;
    /// <summary>[IN]     图像高度</summary>
    public uint nHeight;
    /// <summary>[IN]     输入图像缓存</summary>
    public IntPtr pSrcBuf;
    /// <summary>[IN]     输入图像缓存长度</summary>
    public uint nSrcBufLen;
    /// <summary>[IN]     输入的像素格式</summary>
    public MvGvspPixelType enPixelType;
    /// <summary>[OUT]    输出像素数据缓存</summary>
    public IntPtr pDstBuf;
    /// <summary>[IN]     提供的输出缓冲区大小</summary>
    public uint nDstBufSize;
    /// <summary>[OUT]    输出像素数据缓存长度</summary>
    public uint nDstBufLen;
    /// <summary>[IN]     输入有效图像位数，8 or 10 or 12 or 16</summary>
    public uint nImageBit;
    /// <summary>[IN]     输入Gamma信息</summary>
    public MV_CC_GAMMA_PARAM stGammaParam;
    /// <summary>[IN]     输入CCM信息</summary>
    public MV_CC_CCM_PARAM_EX stCCMParam;
    /// <summary>[IN]     输入CLUT信息</summary>
    public MV_CC_CLUT_PARAM stCLUTParam;
    /// <summary>保留字节</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public uint[] nRes;
  }
}
