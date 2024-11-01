
using System.Collections.ObjectModel;
using System.Diagnostics;
using Emgu.CV;
using Emgu.CV.CvEnum;
using HikCamera.Hardware;
using TlarcKernel;
using TlarcKernel.IO;
using TlarcKernel.IO.ProcessCommunicateInterfaces;

namespace HikCamera;

class VideoCapture : Component
{
    nint handle = 0;
    public ReadOnlyUnmanagedInterfacePublisher<Mat> readOnlyUnmanagedInterface = new("hello");
    public override void Start()
    {
        try
        {
            HikCameraApi.InitializeSDK();
            var list = HikCameraApi.EnumerateDevices();
            if (list.Count == 0)
                throw new("No USB Device");
            handle = HikCameraApi.Open(list[0]);
            if (handle == 0)
                throw new("Failed to Open Device");
            HikCameraApi.StartGrabbing(handle);
            // Mat img = CvInvoke.Imread("a.jpeg", ImreadModes.Color);
            TlarcSystem.LogInfo($"handle,ox{handle:X}");
            Task.Run(() =>
            {// Display the image
                try
                {
                    while (true)
                    {
                        var mat = HikCameraApi.GetImage(handle, 2000);
                        if (!mat.IsEmpty)
                            readOnlyUnmanagedInterface.LoadInstance(ref mat);
                        else
                        {
                            var list = HikCameraApi.EnumerateDevices();
                            if (list.Count == 0)
                                throw new("No USB Device");
                            handle = HikCameraApi.Open(list[0]);
                            if (handle == 0)
                                throw new("Failed to Open Device");
                            HikCameraApi.StartGrabbing(handle);
                        }
                    }
                }
                catch (Exception e) { TlarcSystem.LogError(e.Message); }
                finally
                {
                    if (handle != 0)
                    {
                        HikCameraApi.StopGrabbing(handle);
                        HikCameraApi.Close(handle);
                    }
                    HikCameraApi.FinalizeSDK();
                }
            });
        }
        catch (Exception e) { TlarcSystem.LogError(e.Message); }

    }

    public override void Update()
    {

    }

    ~VideoCapture()
    {


        if (handle != 0)
        {

            try
            {
                HikCameraApi.StopGrabbing(handle);
            }
            catch (Exception e) { TlarcSystem.LogError(e.Message); }
            try
            {
                HikCameraApi.Close(handle);
            }
            catch (Exception e) { TlarcSystem.LogError(e.Message); }
        }
        HikCameraApi.FinalizeSDK();
    }
}