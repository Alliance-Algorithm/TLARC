using Emgu.CV;
using Intel.RealSense;
using TlarcKernel.IO.ProcessCommunicateInterfaces;

namespace RealSenseForTlarc;

class RealSenseCapture : Component
{
    public ReadOnlyUnmanagedInterfacePublisher<Mat> rgb = new("/real_sense/rgb");
    public ReadOnlyUnmanagedInterfacePublisher<Mat> depth = new("/real_sense/depth");
    public ReadOnlyUnmanagedInterfacePublisher<Points> pointCloud = new("/real_sense/frame/pc");

    public override void Start()
    {

        Task.Run(() =>
        {
            using var ctx = new Context();
            using var pipeLine = new Pipeline(ctx);

            var cfg = new Config();
            cfg.EnableStream(Intel.RealSense.Stream.Depth, 1280, 720, Format.Z16, 30);

            cfg.EnableStream(Intel.RealSense.Stream.Color, 1280, 720, Format.Rgb8, 30);
            var profile = pipeLine.Start(cfg);

            var device = profile.Device;
            var sensor = device.Sensors[0];
            sensor.Options[Option.EnableAutoExposure].Value = 0;
            sensor.Options[Option.Exposure].Value = 30f;
            while (true)
            {
                using var frames = pipeLine.WaitForFrames();
                var align = new Align(Intel.RealSense.Stream.Color);
                var alignedFrames = align.Process(frames).AsFrameSet();
                using var depthFrame = alignedFrames?.DepthFrame;
                using var colorFrame = alignedFrames?.ColorFrame;
                Mat depthMat = new Mat(depthFrame.Height, depthFrame.Width, Emgu.CV.CvEnum.DepthType.Cv16U, 1);
                unsafe
                {
                    Buffer.MemoryCopy(depthFrame.Data.ToPointer(), depthMat.DataPointer.ToPointer(), depthFrame.Height * depthFrame.Width * 2, depthFrame.Height * depthFrame.Width * 2);
                }


                // 处理颜色帧数据
                var colorData = new byte[colorFrame.Width * colorFrame.Height * 3];
                Mat colorMat = new Mat(colorFrame.Height, colorFrame.Width, Emgu.CV.CvEnum.DepthType.Cv8U, 3);
                unsafe
                {
                    Buffer.MemoryCopy(colorFrame.Data.ToPointer(), colorMat.DataPointer.ToPointer(), colorFrame.Height * colorFrame.Width * 3, colorFrame.Height * colorFrame.Width * 3);
                }
                using PointCloud pc = new();
                pc.MapTexture(colorFrame);
                var verticals = pc.Process(depthFrame).As<Points>();
                CvInvoke.CvtColor(colorMat, colorMat, Emgu.CV.CvEnum.ColorConversion.Rgb2Bgr);
                rgb.LoadInstance(ref colorMat);
                depth.LoadInstance(ref depthMat);
                pointCloud.LoadInstance(ref verticals);

                // 在此添加处理颜色数据的代码
            }
        });
    }

}