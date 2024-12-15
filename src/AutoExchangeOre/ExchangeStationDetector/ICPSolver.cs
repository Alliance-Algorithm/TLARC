
using System;
using System.Collections.Generic;
using Accord.Math;
using Accord.Math.Decompositions;
using Accord.Math.Geometry;
using Accord.Statistics;
using Emgu.CV;
using Emgu.CV.Structure;

namespace AutoExchange.ExchangeStationDetector;

static class ICPSolver
{
    public static (Vector3d translation, Quaterniond quaternion) ICP(List<(MCvPoint3D32f point3dInCamera, MCvPoint3D32f Point3dInWorld)> pointPairs)
    {
        // 提取相机坐标系和世界坐标系下的点
        double[,] cameraPoints = Matrix.Zeros(3, pointPairs.Count);
        double[,] worldPoints = Matrix.Zeros(3, pointPairs.Count);

        for (int i = 0; i < pointPairs.Count; i++)
        {
            cameraPoints[0, i] = pointPairs[i].point3dInCamera.X;
            cameraPoints[1, i] = pointPairs[i].point3dInCamera.Y;
            cameraPoints[2, i] = pointPairs[i].point3dInCamera.Z;

            worldPoints[0, i] = pointPairs[i].Point3dInWorld.X;
            worldPoints[1, i] = pointPairs[i].Point3dInWorld.Y;
            worldPoints[2, i] = pointPairs[i].Point3dInWorld.Z;
        }

        // 计算质心
        double[] P = cameraPoints.Mean(1);
        double[] Q = worldPoints.Mean(1);

        // 中心化数据
        double[,] X = cameraPoints.Subtract(P, VectorType.ColumnVector);
        double[,] Y = worldPoints.Subtract(Q, VectorType.ColumnVector);

        // 计算协方差矩阵
        double[,] H = X.Dot(Y.Transpose());

        // 计算 SVD
        var svd = new Accord.Math.Decompositions.SingularValueDecomposition(H, true, true);
        double[,] U = svd.LeftSingularVectors;
        double[,] V = svd.RightSingularVectors;


        // 计算旋转矩阵
        double[,] R = V.Dot(U.Transpose());

        // 确保旋转矩阵是一个旋转矩阵而非反射矩阵
        var det = R.Determinant();
        if (det < 0)
        {
            V[0, 2] *= -1;
            V[1, 2] *= -1;
            V[2, 2] *= -1;
            R = V.Dot(U.Transpose());
        }

        // 计算平移
        double[] t = Q.Subtract(R.Dot(P));
        TlarcSystem.LogInfo($"trans:{t[0]:F2},{t[1]:F2},{t[2]:F2}");

        // 将旋转矩阵转换为四元数
        Quaterniond quaternion = Tools.MatrixToQuaternion(R);

        return (new(t[0], t[1], t[2]), quaternion);
    }
}
public static class Tools
{
    public static Quaterniond MatrixToQuaternion(double[,] R)
    {

        double trace = R[0, 0] + R[1, 1] + R[2, 2];

        if (trace > 0)
        {
            double s = 0.5 / Math.Sqrt(trace + 1.0);
            double w = 0.25 / s;
            double x = (R[2, 1] - R[1, 2]) * s;
            double y = (R[0, 2] - R[2, 0]) * s;
            double z = (R[1, 0] - R[0, 1]) * s;
            return new(x, y, z, w);
        }
        else
        {
            if (R[0, 0] > R[1, 1] && R[0, 0] > R[2, 2])
            {
                double s = 2.0 * Math.Sqrt(1.0 + R[0, 0] - R[1, 1] - R[2, 2]);
                double w = (R[2, 1] - R[1, 2]) / s;
                double x = 0.25 * s;
                double y = (R[0, 1] + R[1, 0]) / s;
                double z = (R[0, 2] + R[2, 0]) / s;
                return new(x, y, z, w);
            }
            else if (R[1, 1] > R[2, 2])
            {
                double s = 2.0 * Math.Sqrt(1.0 + R[1, 1] - R[0, 0] - R[2, 2]);
                double w = (R[0, 2] - R[2, 0]) / s;
                double x = (R[0, 1] + R[1, 0]) / s;
                double y = 0.25 * s;
                double z = (R[1, 2] + R[2, 1]) / s;
                return new(x, y, z, w);
            }
            else
            {
                double s = 2.0 * Math.Sqrt(1.0 + R[2, 2] - R[0, 0] - R[1, 1]);
                double w = (R[1, 0] - R[0, 1]) / s;
                double x = (R[0, 2] + R[2, 0]) / s;
                double y = (R[1, 2] + R[2, 1]) / s;
                double z = 0.25 * s;
                return new(x, y, z, w);
            }
        }
    }
}
