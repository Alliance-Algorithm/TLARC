using Kernel.Contract;
using Kernel.Contract.Constraints;
using Kernel.Contract.Navigation;

namespace ALPlanner.Infrastructure.SafeCorridorConstruct;


public record struct SafeCorridorDecorator
{
    public static SafeCorridor2DData<T> Build<F, T>(SafeCorridor2DData<F> Circle2Ds) where F : T where T : IConstraint =>
        new() {
            Header = Circle2Ds.Header,
            Length = Circle2Ds.Length,
            Corridors = [.. from it in Circle2Ds.Corridors select (T)it]
        };
}