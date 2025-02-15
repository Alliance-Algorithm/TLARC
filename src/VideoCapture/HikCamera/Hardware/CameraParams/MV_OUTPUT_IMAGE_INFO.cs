// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.CameraParams.MV_OUTPUT_IMAGE_INFO
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

using System.Runtime.InteropServices;

namespace MvsSharp.CameraParams
{
  /// <summary>重构后的图像列表</summary>
  public struct MV_OUTPUT_IMAGE_INFO
  {
    /// <summary>源图像宽</summary>
    public uint nWidth;
    /// <summary>源图像高</summary>
    public uint nHeight;
    /// <summary>像素格式</summary>
    public MvGvspPixelType enPixelType;
    /// <summary>输出数据缓存</summary>
    public IntPtr pBuf;
    /// <summary>输出数据长度</summary>
    public uint nBufLen;
    /// <summary>提供的输出缓冲区大小</summary>
    public uint nBufSize;
    /// <summary>预留字节</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public uint[] nReserved;
  }
}
