// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.CameraParams.MVCC_LINES_INFO
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

using System.Runtime.InteropServices;

namespace MvsSharp.CameraParams
{
  /// <summary>线条辅助线信息</summary>
  public struct MVCC_LINES_INFO
  {
    /// <summary>线条辅助线的起始点坐标</summary>
    public MVCC_POINTF stStartPoint;
    /// <summary>线条辅助线的终点坐标</summary>
    public MVCC_POINTF stEndPoint;
    /// <summary>辅助线颜色信息</summary>
    public MVCC_COLORF stColor;
    /// <summary>辅助线宽度</summary>
    public uint nLineWidth;
    /// <summary>预留字节</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public uint[] nReserved;
  }
}
