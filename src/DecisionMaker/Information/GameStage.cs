namespace DecisionMaker;

public enum GameStage : byte
{
  NOT_START = 0,
  PREPARATION = 1,
  REFEREE_CHECK = 2,
  COUNTDOWN = 3,
  STARTED = 4,
  SETTLING = 5,
  UNKNOWN = byte.MaxValue,
}
