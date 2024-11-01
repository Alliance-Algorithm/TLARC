// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.CameraParams.MV_ACTION_CMD_RESULT
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

using System.Runtime.InteropServices;

namespace MvsSharp.CameraParams
{
  /// <summary>动作命令结果</summary>
  public struct MV_ACTION_CMD_RESULT
  {
    /// <summary>IP address of the device</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
    public string strDeviceAddress;
    /// <summary>status code returned by the device</summary>
    public int nStatus;
    /// <summary>保留字节</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public uint[] nReserved;
  }
}
