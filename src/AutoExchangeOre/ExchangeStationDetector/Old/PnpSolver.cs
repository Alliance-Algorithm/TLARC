
using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
namespace AutoExchange.ExchangeStationDetector.Old;


class PnpSolver
{
    double[] cameraMatrixData = [1.722231837421459e+03, 0, 7.013056440882832e+02, 0, 1.724876404292754e+03, 5.645821718351237e+02, 0, 0, 1];
    double[] distCoeffsData = [-0.064232403853946, -0.087667493884102, 0, 0, 0.792381808294582];
    Mat cameraMatrix = new(3, 3, DepthType.Cv64F, 1);
    Mat distCoeffs = new(1, 5, DepthType.Cv64F, 1);

    public PnpSolver()
    {
        cameraMatrix.SetTo(cameraMatrixData);
        distCoeffs.SetTo(distCoeffsData);
    }


    public (Vector3d position, Quaterniond rotation) Solve(List<(Point Point2D, MCvPoint3D32f Point3D)> list, IInputOutputArray img)
    {
        using Mat rvec = new();
        using Mat tvec = new();

        if (!CvInvoke.SolvePnP(list.Select(x => x.Point3D).ToArray(), list.Select(x => new PointF(x.Point2D.X, x.Point2D.Y)).ToArray(), cameraMatrix, distCoeffs, rvec, tvec, false, SolvePnpMethod.EPnP))
            return (new(), new());

        using Mat rotationMatrix = new();
        CvInvoke.Rodrigues(rvec, rotationMatrix);
        Quaterniond quaternion = RotationMatrixToQuaternion(rotationMatrix);

        var arr = (double[,])tvec.GetData();
        Vector3d position = new(arr[0, 0], arr[1, 0], arr[2, 0]);


        MCvPoint3D32f[] axisPoints = [new(0, 0, 0), new(1, 0, 0), new(0, 1, 0), new(0, 0, 1)];
        var imagePoints = CvInvoke.ProjectPoints(axisPoints, rvec, tvec, cameraMatrix, distCoeffs);
        CvInvoke.Line(img, new Point((int)imagePoints[0].X, (int)imagePoints[0].Y), new Point((int)imagePoints[1].X, (int)imagePoints[1].Y), new MCvScalar(0, 0, 255), 2);
        CvInvoke.Line(img, new Point((int)imagePoints[0].X, (int)imagePoints[0].Y), new Point((int)imagePoints[2].X, (int)imagePoints[2].Y), new MCvScalar(0, 255, 0), 2);
        CvInvoke.Line(img, new Point((int)imagePoints[0].X, (int)imagePoints[0].Y), new Point((int)imagePoints[3].X, (int)imagePoints[3].Y), new MCvScalar(255, 0, 0), 2);
        return new(position, quaternion);
    }
    static Quaterniond RotationMatrixToQuaternion(Mat R)
    {
        var arr = (double[,])R.GetData();
        double m00 = arr[0, 0];
        double m01 = arr[0, 1];
        double m02 = arr[0, 2];
        double m10 = arr[1, 0];
        double m11 = arr[1, 1];
        double m12 = arr[1, 2];
        double m20 = arr[2, 0];
        double m21 = arr[2, 1];
        double m22 = arr[2, 2];
        double tr = m00 + m11 + m22;
        double qw, qx, qy, qz;
        if (tr > 0)
        {
            double S = Math.Sqrt(tr + 1.0) * 2;
            qw = 0.25 * S;
            qx = (m21 - m12) / S;
            qy = (m02 - m20) / S;
            qz = (m10 - m01) / S;
        }
        else if ((m00 > m11) & (m00 > m22))
        {
            double S = Math.Sqrt(1.0 + m00 - m11 - m22) * 2;
            qw = (m21 - m12) / S;
            qx = 0.25 * S;
            qy = (m01 + m10) / S; qz = (m02 + m20) / S;
        }
        else if (m11 > m22)
        {
            double S = Math.Sqrt(1.0 + m11 - m00 - m22) * 2;
            qw = (m02 - m20) / S;
            qx = (m01 + m10) / S;
            qy = 0.25 * S; qz = (m12 + m21) / S;
        }
        else
        {
            double S = Math.Sqrt(1.0 + m22 - m00 - m11) * 2;
            qw = (m10 - m01) / S;
            qx = (m02 + m20) / S;
            qy = (m12 + m21) / S;
            qz = 0.25 * S;
        }
        return new Quaterniond(qx, qy, qz, qw);
    }
}