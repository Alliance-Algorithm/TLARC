using Accord.Math.Optimization;

namespace ALPlanner.TrajectoryOptimizer;
public class Constraint(int index, double? lowerBound = null, double? upperBound = null)
{
    private int _interIndex = index;
    public Constraint? next = null;

    readonly public double? LowerBound = lowerBound;
    readonly public double? UpperBound = upperBound;

    // exp: while(Constraint is not null) i += Iterator; Constraint = next;
    public int IteratorIncrementToNext => next is null ? 0 : next._interIndex - _interIndex;
}

public class ConstraintCollection
{
    public Constraint? XBegin;
    public Constraint? YBegin;
    public Constraint? ZBegin;

    public double TimeStep;
}

public static class ConstraintHelper
{
    public static LinearConstraintCollection BuildConstraint(double[] A, Constraint B, int[]? variablesAtIndices = null, double tolerance = 1E-12)
    => variablesAtIndices is null ? InternalBuildConstraint(A, B, tolerance) : InternalBuildConstraint(A, B, tolerance, variablesAtIndices);


    private static LinearConstraintCollection InternalBuildConstraint(double[] A, Constraint B, double tolerance)
    {
        if (B.LowerBound is not null && B.UpperBound is not null)
            if (B.LowerBound == B.UpperBound)
                return [new() { CombinedAs = A, ShouldBe = ConstraintType.EqualTo, Tolerance = tolerance, Value = (double)B.LowerBound }];
            else
                return [new() { CombinedAs = A, ShouldBe = ConstraintType.LesserThanOrEqualTo, Tolerance = tolerance, Value = (double)B.UpperBound },
                        new() { CombinedAs = A, ShouldBe = ConstraintType.GreaterThanOrEqualTo, Tolerance = tolerance, Value = (double)B.LowerBound }];
        else if (B.LowerBound is not null)
            return [new() { CombinedAs = A, ShouldBe = ConstraintType.GreaterThanOrEqualTo, Tolerance = tolerance, Value = (double)B.LowerBound }];
        else if (B.UpperBound is not null)
            return [new() { CombinedAs = A, ShouldBe = ConstraintType.GreaterThanOrEqualTo, Tolerance = tolerance, Value = (double)B.UpperBound }];
        else return [];
    }

    private static LinearConstraintCollection InternalBuildConstraint(double[] A, Constraint B, double tolerance, int[] variablesAtIndices)
    {
        if (B.LowerBound is not null && B.UpperBound is not null)
            if (B.LowerBound == B.UpperBound)
                return [new() { CombinedAs = A, ShouldBe = ConstraintType.EqualTo, Tolerance = tolerance, Value = (double)B.LowerBound, VariablesAtIndices = variablesAtIndices }];
            else
                return [new() { CombinedAs = A, ShouldBe = ConstraintType.LesserThanOrEqualTo, Tolerance = tolerance, Value = (double)B.UpperBound, VariablesAtIndices = variablesAtIndices },
                        new() { CombinedAs = A, ShouldBe = ConstraintType.GreaterThanOrEqualTo, Tolerance = tolerance, Value = (double)B.LowerBound, VariablesAtIndices = variablesAtIndices}];
        else if (B.LowerBound is not null)
            return [new() { CombinedAs = A, ShouldBe = ConstraintType.GreaterThanOrEqualTo, Tolerance = tolerance, Value = (double)B.LowerBound, VariablesAtIndices = variablesAtIndices }];
        else if (B.UpperBound is not null)
            return [new() { CombinedAs = A, ShouldBe = ConstraintType.GreaterThanOrEqualTo, Tolerance = tolerance, Value = (double)B.UpperBound, VariablesAtIndices = variablesAtIndices }];
        else return [];
    }
}