// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.CameraParams.MV_SAVE_IMAGE_PARAM_EX2
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

using System.Runtime.InteropServices;

namespace MvsSharp.CameraParams
{
  /// <summary>保存的图像信息扩展</summary>
  public struct MV_SAVE_IMAGE_PARAM_EX2
  {
    /// <summary>[IN]     输入数据缓存</summary>
    public IntPtr pData;
    /// <summary>[IN]     输入数据大小</summary>
    public uint nDataLen;
    /// <summary>[IN]     输入数据的像素格式</summary>
    public MvGvspPixelType enPixelType;
    /// <summary>[IN]     图像宽</summary>
    public uint nWidth;
    /// <summary>[IN]     图像高</summary>
    public uint nHeight;
    /// <summary>[OUT]    输出图片缓存</summary>
    public IntPtr pImageBuffer;
    /// <summary>[OUT]    输出图片大小</summary>
    public uint nImageLen;
    /// <summary>[IN]     提供的输出缓冲区大小</summary>
    public uint nBufferSize;
    /// <summary>[IN]     输出图片格式</summary>
    public MV_SAVE_IAMGE_TYPE enImageType;
    /// <summary>[IN]     编码质量, (50-99]</summary>
    public uint nJpgQuality;
    /// <summary>[IN]     Bayer的插值方法 0-快速 1-均衡 2-最优（如果传入其它值则默认为最优）</summary>
    public uint iMethodValue;
    /// <summary>保留字节</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public uint[] nReserved;
  }
}
