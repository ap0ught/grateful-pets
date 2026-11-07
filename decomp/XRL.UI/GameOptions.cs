using System;
using System.Collections.Generic;

namespace XRL.UI;

[Serializable]
public class GameOptions
{
	public Dictionary<string, string> Options = new Dictionary<string, string>(256);
}
