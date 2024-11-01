// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.CameraParams.MV_CC_NOISE_ESTIMATE_PARAM
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

using System.Runtime.InteropServices;

namespace MvsSharp.CameraParams
{
  /// <summary>噪声估计参数</summary>
  public struct MV_CC_NOISE_ESTIMATE_PARAM
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
    /// <summary>[IN]     图像ROI</summary>
    public IntPtr pstROIRect;
    /// <summary>[IN]     ROI个数</summary>
    public uint nROINum;
    /// <summary>[IN]     噪声阈值[0-4095]</summary>
    public uint nNoiseThreshold;
    /// <summary>[OUT]    输出噪声特性</summary>
    public IntPtr pNoiseProfile;
    /// <summary>[IN]     提供的输出缓冲区大小</summary>
    public uint nNoiseProfileSize;
    /// <summary>[OUT]    输出噪声特性长度</summary>
    public uint nNoiseProfileLen;
    /// <summary>保留字节</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public uint[] nRes;
  }
}
