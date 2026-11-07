using System;
using XRL.UI.Framework;

namespace Qud.UI;

[Serializable]
public class CyberneticsTerminalLineData : FrameworkDataElement
{
	public string Text;

	public int OptionID;

	public CyberneticsTerminalRow row;

	public CyberneticsTerminalLineData nextCursorData;

	public CyberneticsTerminalScreen screen;

	public bool CursorDone => row?.cursorDone ?? false;
}
