// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.CameraParams.MVCC_CIRCLE_INFO
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

using System.Runtime.InteropServices;

namespace MvsSharp.CameraParams
{
  /// <summary>圆形框区域信息</summary>
  public struct MVCC_CIRCLE_INFO
  {
    /// <summary>圆心信息</summary>
    public MVCC_POINTF stCenterPoint;
    /// <summary>宽向半径，根据图像的相对位置[0, 1.0]</summary>
    public float fR1;
    /// <summary>高向半径，根据图像的相对位置[0, 1.0]</summary>
    public float fR2;
    /// <summary>辅助线颜色信息</summary>
    public MVCC_COLORF stColor;
    /// <summary>辅助线宽度</summary>
    public uint nLineWidth;
    /// <summary>预留字节</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public uint[] nReserved;
  }
}
