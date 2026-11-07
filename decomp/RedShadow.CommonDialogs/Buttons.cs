using System;

namespace RedShadow.CommonDialogs;

[Flags]
public enum Buttons
{
	None = 0,
	Ok = 2,
	Cancel = 4,
	Yes = 8,
	No = 0x10,
	Abort = 0x20,
	Retry = 0x40,
	Ignore = 0x80
}
