// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.CameraParams.MV_CHUNK_DATA_CONTENT
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

using System.Runtime.InteropServices;

namespace MvsSharp.CameraParams
{
  /// <summary>Chunk数据信息</summary>
  public struct MV_CHUNK_DATA_CONTENT
  {
    /// <summary>Chunk数据</summary>
    public IntPtr pChunkData;
    /// <summary>ChunkID</summary>
    public uint nChunkID;
    /// <summary>Chunk大小</summary>
    public uint nChunkLen;
    /// <summary>保留字节</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public uint[] nReserved;
  }
}
