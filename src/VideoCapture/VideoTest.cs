using System.Collections.ObjectModel;
using System.Diagnostics;
using Emgu.CV;
using Emgu.CV.CvEnum;
using HikCamera.Hardware;
using TlarcKernel;
using TlarcKernel.IO;
using TlarcKernel.IO.ProcessCommunicateInterfaces;

namespace VideoCapture;

class VideoTest : Component
{
  ReadOnlyUnmanagedSubscription<Mat> cap = new("hello");
  string windowsName = "";

  public override void Start() { }

  public override void Update()
  {
    using var matPtr = cap.Rent;
    if (matPtr == null)
      return;
    CvInvoke.Imshow(windowsName, matPtr.Instance.Value);
    CvInvoke.WaitKey(1); // Wait for a key press to close the window}
  }
}
