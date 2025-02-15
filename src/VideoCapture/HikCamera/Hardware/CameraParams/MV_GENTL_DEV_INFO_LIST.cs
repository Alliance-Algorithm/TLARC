// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.CameraParams.MV_GENTL_DEV_INFO_LIST
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

using System.Runtime.InteropServices;

namespace MvsSharp.CameraParams
{
  /// <summary>ch:GenTL设备列表 | en:GenTL devices list</summary>
  public struct MV_GENTL_DEV_INFO_LIST
  {
    /// <summary>在线设备数量</summary>
    public uint nDeviceNum;
    /// <summary>支持最多256个设备</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
    public IntPtr[] pDeviceInfo;
  }
}
