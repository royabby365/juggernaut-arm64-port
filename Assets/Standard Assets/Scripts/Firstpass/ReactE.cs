using System;

[Flags]
public enum ReactE
{
	None = 0,
	Block = 1,
	Dodge = 2,
	Death = 4,
	Critical = 8,
	Damage = 0x10,
	Heal = 0x20,
	NoBubbles = 0x40,
	FromRages = 0x80,
	Fatality = 0x100
}
