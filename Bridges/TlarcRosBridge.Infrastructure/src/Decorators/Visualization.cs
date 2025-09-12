
using Kernel.DataInterfaces.Constraints;
using Kernel.DataInterfaces.Navigation;
using Rcl;
using TlarcRosBridge.Infrastructure.Messages.Builtin;

namespace TlarcRosBridge.Infrastructure.Decorators;


internal static class Visualization
{
    static void Draw(in Circle2D circle, ref Messages.Visualization.Marker.Priv marker)
    {
        marker.Type = Messages.Visualization.Marker.SPHERE;
        marker.Scale.X = circle.R * 2;
        marker.Scale.Y = circle.R * 2;
        marker.Scale.Z = 0.1;

        marker.Pose.Position.X = circle.Origin.X;
        marker.Pose.Position.Y = circle.Origin.Y;
        marker.Pose.Position.Z = 0;
    }
    static internal void Draw(in string id, IRclNode node, in ISafeCorridor2DData<Circle2D> circles, ref Messages.Visualization.MarkerArray.Priv markers)
    {
        markers.Markers = new(circles.Length);
        var span = markers.Markers.AsSpan();
        int i = 0;
        foreach (var circle in circles.Corridors)
        {
            Draw(circle, ref span[i]);
            span[i].Id = i;
            Std.FromData(id, node, ref span[i].Header);
            ++i;
        }
    }
}