// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.CameraParams.MV_CC_LSC_CALIB_PARAM
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

using System.Runtime.InteropServices;

namespace MvsSharp.CameraParams
{
  /// <summary>LSC标定参数</summary>
  public struct MV_CC_LSC_CALIB_PARAM
  {
    /// <summary>[IN]     图像宽度(16~65536)</summary>
    public uint nWidth;
    /// <summary>[IN]     图像高度(16~65536)</summary>
    public uint nHeight;
    /// <summary>[IN]     输入的像素格式</summary>
    public MvGvspPixelType enPixelType;
    /// <summary>[IN]     输入图像缓存</summary>
    public IntPtr pSrcBuf;
    /// <summary>[IN]     输入图像缓存长度</summary>
    public uint nSrcBufLen;
    /// <summary>[OUT]    输出标定表缓存</summary>
    public IntPtr pCalibBuf;
    /// <summary>
    /// [IN]     提供的标定表缓冲大小（nWidth*nHeight*sizeof(unsigned short)）
    /// </summary>
    public uint nCalibBufSize;
    /// <summary>[OUT]    输出标定表缓存长度</summary>
    public uint nCalibBufLen;
    /// <summary>[IN]     宽度分块数</summary>
    public uint nSecNumW;
    /// <summary>[IN]     高度分块数</summary>
    public uint nSecNumH;
    /// <summary>[IN]     边缘填充系数，范围1~5</summary>
    public uint nPadCoef;
    /// <summary>
    /// [IN]     标定方式，0-中心为基准
    ///                    1-最亮区域为基准
    ///                    2-目标亮度
    /// </summary>
    public uint nCalibMethod;
    /// <summary>
    /// [IN]     目标亮度（8bits，[0,255])
    ///         （10bits，[0,1023])
    ///         （12bits，[0,4095])
    ///         （16bits，[0,65535])
    /// </summary>
    public uint nTargetGray;
    /// <summary>保留字节</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public uint[] nRes;
  }
}
