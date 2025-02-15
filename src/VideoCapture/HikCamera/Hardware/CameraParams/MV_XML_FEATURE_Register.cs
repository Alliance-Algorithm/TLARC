// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.CameraParams.MV_XML_FEATURE_Register
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

using System.Runtime.InteropServices;

namespace MvsSharp.CameraParams
{
  /// <summary>寄存器型节点</summary>
  public struct MV_XML_FEATURE_Register
  {
    /// <summary>节点名</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string strName;
    /// <summary>显示名称</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string strDisplayName;
    /// <summary>节点描述</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
    public string strDescription;
    /// <summary>提示</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
    public string strToolTip;
    /// <summary>是否可见</summary>
    public MV_XML_Visibility enVisivility;
    /// <summary>访问模式</summary>
    public MV_XML_AccessMode enAccessMode;
    /// <summary>是否锁定。0-否；1-是</summary>
    public int bIsLocked;
    /// <summary>当前值</summary>
    public long nAddrValue;
    /// <summary>保留字节</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public uint[] nReserved;
  }
}
