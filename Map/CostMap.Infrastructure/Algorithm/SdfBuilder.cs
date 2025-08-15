namespace CostMap.Infrastructure.Algorithm;

public static class SdfBuilder
{
    private static (int stepX, int stepY) Step = Environment.ProcessorCount >= 9 ? (2, 2) :
        Environment.ProcessorCount >= 6 ? (1, 2) : (1, 1);

    internal enum SdfDistanceType
    {
        L1,
        L2
    }

    internal enum SdfForegroundValue
    {
        Min,
        Max
    }

    internal static void Dilate<T>(
        in int width,
        in int height,
        in int coreLength,
        in SdfDistanceType type,
        in SdfForegroundValue foreground,
        in Span<T> inData,
        out Span<T> outData)
    {
        outData = new T[inData.Length].AsSpan();
        var (stepX, stepY) = Step;
        if (width > height)
            (stepX, stepY) = (stepY, stepX);
    }
}