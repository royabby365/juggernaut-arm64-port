using System;

[Flags]
public enum DamageTypeE
{
	Natural = 1,
	Timeout = 2,
	DarkMagic = 4,
	HolyMagic = 8,
	IceMagic = 0x10,
	FireMagic = 0x20,
	AcidMagic = 0x40,
	LightingMagic = 0x80,
	BloodMagic = 0x100
}
