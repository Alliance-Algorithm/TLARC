// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.cbXmlUpdatedelegate
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

using MvsSharp.CameraParams;

namespace MvsSharp
{
  /// <summary>Xml Update callback(Interfaces not recommended)</summary>
  /// <param name="enType">Node type</param>
  /// <param name="pstFeature">Current node feature structure</param>
  /// <param name="pstNodesList">Nodes list</param>
  /// <param name="pUser">User defined variable</param>
  public delegate void cbXmlUpdatedelegate(
    MV_XML_InterfaceType enType,
    IntPtr pstFeature,
    ref MV_XML_NODES_LIST pstNodesList,
    IntPtr pUser);
}
