// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.CameraParams.MV_CC_FILE_ACCESS_EX
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

using System.Runtime.InteropServices;

namespace MvsSharp.CameraParams
{
  /// <summary>文件存取</summary>
  public struct MV_CC_FILE_ACCESS_EX
  {
    /// <summary>用户文件数据缓存空间</summary>
    public IntPtr pUserFileBuf;
    /// <summary>用户数据缓存大小</summary>
    public uint nFileBufSize;
    /// <summary>文件实际缓存大小</summary>
    public uint nFileBufLen;
    /// <summary>设备文件名</summary>
    public string pDevFileName;
    /// <summary>保留字节</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
    public uint[] nReserved;
  }
}
