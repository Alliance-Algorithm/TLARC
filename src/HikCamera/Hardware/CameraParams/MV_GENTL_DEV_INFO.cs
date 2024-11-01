// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.CameraParams.MV_GENTL_DEV_INFO
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

using System.Runtime.InteropServices;

namespace MvsSharp.CameraParams
{
  /// <summary>
  /// ch:通过GenTL枚举到的设备信息 | en:Device Information discovered by with GenTL
  /// </summary>
  public struct MV_GENTL_DEV_INFO
  {
    /// <summary>采集卡ID</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string chInterfaceID;
    /// <summary>设备ID</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string chDeviceID;
    /// <summary>供应商名字</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string chVendorName;
    /// <summary>模型名</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string chModelName;
    /// <summary>传输类型</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string chTLType;
    /// <summary>显示名</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string chDisplayName;
    /// <summary>用户自定义名</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
    public byte[] chUserDefinedName;
    /// <summary>序列号</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string chSerialNumber;
    /// <summary>设备版本信息</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string chDeviceVersion;
    /// <summary>cti文件序号</summary>
    public uint nCtiIndex;
    /// <summary>保留字节</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public uint[] nReserved;
  }
}
