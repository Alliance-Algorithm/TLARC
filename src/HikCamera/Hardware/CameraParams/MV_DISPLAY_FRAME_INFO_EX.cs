// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.CameraParams.MV_DISPLAY_FRAME_INFO_EX
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

using System.Runtime.InteropServices;

namespace MvsSharp.CameraParams
{
  /// <summary>显示帧信息</summary>
  public struct MV_DISPLAY_FRAME_INFO_EX
  {
    /// <summary>图像宽</summary>
    public uint nWidth;
    /// <summary>图像高</summary>
    public uint nHeight;
    /// <summary>像素格式</summary>
    public MvGvspPixelType enPixelType;
    /// <summary>显示的帧数据</summary>
    public IntPtr pData;
    /// <summary>显示的帧数据大小</summary>
    public uint nDataLen;
    /// <summary>保留字节</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public uint[] nReserved;
  }
}
