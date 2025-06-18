using System.Numerics;

namespace Kernel.Core.TransformTree;

internal class TransformCache
{
    private bool _changed = true;
    private Matrix4x4 _cache;

    public required string[] RootToFrom { get; init; }
    public required string[] RootToTo { get; init; }


    internal void ChangedCallback()
    {
        _changed = true;
    }

    internal ref Matrix4x4 GetTransform()
    {
        if (!_changed)
            return ref _cache;
        ref var cacheData = ref _cache;
        var fromRootMatrix = Task.Run(() =>
            {
                var mat = Matrix4x4.Identity;

                foreach (var matrixId in RootToFrom)
                {
                    var matrix = Tf.Nodes[matrixId].GetAffineRef();
                    mat = mat * matrix[0];
                }

                return mat;
            }
        );
        var rootToMatrix = Task.Run(() =>
            {
                var mat = Matrix4x4.Identity;

                foreach (var matrixId in RootToTo)
                {
                    var matrix = Tf.Nodes[matrixId].GetAffineRef();
                    mat = mat * matrix[0];
                }

                Matrix4x4.Invert(mat, out var retMat);
                return retMat;
            }
        );

        var result = Task.WhenAll([fromRootMatrix, rootToMatrix]);
        result.Wait();
        _cache = result.Result[1] * result.Result[0];
        _changed = false;
        return ref _cache;
    }
}