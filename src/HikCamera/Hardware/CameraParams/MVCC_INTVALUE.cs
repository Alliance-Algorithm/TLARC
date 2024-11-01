// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.CameraParams.MVCC_INTVALUE
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

using System.Runtime.InteropServices;

namespace MvsSharp.CameraParams
{
  /// <summary>整型节点值</summary>
  public struct MVCC_INTVALUE
  {
    /// <summary>当前值</summary>
    public uint nCurValue;
    /// <summary>最大值</summary>
    public uint nMax;
    /// <summary>最小值</summary>
    public uint nMin;
    /// <summary>Inc</summary>
    public uint nInc;
    /// <summary>保留字节</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public uint[] nReserved;
  }
}
