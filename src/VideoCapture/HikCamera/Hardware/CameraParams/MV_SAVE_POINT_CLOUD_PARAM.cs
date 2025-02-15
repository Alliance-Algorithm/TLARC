// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.CameraParams.MV_SAVE_POINT_CLOUD_PARAM
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

using System.Runtime.InteropServices;

namespace MvsSharp.CameraParams
{
  /// <summary>保存的点阵参数</summary>
  public struct MV_SAVE_POINT_CLOUD_PARAM
  {
    /// <summary>[IN]     每一行点的数量</summary>
    public uint nLinePntNum;
    /// <summary>[IN]     行数</summary>
    public uint nLineNum;
    /// <summary>[IN]     输入数据的像素格式</summary>
    public MvGvspPixelType enSrcPixelType;
    /// <summary>[IN]     输入数据缓存</summary>
    public IntPtr pSrcData;
    /// <summary>[IN]     输入数据大小</summary>
    public uint nSrcDataLen;
    /// <summary>[OUT]    输出像素数据缓存</summary>
    public IntPtr pDstBuf;
    /// <summary>
    /// [IN]     提供的输出缓冲区大小(nLinePntNum * nLineNum * (16*3 + 4) + 2048)
    /// </summary>
    public uint nDstBufSize;
    /// <summary>[OUT]    输出像素数据缓存长度</summary>
    public uint nDstBufLen;
    /// <summary>保存的点阵文件类型</summary>
    public MV_SAVE_POINT_CLOUD_FILE_TYPE enPointCloudFileType;
    /// <summary>保留字节</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public uint[] nRes;
  }
}
