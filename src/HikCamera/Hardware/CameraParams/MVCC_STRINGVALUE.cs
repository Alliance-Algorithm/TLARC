// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.CameraParams.MVCC_STRINGVALUE
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

using System.Runtime.InteropServices;

namespace MvsSharp.CameraParams
{
  /// <summary>字符串型节点值</summary>
  public struct MVCC_STRINGVALUE
  {
    /// <summary>当前值</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
    public string chCurValue;
    /// <summary>节点值的最大长度</summary>
    public long nMaxLength;
    /// <summary>保留字节</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
    public uint[] nReserved;
  }
}
