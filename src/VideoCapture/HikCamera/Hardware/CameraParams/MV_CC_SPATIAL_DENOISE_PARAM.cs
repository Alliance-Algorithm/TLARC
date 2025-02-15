// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.CameraParams.MV_CC_SPATIAL_DENOISE_PARAM
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

using System.Runtime.InteropServices;

namespace MvsSharp.CameraParams
{
  /// <summary>空域降噪参数</summary>
  public struct MV_CC_SPATIAL_DENOISE_PARAM
  {
    /// <summary>[IN]     图像宽度</summary>
    public uint nWidth;
    /// <summary>[IN]     图像高度</summary>
    public uint nHeight;
    /// <summary>[IN]     输入的像素格式</summary>
    public MvGvspPixelType enPixelType;
    /// <summary>[IN]     输入图像缓存</summary>
    public IntPtr pSrcBuf;
    /// <summary>[IN]     输入图像缓存长度</summary>
    public uint nSrcBufLen;
    /// <summary>[OUT]    输出降噪后的数据</summary>
    public IntPtr pDstBuf;
    /// <summary>[IN]     提供的输出缓冲区大小</summary>
    public uint nDstBufSize;
    /// <summary>[OUT]    输出降噪后的数据长度</summary>
    public uint nDstBufLen;
    /// <summary>[IN]     输入噪声特性</summary>
    public IntPtr pNoiseProfile;
    /// <summary>[IN]     输入噪声特性长度</summary>
    public uint nNoiseProfileLen;
    /// <summary>[IN]     降噪强度(0-100)</summary>
    public uint nBayerDenoiseStrength;
    /// <summary>[IN]     锐化强度(0-32)</summary>
    public uint nBayerSharpenStrength;
    /// <summary>[IN]     噪声校正系数(0-1280)</summary>
    public uint nBayerNoiseCorrect;
    /// <summary>[IN]     亮度校正系数(1-2000)</summary>
    public uint nNoiseCorrectLum;
    /// <summary>[IN]     色调校正系数(1-2000)</summary>
    public uint nNoiseCorrectChrom;
    /// <summary>[IN]     亮度降噪强度(0-100)</summary>
    public uint nStrengthLum;
    /// <summary>[IN]     色调降噪强度(0-100)</summary>
    public uint nStrengthChrom;
    /// <summary>[IN]     锐化强度(1-1000)</summary>
    public uint nStrengthSharpen;
    /// <summary>保留字节</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public uint[] nRes;
  }
}
