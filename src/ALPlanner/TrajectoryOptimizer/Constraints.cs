using Accord.Math.Optimization;

namespace ALPlanner.TrajectoryOptimizer;
public class Constraint(int index, double[,] rotation, double? lowerBound = null, double? upperBound = null)
{
    private int _interIndex = index;
    public Constraint? next = null;

    readonly public double? LowerBound = lowerBound;
    readonly public double? UpperBound = upperBound;
    readonly public double[,] Rotation = rotation;

    // exp: while(Constraint is not null) i += Iterator; Constraint = next;
    public int IteratorIncrementToNext => next is null ? 0 : next._interIndex - _interIndex;
}

public class ConstraintCollection
{
    public Constraint? XBegin;
    public Constraint? YBegin;
    public Constraint? ZBegin;

    public int Length;

    public double TimeStep;
}

public static class ConstraintHelper
{
    public static LinearConstraintCollection BuildConstraint(int numberOfVariables, double[] A, Constraint B, int[]? variablesAtIndices = null, double tolerance = 1E-12)
    => variablesAtIndices is null ? InternalBuildConstraint(numberOfVariables, A, B, tolerance) : InternalBuildConstraint(numberOfVariables, A, B, tolerance, variablesAtIndices);


    public static LinearConstraint CreateLinearConstraint(int numberOfVariables, double[] a, ConstraintType shouldBe, int[] variablesAtIndices, double value, double tolerance = 1E-12)
    {

        double[] A = new double[numberOfVariables];
        for (int i = 0; i < a.Length; i++)
            A[variablesAtIndices[i]] = a[i];
        return new(numberOfVariables)
        {
            CombinedAs = A,
            ShouldBe = shouldBe,
            Tolerance = tolerance,
            Value = value
        };
    }
    private static LinearConstraintCollection InternalBuildConstraint(int numberOfVariables, double[] A, Constraint B, double tolerance)
    {
        if (B.LowerBound is not null && B.UpperBound is not null)
            if (B.LowerBound == B.UpperBound)
                return [new(numberOfVariables) { CombinedAs = A, ShouldBe = ConstraintType.EqualTo, Tolerance = tolerance, Value = (double)B.LowerBound }];
            else
                return [new(numberOfVariables) { CombinedAs = A, ShouldBe = ConstraintType.LesserThanOrEqualTo, Tolerance = tolerance, Value = (double)B.UpperBound },
                        new(numberOfVariables) { CombinedAs = A, ShouldBe = ConstraintType.GreaterThanOrEqualTo, Tolerance = tolerance, Value = (double)B.LowerBound }];
        else if (B.LowerBound is not null)
            return [new(numberOfVariables) { CombinedAs = A, ShouldBe = ConstraintType.GreaterThanOrEqualTo, Tolerance = tolerance, Value = (double)B.LowerBound }];
        else if (B.UpperBound is not null)
            return [new(numberOfVariables) { CombinedAs = A, ShouldBe = ConstraintType.LesserThanOrEqualTo, Tolerance = tolerance, Value = (double)B.UpperBound }];
        else return [];
    }

    private static LinearConstraintCollection InternalBuildConstraint(int numberOfVariables, double[] a, Constraint B, double tolerance, int[] variablesAtIndices)
    {
        double[] A = new double[numberOfVariables];
        for (int i = 0; i < a.Length; i++)
            A[variablesAtIndices[i]] = a[i];
        if (B.LowerBound is not null && B.UpperBound is not null)
            if (B.LowerBound == B.UpperBound)
                return [new(numberOfVariables) { CombinedAs = A, ShouldBe = ConstraintType.EqualTo, Tolerance = tolerance, Value = (double)B.LowerBound }];
            else
                return [new(numberOfVariables) { CombinedAs = A, ShouldBe = ConstraintType.LesserThanOrEqualTo, Tolerance = tolerance, Value = (double)B.UpperBound},
                        new(numberOfVariables) { CombinedAs = A, ShouldBe = ConstraintType.GreaterThanOrEqualTo, Tolerance = tolerance, Value = (double)B.LowerBound}];
        else if (B.LowerBound is not null)
            return [new(numberOfVariables) { CombinedAs = A, ShouldBe = ConstraintType.GreaterThanOrEqualTo, Tolerance = tolerance, Value = (double)B.LowerBound }];
        else if (B.UpperBound is not null)
            return [new(numberOfVariables) { CombinedAs = A, ShouldBe = ConstraintType.GreaterThanOrEqualTo, Tolerance = tolerance, Value = (double)B.UpperBound }];
        else return [];
    }
}