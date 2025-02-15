// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.CameraParams.MV_GENTL_IF_INFO
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

using System.Runtime.InteropServices;

namespace MvsSharp.CameraParams
{
  /// <summary>
  /// ch:通过GenTL枚举到的Interface信息 | en:Interface Information with GenTL
  /// </summary>
  public struct MV_GENTL_IF_INFO
  {
    /// <summary>GenTL接口ID</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string chInterfaceID;
    /// <summary>传输层类型</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string chTLType;
    /// <summary>设备显示名称</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string chDisplayName;
    /// <summary>GenTL的cti文件索引</summary>
    public uint nCtiIndex;
    /// <summary>保留字节</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public uint[] nReserved;
  }
}
