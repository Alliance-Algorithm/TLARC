namespace DecisionMaker;

enum ActionState
{
  Success,
  Failure,
  Running,
}

interface IAction
{
  ActionState Action();
}
