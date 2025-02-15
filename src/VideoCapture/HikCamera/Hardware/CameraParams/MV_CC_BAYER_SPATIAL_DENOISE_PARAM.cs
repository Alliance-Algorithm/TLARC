// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.CameraParams.MV_CC_BAYER_SPATIAL_DENOISE_PARAM
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

using System.Runtime.InteropServices;

namespace MvsSharp.CameraParams
{
  /// <summary>降噪参数</summary>
  public struct MV_CC_BAYER_SPATIAL_DENOISE_PARAM
  {
    /// <summary>[IN]     图像宽(大于等于8)</summary>
    public uint nWidth;
    /// <summary>[IN]     图像高(大于等于8)</summary>
    public uint nHeight;
    /// <summary>[IN]     像素格式</summary>
    public MvGvspPixelType enPixelType;
    /// <summary>[IN]     输入数据缓存</summary>
    public IntPtr pSrcData;
    /// <summary>[IN]     输入数据大小</summary>
    public uint nSrcDataLen;
    /// <summary>[OUT]    输出降噪后的数据</summary>
    public IntPtr pDstBuf;
    /// <summary>[IN]     提供的输出缓冲区大小</summary>
    public uint nDstBufSize;
    /// <summary>[OUT]    输出降噪后的数据长度</summary>
    public uint nDstBufLen;
    /// <summary>[IN]    降噪特性信息(来源于噪声估计)</summary>
    public MV_CC_BAYER_NOISE_PROFILE_INFO stNoiseProfile;
    /// <summary>[IN]     降噪强度(0-100)</summary>
    public uint nDenoiseStrength;
    /// <summary>[IN]     锐化强度(0-32)</summary>
    public uint nSharpenStrength;
    /// <summary>[IN]     噪声校正系数(0-1280)</summary>
    public uint nNoiseCorrect;
    /// <summary>[IN]     线程数量，0表示算法库根据硬件自适应；1表示单线程（默认）；大于1表示线程数目</summary>
    public uint nThreadNum;
    /// <summary>保留字节</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public uint[] nRes;
  }
}
