// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.CameraParams.MV_CC_RECORD_PARAM
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

using System.Runtime.InteropServices;

namespace MvsSharp.CameraParams
{
  /// <summary>录像参数</summary>
  public struct MV_CC_RECORD_PARAM
  {
    /// <summary>[IN]     输入数据的像素格式</summary>
    public MvGvspPixelType enPixelType;
    /// <summary>[IN]     图像宽(指定目标参数时需为8的倍数)</summary>
    public ushort nWidth;
    /// <summary>[IN]     图像高(指定目标参数时需为8的倍数)</summary>
    public ushort nHeight;
    /// <summary>[IN]     帧率fps(大于1/16)</summary>
    public float fFrameRate;
    /// <summary>[IN]     码率kbps(128kbps-16Mbps)</summary>
    public uint nBitRate;
    /// <summary>[IN]     录像格式</summary>
    public MV_RECORD_FORMAT_TYPE enRecordFmtType;
    /// <summary>[IN]     录像文件存放路径</summary>
    public string strFilePath;
    /// <summary>保留字节</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public uint[] nRes;
  }
}
