// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.CameraParams.MV_CC_ROTATE_IMAGE_PARAM
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

using System.Runtime.InteropServices;

namespace MvsSharp.CameraParams
{
  /// <summary>旋转图像参数</summary>
  public struct MV_CC_ROTATE_IMAGE_PARAM
  {
    /// <summary>[IN]     像素格式(仅支持Mono8/RGB24/BGR24)</summary>
    public MvGvspPixelType enPixelType;
    /// <summary>[IN][OUT]     图像宽</summary>
    public uint nWidth;
    /// <summary>[IN][OUT]     图像高</summary>
    public uint nHeight;
    /// <summary>[IN]     输入数据缓存</summary>
    public IntPtr pSrcData;
    /// <summary>[IN]     输入数据大小</summary>
    public uint nSrcDataLen;
    /// <summary>[OUT]    输出图片缓存</summary>
    public IntPtr pDstBuf;
    /// <summary>[OUT]    输出图片大小</summary>
    public uint nDstBufLen;
    /// <summary>[IN]     提供的输出缓冲区大小</summary>
    public uint nDstBufSize;
    /// <summary>[IN]     旋转角度</summary>
    public MV_IMG_ROTATION_ANGLE enRotationAngle;
    /// <summary>保留字节</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public uint[] nRes;
  }
}
