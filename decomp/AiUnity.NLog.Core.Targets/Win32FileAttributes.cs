using System;

namespace AiUnity.NLog.Core.Targets;

[Flags]
public enum Win32FileAttributes
{
	Nothing = 0,
	ReadOnly = 1,
	Hidden = 2,
	System = 4,
	Archive = 0x20,
	Device = 0x40,
	Normal = 0x80,
	Temporary = 0x100,
	SparseFile = 0x200,
	ReparsePoint = 0x400,
	Compressed = 0x800,
	NotContentIndexed = 0x2000,
	Encrypted = 0x4000,
	WriteThrough = int.MinValue,
	NoBuffering = 0x20000000,
	DeleteOnClose = 0x4000000,
	PosixSemantics = 0x1000000
}
