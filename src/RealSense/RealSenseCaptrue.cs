using Emgu.CV;
using Intel.RealSense;
using TlarcKernel.IO.ProcessCommunicateInterfaces;

namespace RealSenseForTlarc;

class RealSenseCapture : Component
{
    public ReadOnlyUnmanagedInterfacePublisher<Mat> rgb = new("/real_sense/rgb");
    public ReadOnlyUnmanagedInterfacePublisher<Mat> depth = new("/real_sense/depth");

    public override void Start()
    {

        Task.Run(() =>
        {
            using var ctx = new Context();
            using var pipeLine = new Pipeline(ctx);

            var cfg = new Config();
            cfg.EnableStream(Intel.RealSense.Stream.Depth, 1280, 720, Format.Z16, 30);
            cfg.EnableStream(Intel.RealSense.Stream.Color, 1280, 720, Format.Rgb8, 30);
            pipeLine.Start(cfg);

            while (true)
            {
                using var frames = pipeLine.WaitForFrames();
                using var depthFrame = frames.DepthFrame;

                Mat depthMat = new Mat(depthFrame.Height, depthFrame.Width, Emgu.CV.CvEnum.DepthType.Cv16U, 1);
                unsafe
                {
                    Buffer.MemoryCopy(depthFrame.Data.ToPointer(), depthMat.DataPointer.ToPointer(), depthFrame.Height * depthFrame.Width * 2, depthFrame.Height * depthFrame.Width * 2);
                }
                // 在此添加处理深度数据的代码

                // 获取颜色帧
                using var colorFrame = frames.ColorFrame;

                // 处理颜色帧数据
                var colorData = new byte[colorFrame.Width * colorFrame.Height * 3];
                Mat colorMat = new Mat(colorFrame.Height, colorFrame.Width, Emgu.CV.CvEnum.DepthType.Cv8U, 3);
                unsafe
                {
                    Buffer.MemoryCopy(colorFrame.Data.ToPointer(), colorMat.DataPointer.ToPointer(), colorFrame.Height * depthFrame.Width * 3, colorFrame.Height * depthFrame.Width * 3);
                }
                CvInvoke.CvtColor(colorMat, colorMat, Emgu.CV.CvEnum.ColorConversion.Rgb2Bgr);
                rgb.LoadInstance(ref colorMat);
                depth.LoadInstance(ref depthMat);

                // 在此添加处理颜色数据的代码
            }
        });
    }

}