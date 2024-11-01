// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.CameraParams.MVCC_RECT_INFO
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

using System.Runtime.InteropServices;

namespace MvsSharp.CameraParams
{
  /// <summary>矩形框区域信息</summary>
  public struct MVCC_RECT_INFO
  {
    /// <summary>[0.0 , 1.0]</summary>
    public float fTop;
    /// <summary>[0.0 , 1.0]</summary>
    public float fBottom;
    /// <summary>[0.0 , 1.0]</summary>
    public float fLeft;
    /// <summary>[0.0 , 1.0]</summary>
    public float fRight;
    /// <summary>辅助线颜色</summary>
    public MVCC_COLORF stColor;
    /// <summary>辅助线宽度</summary>
    public uint nLineWidth;
    /// <summary>预留字节</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public uint[] nReserved;
  }
}
