using Kernel.DataInterfaces;
using Kernel.DataInterfaces.Constraints;
using Kernel.DataInterfaces.Navigation;

namespace ALPlanner.Infrastructure.SafeCorridorConstruct;


public record struct SafeCorridorDecorator<T>(LinkedList<T> Circle2Ds, IHeader Header) : ISafeCorridor2DData<T> where T : IConstraint
{

    readonly public int Length => Corridors.Length;

    readonly public T[] Corridors { get; } = [.. Circle2Ds];
}

public record struct SafeCorridorDecorator<F, T>(ISafeCorridor2DData<F> Circle2Ds) : ISafeCorridor2DData<T> where F : T where T : IConstraint
{
    readonly public IHeader Header { get; } = Circle2Ds.Header;
    readonly public int Length { get; } = Circle2Ds.Length;

    readonly public T[] Corridors { get; } = [.. from it in Circle2Ds.Corridors select (T)it];

}