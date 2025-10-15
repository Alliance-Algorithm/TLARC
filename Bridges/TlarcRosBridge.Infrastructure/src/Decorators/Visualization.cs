
using Kernel.Contract.Constraints;
using Kernel.Contract.Navigation;
using Kernel.Contract.Visualization;
using Rcl;
using TlarcRosBridge.Infrastructure.Messages.Builtin;

namespace TlarcRosBridge.Infrastructure.Decorators;


internal static class Visualization
{
    static void Draw(in Circle circle, ref Messages.Visualization.Marker.Priv marker)
    {
        marker.Type = Messages.Visualization.Marker.SPHERE;
        marker.Scale.X = circle.R * 2;
        marker.Scale.Y = circle.R * 2;
        marker.Scale.Z = 0.1;

        marker.Pose.Position.X = circle.Origin.X;
        marker.Pose.Position.Y = circle.Origin.Y;
        marker.Pose.Position.Z = 0;
    }
    static internal void Draw(in string id, IRclNode node,
    in SafeCorridor2DData<Circle> circles,
    ref Messages.Visualization.MarkerArray.Priv markers)
    {
        markers.Markers = new(circles.Length);
        var span = markers.Markers.AsSpan();
        int i = 0;
        foreach (var circle in circles.Corridors)
        {
            Draw(circle, ref span[i]);
            span[i].Id = i;
            span[i].Lifetime.Sec = 2;
            Std.FromData(id, node, ref span[i].Header);
            ++i;
        }
    }
    static void Draw(in Rectangle circle, ref Messages.Visualization.Marker.Priv marker)
    {
        marker.Type = Messages.Visualization.Marker.CUBE;
        marker.Scale.X = circle.Size.X;
        marker.Scale.Y = circle.Size.Y;
        marker.Scale.Z = 0.1;

        marker.Pose.Position.X = circle.Origin.X + circle.Size.X / 2;
        marker.Pose.Position.Y = circle.Origin.Y + circle.Size.Y / 2;
        marker.Pose.Position.Z = 0;
    }
    static internal void Draw(in string id, IRclNode node,
    in SafeCorridor2DData<Rectangle> rectangle,
    ref Messages.Visualization.MarkerArray.Priv markers)
    {
        markers.Markers = new(rectangle.Length);
        var span = markers.Markers.AsSpan();
        int i = 0;
        foreach (var circle in rectangle.Corridors)
        {
            Draw(circle, ref span[i]);
            span[i].Id = i;
            span[i].Lifetime.Sec = 2;
            Std.FromData(id, node, ref span[i].Header);
            ++i;
        }
    }
}