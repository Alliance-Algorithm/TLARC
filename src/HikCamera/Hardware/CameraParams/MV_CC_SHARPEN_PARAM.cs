// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.CameraParams.MV_CC_SHARPEN_PARAM
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

using System.Runtime.InteropServices;

namespace MvsSharp.CameraParams
{
  /// <summary>锐化参数</summary>
  public struct MV_CC_SHARPEN_PARAM
  {
    /// <summary>[IN]     图像宽度(最小8)</summary>
    public uint nWidth;
    /// <summary>[IN]     图像高度(最小8)</summary>
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
    /// <summary>[IN]     锐度调节强度，范围:[0, 500]</summary>
    public uint nSharpenAmount;
    /// <summary>[IN]     锐度调节半径（半径越大，耗时越长），范围:[1, 21]</summary>
    public uint nSharpenRadius;
    /// <summary>[IN]     锐度调节阈值，范围:[0, 255]</summary>
    public uint nSharpenThreshold;
    /// <summary>保留字节</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public uint[] nRes;
  }
}
