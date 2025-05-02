// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.CameraParams.MV_FRAME_OUT
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

using System.Runtime.InteropServices;

namespace MvsSharp.CameraParams
{
  /// <summary>输出帧信息</summary>
  [StructLayout(LayoutKind.Sequential, Pack = 8)]
  public struct MV_FRAME_OUT
  {
    /// <summary>帧数据地址</summary>
    public IntPtr pBufAddr;

    /// <summary>帧信息</summary>
    public MV_FRAME_OUT_INFO_EX stFrameInfo;

    /// <summary>保留字节</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public uint[] nReserved;
  }
}
