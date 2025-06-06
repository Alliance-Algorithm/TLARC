﻿// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.CameraParams.MV_XML_NODES_LIST
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

using System.Runtime.InteropServices;

namespace MvsSharp.CameraParams
{
  /// <summary>XML节点列表</summary>
  public struct MV_XML_NODES_LIST
  {
    /// <summary>节点个数</summary>
    public uint nNodeNum;

    /// <summary>节点列表</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
    public MV_XML_NODE_FEATURE[] stNodes;
  }
}
