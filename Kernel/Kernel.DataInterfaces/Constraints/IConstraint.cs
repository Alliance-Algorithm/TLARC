namespace Kernel.DataInterfaces.Constraints;

public interface IConstraint : ITlarcData
{
    /// <summary>
    /// <para>返回目标点在约束内的线性方程组</para>
    /// <para>假设点P = vecC X</para>
    /// <para>有 matA X &le; vecB</para>
    /// </summary>
    /// <param name="vecC">P = vecC x X</param>
    /// <returns>matA vecB</returns>
    (double[][] matA, double[] vecB) Generate(double[] vecC);
}
