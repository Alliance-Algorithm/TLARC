
using System;
using System.Numerics;

namespace AllianceDM.ALPlanner;

internal class Node3 : IEquatable<Node3>, IComparable<Node3>, IThreeDimensional
{
    const float Equal_MaxDistance = 0.2f;
    const float Equal_MaxTheta = MathF.PI / 12.0f;
    const int Childs_Headings = 3;
    const float Childs_TimeInterval = 0.1f;
    const float Childs_Speed = 3;
    const float Childs_GInterval = Childs_Speed * Childs_TimeInterval;
    const float Childs_ThetaInterval = Equal_MaxTheta;
    const float Childs_ThetaMax = Childs_ThetaInterval * Childs_Headings / 2;

    const float HCalc_lambda11 = Equal_MaxTheta * 3;
    const float HCalc_lambda12 = 1;
    const float HCalc_lambda2 = 1f;
    const float HCalc_lambda3 = 10;
    const float HCalc_lambda4 = 1;
    internal Vector2 Pos;
    internal float Theta;

    internal Node3? Parent;

    internal float G;

    internal float F;

    float IThreeDimensional.X { get => Pos.X; set => Pos.X = value; }

    float IThreeDimensional.Y { get => Pos.X; set => Pos.Y = value; }

    float IThreeDimensional.Z { get => Theta; set => Theta = value; }

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

    internal Node3[] ChildrenGen()
    {
        Node3[] childs;
        if (Theta < float.MaxValue)
        {
            childs = new Node3[Childs_Headings];

            for (int i = 0; i < Childs_Headings; i++)
            {
                var theta = Childs_ThetaInterval * i + Theta - Childs_ThetaMax;
                childs[i] = new(Pos + Childs_GInterval * new Vector2(
                    MathF.Cos(theta),
                    MathF.Sin(theta)
                ), theta, this, G + Childs_GInterval);
            }
        }
        else
        {
            int headings = 36;
            childs = new Node3[headings];
            for (int i = 0; i < headings; i++)
            {
                float theta = (float)(i * 2 * Math.PI / headings);
                childs[i] = new(Pos + Childs_GInterval * new Vector2(
                    MathF.Cos(theta),
                    MathF.Sin(theta)
                ), theta, this, G + Childs_GInterval);
            }
        }
        return childs;
    }

    internal float CalcF(in Node3 Target, in GlobalESDFMap CostMap)
    {
        F = CalcH(Target, CostMap) + G;
        return F;
    }

    internal float CalcH(in Node3 Target, in GlobalESDFMap CostMap)
    {
        return AngleCost(Target) + ObsCost(CostMap) + TurnCost() + DistanceCost(Target);
    }

    private float AngleCost(in Node3 Target)
    {
        if (Target.Theta == float.MaxValue)
            return 0;
        var p = Target.Pos - Pos;
        var a = MathF.Atan(p.Y / p.X);
        return HCalc_lambda12 * MathF.Exp(
          Math.Abs(a - Target.Theta) - HCalc_lambda11
        );
    }
    private float DistanceCost(in Node3 Target)
    {
        return (Target.Pos - Pos).Length() * HCalc_lambda4;
    }

    private float ObsCost(in GlobalESDFMap CostMap)
    {
        var xy = CostMap.Vector2ToXY(Pos);
        return (100 - CostMap[xy.x, xy.y]) * HCalc_lambda2;
    }

    private float TurnCost()
    {
        if (Parent.Theta == float.MaxValue)
            return 0;
        return MathF.Abs(Parent.Theta - Theta) * HCalc_lambda3;
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

        if (Theta == float.MaxValue || other.Theta == float.MaxValue)
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