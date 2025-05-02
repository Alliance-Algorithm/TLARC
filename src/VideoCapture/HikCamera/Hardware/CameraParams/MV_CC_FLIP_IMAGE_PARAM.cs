// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.CameraParams.MV_CC_FLIP_IMAGE_PARAM
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

using System.Runtime.InteropServices;

namespace MvsSharp.CameraParams
{
  /// <summary>像素转换结构体</summary>
  [StructLayout(LayoutKind.Sequential, Pack = 8)]
  public struct MV_CC_PIXEL_CONVERT_PARAM
  {
    public ushort nWidth;

    ///< [IN]  \~chinese 图像宽                 \~english Width
    public ushort nHeight;

    ///< [IN]  \~chinese 图像高                 \~english Height
    public long enSrcPixelType;

    ///< [IN]  \~chinese 源像素格式             \~english Source pixel format
    public IntPtr pSrcData;

    ///< [IN]  \~chinese 输入数据缓存           \~english Input data buffer
    public uint nSrcDataLen;

    ///< [IN]  \~chinese 输入数据长度           \~english Input data length
    public long enDstPixelType;

    ///< [IN]  \~chinese 目标像素格式           \~english Destination pixel format
    public IntPtr pDstBuffer;

    ///< [OUT] \~chinese 输出数据缓存           \~english Output data buffer
    public uint nDstLen;

    ///< [OUT] \~chinese 输出数据长度           \~english Output data length
    public uint nDstBufferSize;

    ///< [IN]  \~chinese 提供的输出缓冲区大小   \~english Provided output buffer size
    /// <summary>保留字节</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
    public uint[] nRes;
  }
}
