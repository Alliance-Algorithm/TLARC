namespace Kernel.Core.TransformTree;

public static class TlarcTfError
{
    public class SetNoNodeException(string nodeName)
        : ArgumentNullException(nodeName, $"TF do not include node {nodeName} but try to set it");

    public class FoundNoNodeException(string nodeName)
        : ArgumentNullException(nodeName, $"TF do not include node {nodeName} but try to find it");

    public class NoRoute(string from, string to)
        : ArgumentException($"{from},{to}", $"No toute from {from} to {to}");
}