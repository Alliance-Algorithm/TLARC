// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.CameraParams.MV_ACTION_CMD_RESULT_LIST
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

namespace MvsSharp.CameraParams
{
  /// <summary>动作命令结果列表</summary>
  public struct MV_ACTION_CMD_RESULT_LIST
  {
    /// <summary>返回值个数</summary>
    public uint nNumResults;
    /// <summary>返回的结果</summary>
    public IntPtr pResults;
  }
}
