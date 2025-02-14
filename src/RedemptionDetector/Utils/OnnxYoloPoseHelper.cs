using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace AutoExchange.RedemptionDetector.Utils;

public class OnnxYoloHelper
{
    private readonly InferenceSession _session;
    private readonly float _confidenceThreshold;

    public OnnxYoloHelper(string modelPath, float confidenceThreshold = 0.5f)
    {
        _session = new InferenceSession(modelPath);
        _confidenceThreshold = confidenceThreshold;
    }

    public Dictionary<int, (Rectangle box, List<Point> keypoints, float confidence, float keypointConfidenceSum)> ProcessImage(Mat image)
    {
        // 预处理图像
        var inputTensor = PreprocessImage(image);

        // 创建输入数据
        var inputName = _session.InputMetadata.Keys.First();
        var inputData = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor(inputName, inputTensor)
        };

        // 运行模型推理
        using var results = _session.Run(inputData);

        // 获取输出
        var outputName = _session.OutputMetadata.Keys.First();
        var outputTensor = results.First(x => x.Name == outputName).AsTensor<float>();

        // 处理输出数据
        return PostprocessOutput(outputTensor, image.Width / 640f, image.Height / 640f);
    }

    private DenseTensor<float> PreprocessImage(Mat image)
    {
        // 修改为模型的输入尺寸
        int targetWidth = 640;
        int targetHeight = 640;
        Mat resized = new Mat();
        CvInvoke.Resize(image, resized, new Size(targetWidth, targetHeight), interpolation: Inter.Linear);
        using var img = resized.ToImage<Bgr, byte>();
        float[] input = new float[1 * 3 * targetHeight * targetWidth];
        int stride = targetWidth * targetHeight;

        for (int y = 0; y < targetHeight; y++)
        {
            for (int x = 0; x < targetWidth; x++)
            {
                input[0 * stride + y * targetWidth + x] = img.Data[y, x, 0] / 255.0f; // B通道
                input[1 * stride + y * targetWidth + x] = img.Data[y, x, 1] / 255.0f; // G通道
                input[2 * stride + y * targetWidth + x] = img.Data[y, x, 2] / 255.0f; // R通道
            }
        }

        return new DenseTensor<float>(input, new[] { 1, 3, targetHeight, targetWidth });
    }

    private Dictionary<int, (Rectangle box, List<Point> keypoints, float confidence, float keypointConfidenceSum)> PostprocessOutput(Tensor<float> outputTensor, float originalWidth, float originalHeight)
    {
        var results = new Dictionary<int, (Rectangle, List<Point>, float, float)>();

        // 输出张量格式 [1, 29, 8400]
        int numDetections = 8400;
        int numKeypoints = 6;

        for (int i = 0; i < numDetections; i++)
        {
            int classId = 0;
            float confidence = -1;
            for (int j = 0; j < 7; j++)
                if (confidence < outputTensor[0, 4 + j, i])
                {
                    confidence = outputTensor[0, 4 + j, i];
                    classId = j;
                }

            if (confidence < _confidenceThreshold) continue;

            float x_center = outputTensor[0, 0, i] * originalWidth;
            float y_center = outputTensor[0, 1, i] * originalHeight;
            float width = outputTensor[0, 2, i] * originalWidth;
            float height = outputTensor[0, 3, i] * originalHeight;

            float x = x_center - width / 2;
            float y = y_center - height / 2;

            Rectangle box = new Rectangle((int)x, (int)y, (int)width, (int)height);
            List<Point> keypoints = new List<Point>();
            float keypointConfidenceSum = 0;

            for (int k = 0; k < numKeypoints; k++)
            {
                float px = outputTensor[0, 11 + 3 * k, i] * originalWidth;
                float py = outputTensor[0, 12 + 3 * k, i] * originalHeight;
                float kpConfidence = outputTensor[0, 13 + 3 * k, i];
                keypoints.Add(new((int)Math.Round(px), (int)Math.Round(py)));
                keypointConfidenceSum += kpConfidence;
            }

            results[classId] = results.ContainsKey(classId) ? (results[classId].Item3 > confidence ? results[classId] : (box, keypoints, confidence, keypointConfidenceSum)) : (box, keypoints, confidence, keypointConfidenceSum);
        }

        return results;
    }

    public void DrawDetections(Mat image, Dictionary<int, (Rectangle box, List<PointF> keypoints, float confidence, float keypointConfidenceSum)> detections)
    {
        var classNames = new[] { "Class0", "Class1", "Class2", "Class3", "Class4", "Class5", "Class6" };

        foreach (var detection in detections)
        {
            var (box, keypoints, confidence, keypointConfidenceSum) = detection.Value;

            // 绘制边界框
            CvInvoke.Rectangle(image, box, new MCvScalar(0, 255, 0), 2);

            // 绘制置信度和类别
            string label = $"{classNames[detection.Key]}: {confidence:F2} (KP Conf Sum: {keypointConfidenceSum:F2})";
            CvInvoke.PutText(image, label, new Point(box.X, box.Y - 10), FontFace.HersheySimplex, 0.5, new MCvScalar(0, 255, 0), 2);

            // 绘制关键点
            foreach (var point in keypoints)
            {
                CvInvoke.Circle(image, new System.Drawing.Point((int)point.X, (int)point.Y), 5, new MCvScalar(255, 0, 0), -1);
            }
        }
    }
}

