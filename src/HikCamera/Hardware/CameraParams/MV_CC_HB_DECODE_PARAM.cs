// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.CameraParams.MV_CC_HB_DECODE_PARAM
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

using System.Runtime.InteropServices;

namespace MvsSharp.CameraParams
{
  /// <summary>HB解码参数</summary>
  public struct MV_CC_HB_DECODE_PARAM
  {
    /// <summary>[IN]     输入数据缓存</summary>
    public IntPtr pSrcBuf;
    /// <summary>[IN]     输入数据大小</summary>
    public uint nSrcLen;
    /// <summary>[OUT]    图像宽</summary>
    public uint nWidth;
    /// <summary>[OUT]    图像高</summary>
    public uint nHeight;
    /// <summary>[OUT]    输出数据缓存</summary>
    public IntPtr pDstBuf;
    /// <summary>[IN]     提供的输出缓冲区大小</summary>
    public uint nDstBufSize;
    /// <summary>[OUT]    输出数据大小</summary>
    public uint nDstBufLen;
    /// <summary>[OUT]     输出的像素格式</summary>
    public MvGvspPixelType enDstPixelType;
    /// <summary>[OUT]    水印信息</summary>
    public MV_CC_FRAME_SPEC_INFO stFrameSpecInfo;
    /// <summary>保留字节</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public uint[] nRes;
  }
}
