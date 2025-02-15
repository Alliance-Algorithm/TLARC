// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.MV_GRAB_STRATEGY
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

namespace MvsSharp
{
  /// <summary>取流策略</summary>
  public enum MV_GRAB_STRATEGY
  {
    /// <summary>从旧到新一帧一帧的获取图像（默认为该策略）</summary>
    MV_GrabStrategy_OneByOne,
    /// <summary>获取列表中最新的一帧图像（同时清除列表中的其余图像）</summary>
    MV_GrabStrategy_LatestImagesOnly,
    /// <summary>
    /// 获取列表中最新的图像，个数由OutputQueueSize决定，范围为1-ImageNodeNum，设置成1等同于LatestImagesOnly，设置成ImageNodeNum等同于OneByOne
    /// </summary>
    MV_GrabStrategy_LatestImages,
    /// <summary>等待下一帧图像</summary>
    MV_GrabStrategy_UpcomingImage,
  }
}
