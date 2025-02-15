// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.CameraParams.MV_SAVE_IMG_TO_FILE_PARAM
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

using System.Runtime.InteropServices;

namespace MvsSharp.CameraParams
{
  /// <summary>保存图像到文件的参数</summary>
  public struct MV_SAVE_IMG_TO_FILE_PARAM
  {
    /// <summary>[IN]     输入数据的像素格式</summary>
    public MvGvspPixelType enPixelType;
    /// <summary>[IN]     输入数据缓存</summary>
    public IntPtr pData;
    /// <summary>[IN]     输入数据大小</summary>
    public uint nDataLen;
    /// <summary>[IN]     图像宽</summary>
    public ushort nWidth;
    /// <summary>[IN]     图像高</summary>
    public ushort nHeight;
    /// <summary>[IN]     输入图片格式</summary>
    public MV_SAVE_IAMGE_TYPE enImageType;
    /// <summary>[IN]     编码质量, (0-100]</summary>
    public uint nQuality;
    /// <summary>[IN]     输入文件路径</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
    public string pImagePath;
    /// <summary>[IN]     Bayer的插值方法 0-快速 1-均衡 2-最优（如果传入其它值则默认为最优）</summary>
    public uint iMethodValue;
    /// <summary>保留字节</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public uint[] nRes;
  }
}
