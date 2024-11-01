// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.CameraParams.MV_SAVE_IMAGE_PARAM
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

namespace MvsSharp.CameraParams
{
  /// <summary>保存的图像参数</summary>
  public struct MV_SAVE_IMAGE_PARAM
  {
    /// <summary>[IN]     输入数据缓存</summary>
    public IntPtr pData;
    /// <summary>[IN]     输入数据大小</summary>
    public uint nDataLen;
    /// <summary>[IN]     输入数据的像素格式</summary>
    public MvGvspPixelType enPixelType;
    /// <summary>[IN]     图像宽</summary>
    public ushort nWidth;
    /// <summary>[IN]     图像高</summary>
    public ushort nHeight;
    /// <summary>[OUT]    输出图片缓存</summary>
    public IntPtr pImageBuffer;
    /// <summary>[OUT]    输出图片大小</summary>
    public uint nImageLen;
    /// <summary>[IN]     提供的输出缓冲区大小</summary>
    public uint nBufferSize;
    /// <summary>[IN]     输出图片格式</summary>
    public MV_SAVE_IAMGE_TYPE enImageType;
  }
}
