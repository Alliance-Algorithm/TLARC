namespace DecisionMaker;

[Flags]
enum FirePermission : sbyte
{
  Hero = 1 << 0,
  Engineer = 1 << 1,
  InfantryIII = 1 << 2,
  InfantryIV = 1 << 3,
  Sentry = 1 << 4,
  Outpost = 1 << 5,
  Base = 1 << 6,
}
