using Accord;
using DecisionMaker;

namespace DecisionMaker.StateMachine;

abstract class StateMachineNodeBase
{
  internal delegate StateMachineNodeBase ChangeTo();
  internal abstract void Action();
  internal List<ChangeTo> checks = [];

  internal StateMachineNodeBase Check()
  {
    foreach (var i in checks)
      if (i() != this)
        return i();
    return this;
  }
};

class StateMachineNode<ActionType> : StateMachineNodeBase
  where ActionType : IAction, new()
{
  private ActionType _actionType = new();

  internal override void Action()
  {
    _actionType.Action();
  }

  public void LoadAction(ActionType act)
  {
    _actionType = act;
  }
}

class StateMachineNode(Action action) : StateMachineNodeBase
{
  private Action _action = action;

  internal override void Action() => _action();
}

class StateMachine
{
  private List<(CheckFunction check, StateMachineNodeBase target)> _any = [];
  public delegate bool CheckFunction();

  public void BeginTo(StateMachineNodeBase to) => Current = to;

  public void FromTo(
    StateMachineNodeBase form,
    CheckFunction checkFunction,
    StateMachineNodeBase to
  ) =>
    form.checks.Add(() =>
    {
      if (checkFunction())
        return to;
      else
        return form;
    });

  public void AnyTo(CheckFunction checkFunction, StateMachineNodeBase to) =>
    _any.Add((checkFunction, to));

  public void Step()
  {
    Current.Action();
    Current = Current.Check();
    foreach (var (check, target) in _any)
      if (check())
      {
        Current = target;
        return;
      }
  }

  StateMachineNodeBase Current;
}
