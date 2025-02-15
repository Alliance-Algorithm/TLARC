// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.CameraParams.MV_RECONSTRUCT_IMAGE_PARAM
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

using System.Runtime.InteropServices;

namespace MvsSharp.CameraParams
{
  /// <summary>重构图像参数信息</summary>
  public struct MV_RECONSTRUCT_IMAGE_PARAM
  {
    /// <summary>源图像宽</summary>
    public uint nWidth;
    /// <summary>源图像高</summary>
    public uint nHeight;
    /// <summary>像素格式</summary>
    public MvGvspPixelType enPixelType;
    /// <summary>输入数据缓存</summary>
    public IntPtr pSrcData;
    /// <summary>输入数据长度</summary>
    public uint nSrcDataLen;
    /// <summary>曝光个数(1-8]</summary>
    public uint nExposureNum;
    /// <summary>图像重构方式</summary>
    public MV_IMAGE_RECONSTRUCTION_METHOD enReconstructMethod;
    /// <summary>输出数据缓存信息</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public MV_OUTPUT_IMAGE_INFO[] stDstBufList;
    /// <summary>预留字节</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public uint[] nReserved;
  }
}
