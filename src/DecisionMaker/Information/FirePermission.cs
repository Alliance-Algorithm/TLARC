namespace DecisionMaker;

[Flags]
enum FirePermission : byte
{
  Hero = 1 << 0,
  Engineer = 1 << 1,
  InfantryIII = 1 << 2,
  InfantryIV = 1 << 3,
  InfantryV = 1 << 4,
  Sentry = 1 << 5,
  Outpost = 1 << 6,
  Base = 1 << 7,
}
