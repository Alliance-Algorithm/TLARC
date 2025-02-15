// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.CameraParams.MV_PIXEL_CONVERT_PARAM
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

using System.Runtime.InteropServices;

namespace MvsSharp.CameraParams
{
  /// <summary>像素转换参数</summary>
  public struct MV_PIXEL_CONVERT_PARAM
  {
    /// <summary>[IN]     图像宽</summary>
    public ushort nWidth;
    /// <summary>[IN]     图像高</summary>
    public ushort nHeight;
    /// <summary>[IN]     源像素格式</summary>
    public MvGvspPixelType enSrcPixelType;
    /// <summary>[IN]     输入数据缓存</summary>
    public IntPtr pSrcData;
    /// <summary>[IN]     输入数据大小</summary>
    public uint nSrcDataLen;
    /// <summary>[IN]     目标像素格式</summary>
    public MvGvspPixelType enDstPixelType;
    /// <summary>[OUT]    输出数据缓存</summary>
    public IntPtr pDstBuffer;
    /// <summary>[OUT]    输出数据大小</summary>
    public uint nDstLen;
    /// <summary>[IN]     提供的输出缓冲区大小</summary>
    public uint nDstBufferSize;
    /// <summary>保留字节</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public uint[] nRes;
  }
}
