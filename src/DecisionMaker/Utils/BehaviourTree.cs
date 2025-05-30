using DecisionMaker;

enum BehaviourTreeType
{
  Sequence,
  Fallback,
  Parallel,
  Decorator,
  Action,
  Condition,
}

abstract class BehaviourTree : IAction
{
  public abstract BehaviourTreeType Type { get; }
  public abstract ActionState Action();
  protected List<BehaviourTree> Children { get; private set; } = [];
  protected BehaviourTree Parent { get; private set; }
}

abstract class BehaviourTreeControl : BehaviourTree
{
  public void AddChild(BehaviourTree node) => Children.Add(node);

  public void AddChildren(List<BehaviourTree> node) => Children.AddRange(node);

  public void RemoveChild(BehaviourTree node) => Children.Remove(node);
}

class BehaviourTreeAction<ActionType> : BehaviourTree
  where ActionType : IAction, new()
{
  public sealed override BehaviourTreeType Type => BehaviourTreeType.Action;
  private ActionType _actionType = new();

  public sealed override ActionState Action() => _actionType.Action();
}

class BehaviourTreeAction(Func<ActionState> action) : BehaviourTree
{
  public sealed override BehaviourTreeType Type => BehaviourTreeType.Action;
  private Func<ActionState> _actionType = action;

  public sealed override ActionState Action() => _actionType();
}

class BehaviourTreeCondition(Func<bool> action) : BehaviourTree
{
  public sealed override BehaviourTreeType Type => BehaviourTreeType.Condition;
  private Func<bool> _action = action;

  public sealed override ActionState Action() =>
    _action() ? ActionState.Success : ActionState.Failure;
}

class BehaviourTreeSequence() : BehaviourTreeControl
{
  public sealed override BehaviourTreeType Type => BehaviourTreeType.Sequence;

  public sealed override ActionState Action()
  {
    foreach (var act in Children)
    {
      var rst = act.Action();
      if (rst == ActionState.Success)
        continue;
      else
        return rst;
    }
    return ActionState.Success;
  }
}

class BehaviourTreeFallback : BehaviourTreeControl
{
  public sealed override BehaviourTreeType Type => BehaviourTreeType.Sequence;

  public sealed override ActionState Action()
  {
    foreach (var act in Children)
    {
      var rst = act.Action();
      if (rst == ActionState.Failure || rst == ActionState.Running)
        continue;
      else
        return rst;
    }
    return ActionState.Failure;
  }
}

class BehaviourTreeParallel : BehaviourTreeControl
{
  public sealed override BehaviourTreeType Type => BehaviourTreeType.Sequence;

  public sealed override ActionState Action()
  {
    ActionState rst = ActionState.Failure;
    foreach (var act in Children)
    {
      var rst_ = act.Action();
      if (rst_ == ActionState.Success)
        rst = ActionState.Success;
    }
    return rst;
  }
}
