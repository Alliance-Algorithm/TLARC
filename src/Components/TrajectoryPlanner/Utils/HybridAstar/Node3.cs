
using System;
using System.Numerics;

namespace Tlarc.TrajectoryPlanner.Utils;

internal class Node3 : IEquatable<Node3>, IComparable<Node3>
{
    const float Equal_MaxDistance = 0.2f;
    const float Equal_MaxTheta = MathF.PI / 12.0f;
    const int Children_Headings = 36;
    const float Children_TimeInterval = 0.1f;
    const float Children_Speed = 1.5f;
    const float Children_GInterval = Children_Speed * Children_TimeInterval;
    const float Children_ThetaInterval = Equal_MaxTheta;
    const float Children_ThetaMax = Children_ThetaInterval * Children_Headings / 2;

    const float HCalc_lambda11 = 0;
    const float HCalc_lambda12 = 1;
    const float HCalcLambdaObstacle = 0f;
    const float HCalcLambdaTurn = 1;
    const float HCalc_lambda4 = 0.1f;
    internal Vector2 Pos;
    internal float Theta;

    internal Node3? Parent;

    internal float G;

    internal float F;


    public Node3() { }
    internal Node3(Vector2 Pos, float Theta, Node3? Parent, float G)
    {
        this.Pos = Pos;
        this.Theta = Theta;
        this.Parent = Parent;
        this.G = G;
    }

    public Node3(in Node3? node3)
    {
        this.Pos = node3.Pos;
        this.Theta = node3.Theta;
    }

    internal Node3[] ChildrenGen(in GridMap CostMap)
    {
        List<Node3> children = new();
        if (Theta >= float.PositiveInfinity)
            Theta = 0;

        for (int i = 0; i < Children_Headings; i++)
        {
            var theta = (float)(i * 2 * Math.PI / Children_Headings) + Theta;
            if (CostMap.Cost(Pos) != GridMap.UnReachable)
                children.Add(new(Pos + Children_GInterval * new Vector2(
                MathF.Cos(theta),
                MathF.Sin(theta)
            ), theta, this, G + Children_GInterval));
        }
        return [.. children];
    }

    internal float CalcF(in Node3 Target, in GridMap CostMap)
    {
        F = CalcH(Target, CostMap) + G;
        return F;
    }

    internal float CalcH(in Node3 Target, in GridMap CostMap)
    {
        return AngleCost(Target) + ObsCost(CostMap) + TurnCost() + DistanceCost(Target);
    }
    private float AngleNormalize(double angle)
    {
        while (angle > Math.Tau)
            angle -= Math.Tau;
        while (angle < 0)
            angle += Math.Tau;
        return (float)angle;
    }
    private float AngleCost(in Node3 Target)
    {
        if (Target.Theta == float.PositiveInfinity)
            return 0;
        var p = Target.Pos - Pos;
        var a = MathF.Atan(p.Y / p.X);
        return HCalc_lambda12 * MathF.Exp(
          AngleNormalize(Math.Abs(a - Target.Theta)) - HCalc_lambda11
        );
    }
    private float DistanceCost(in Node3 Target)
    {
        return (Target.Pos - Pos).Length() * HCalc_lambda4;
    }

    private float ObsCost(in GridMap CostMap) => (100 - CostMap.Cost(Pos)) * HCalcLambdaObstacle;

    private float TurnCost()
    {
        if (Parent.Theta == float.PositiveInfinity)
            return 0;
        return AngleNormalize(MathF.Abs(Parent.Theta - Theta)) * HCalcLambdaTurn;
    }

    public static bool operator ==(Node3 node1, Node3 node2)
    {
        if (node1 is null && node2 is null)
            return true;
        else if (node2 is null || node1 is null)
            return false;
        return node1.Equals(node2);
    }

    public static bool operator !=(Node3 node1, Node3 node2)
    {
        if (node1 is null && node2 is null)
            return false;
        else if (node2 is null || node1 is null)
            return true;
        return !node1.Equals(node2);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Node3)
            return base.Equals(obj);
        else return Equals(obj as Node3);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public bool Equals(Node3? other)
    {
        if ((Math.Abs(Pos.X - other.Pos.X) + Math.Abs(Pos.Y - other.Pos.Y)) >= Equal_MaxDistance)
            return false;

        if (Theta == float.PositiveInfinity || other.Theta == float.PositiveInfinity)
            return true;
        if (Theta - other.Theta >= Equal_MaxTheta)
            return false;
        return true;
    }

    public int CompareTo(Node3? other)
    {
        return F.CompareTo(other.F);
    }

}