// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.CameraParams.MV_CamL_DEV_INFO
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

using System.Runtime.InteropServices;

namespace MvsSharp.CameraParams
{
  /// <summary>ch:CamLink设备信息 | en:CamLink device information</summary>
  public struct MV_CamL_DEV_INFO
  {
    /// <summary>端口号ID</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string chPortID;
    /// <summary>模型名</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string chModelName;
    /// <summary>家族名</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string chFamilyName;
    /// <summary>设备版本信息</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string chDeviceVersion;
    /// <summary>制造商名字</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string chManufacturerName;
    /// <summary>序列号</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string chSerialNumber;
    /// <summary>保留字节</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 38)]
    public uint[] nReserved;
  }
}
