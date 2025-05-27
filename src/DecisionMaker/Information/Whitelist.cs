namespace DecisionMaker;

[Flags]
enum Whitelist : byte
{
    Hero = 0x1,
    Engineer = 0x2,
    InfantryIII = 0x4,
    InfantryIV = 0x8,
    InfantryV = 0x10,
    Sentry = 0x20,
    Outpost = 0x40,
    Base = 0x80
}