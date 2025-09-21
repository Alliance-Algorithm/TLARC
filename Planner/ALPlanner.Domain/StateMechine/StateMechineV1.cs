namespace ALPlanner.Domain.StateMechine;

public class StateMechineV1
{
    public enum State : byte
    {
        PathRebuilding = 0,
        CorridorRebuilding,
        TrajectoryRebuilding,
        Successed,
    }

    public State Value { get; set; }

    public void Trigger(bool Successed = true) => Value = Value switch
    {
        State.PathRebuilding => Successed ? State.CorridorRebuilding : State.PathRebuilding,
        State.CorridorRebuilding => Successed ? State.TrajectoryRebuilding : State.CorridorRebuilding,
        State.TrajectoryRebuilding => Successed ? State.Successed : State.TrajectoryRebuilding,
        State.Successed => State.Successed,
        _ => throw new NotImplementedException("State Error")
    };
    public void TargetChanged() => Value = State.PathRebuilding;
    public void CorridorCrush() => Value = Value > State.CorridorRebuilding ? State.CorridorRebuilding : Value;
    public void RoboExitCorri() => Value = Value > State.CorridorRebuilding ? State.CorridorRebuilding : Value;
    public void MoveNextCorri() => Value = Value > State.TrajectoryRebuilding ? State.TrajectoryRebuilding : Value;
    public void TrajNotSafety() => Value = Value > State.TrajectoryRebuilding ? State.TrajectoryRebuilding : Value;
}