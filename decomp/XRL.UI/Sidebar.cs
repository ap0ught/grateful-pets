using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleLib.Console;
using Cysharp.Text;
using UnityEngine;
using XRL.Core;
using XRL.Rules;
using XRL.World;
using XRL.World.Capabilities;
using XRL.World.Effects;
using XRL.World.Parts;
using XRL.World.Text;

namespace XRL.UI;

public class Sidebar
{
	public static string State = "right";

	public static string VState = "top";

	public static bool _Hidden = false;

	public static int _SidebarState = 0;

	public static XRL.World.GameObject _CurrentTarget = null;

	public static Dictionary<XRL.World.GameObject, string> AutogotItems = new Dictionary<XRL.World.GameObject, string>();

	private static XRL.World.GameObject PlayerBody = null;

	public static string sWeight = "";

	public static string sPlayerHPColor;

	public static int LastHP = -1;

	public static int LastHPBase = -1;

	public static int LastWeight = -1;

	public static int LastMaxWeight = -1;

	public static bool Analgesic = false;

	public static StringBuilder SB = new StringBuilder(2048);

	public static List<string> MessageCache = null;

	public static float XPBarPercentage = 0f;

	public static bool bOverlayUpdated = false;

	public static int _CurrentXP = 0;

	public static int _NextXP = 0;

	public static string _sPlayerHP = "";

	public static int _CurrentHP = 0;

	public static int _MaxHP = 0;

	public static string _AbilityText;

	public static string _LeftText;

	public static string _RightText;

	public static Dictionary<char, int> Codepage437Mapping = new Dictionary<char, int>
	{
		{ '\0', 32 },
		{ '\u0001', 9786 },
		{ '\u0002', 9787 },
		{ '\u0003', 9829 },
		{ '\u0004', 9830 },
		{ '\u0005', 9827 },
		{ '\u0006', 9824 },
		{ '\a', 8226 },
		{ '\b', 9688 },
		{ '\t', 9675 },
		{ '\v', 9794 },
		{ '\f', 9792 },
		{ '\u000e', 9835 },
		{ '\u000f', 9788 },
		{ '\u0010', 9654 },
		{ '\u0011', 9664 },
		{ '\u0012', 8597 },
		{ '\u0013', 8252 },
		{ '\u0014', 182 },
		{ '\u0015', 167 },
		{ '\u0016', 9602 },
		{ '\u0017', 8616 },
		{ '\u0018', 8593 },
		{ '\u0019', 8595 },
		{ '\u001a', 8594 },
		{ '\u001b', 8592 },
		{ '\u001c', 8735 },
		{ '\u001d', 8596 },
		{ '\u001e', 9650 },
		{ '\u001f', 9660 },
		{ '\u0080', 199 },
		{ '\u0081', 252 },
		{ '\u0082', 233 },
		{ '\u0083', 226 },
		{ '\u0084', 228 },
		{ '\u0085', 224 },
		{ '\u0086', 229 },
		{ '\u0087', 231 },
		{ '\u0088', 234 },
		{ '\u0089', 235 },
		{ '\u008a', 232 },
		{ '\u008b', 239 },
		{ '\u008c', 238 },
		{ '\u008d', 236 },
		{ '\u008e', 196 },
		{ '\u008f', 197 },
		{ '\u0090', 201 },
		{ '\u0091', 230 },
		{ '\u0092', 198 },
		{ '\u0093', 244 },
		{ '\u0094', 246 },
		{ '\u0095', 242 },
		{ '\u0096', 251 },
		{ '\u0097', 249 },
		{ '\u0098', 255 },
		{ '\u0099', 214 },
		{ '\u009a', 220 },
		{ '\u009b', 162 },
		{ '\u009c', 163 },
		{ '\u009d', 165 },
		{ '\u009e', 8359 },
		{ '\u009f', 402 },
		{ '\u00a0', 225 },
		{ '¡', 237 },
		{ '¢', 243 },
		{ '£', 250 },
		{ '¤', 241 },
		{ '¥', 209 },
		{ '¦', 170 },
		{ '§', 186 },
		{ '\u00a8', 191 },
		{ '©', 8976 },
		{ 'ª', 172 },
		{ '«', 189 },
		{ '¬', 188 },
		{ '\u00ad', 161 },
		{ '®', 171 },
		{ '\u00af', 187 },
		{ '°', 9617 },
		{ '±', 9618 },
		{ '²', 9619 },
		{ '³', 9474 },
		{ '\u00b4', 9508 },
		{ 'µ', 9569 },
		{ '¶', 9570 },
		{ '·', 9558 },
		{ '\u00b8', 9557 },
		{ '¹', 9571 },
		{ 'º', 9553 },
		{ '»', 9559 },
		{ '¼', 9565 },
		{ '½', 9564 },
		{ '¾', 9563 },
		{ '¿', 9488 },
		{ 'À', 9492 },
		{ 'Á', 9524 },
		{ 'Â', 9516 },
		{ 'Ã', 9500 },
		{ 'Ä', 9472 },
		{ 'Å', 9532 },
		{ 'Æ', 9566 },
		{ 'Ç', 9567 },
		{ 'È', 9562 },
		{ 'É', 9556 },
		{ 'Ê', 9577 },
		{ 'Ë', 9574 },
		{ 'Ì', 9568 },
		{ 'Í', 9552 },
		{ 'Î', 9580 },
		{ 'Ï', 9575 },
		{ 'Ð', 9576 },
		{ 'Ñ', 9572 },
		{ 'Ò', 9573 },
		{ 'Ó', 9561 },
		{ 'Ô', 9560 },
		{ 'Õ', 9554 },
		{ 'Ö', 9555 },
		{ '×', 9579 },
		{ 'Ø', 9578 },
		{ 'Ù', 9496 },
		{ 'Ú', 9484 },
		{ 'Û', 9608 },
		{ 'Ü', 9604 },
		{ 'Ý', 9612 },
		{ 'Þ', 9616 },
		{ 'ß', 9600 },
		{ 'à', 945 },
		{ 'á', 223 },
		{ 'â', 915 },
		{ 'ã', 960 },
		{ 'ä', 931 },
		{ 'å', 963 },
		{ 'æ', 181 },
		{ 'ç', 964 },
		{ 'è', 934 },
		{ 'é', 920 },
		{ 'ê', 937 },
		{ 'ë', 948 },
		{ 'ì', 8734 },
		{ 'í', 966 },
		{ 'î', 949 },
		{ 'ï', 8745 },
		{ 'ð', 8801 },
		{ 'ñ', 177 },
		{ 'ò', 8805 },
		{ 'ó', 8804 },
		{ 'ô', 8992 },
		{ 'õ', 8993 },
		{ 'ö', 247 },
		{ '÷', 8776 },
		{ 'ø', 176 },
		{ 'ù', 8729 },
		{ 'ú', 183 },
		{ 'û', 8730 },
		{ 'ü', 8319 },
		{ 'ý', 178 },
		{ 'þ', 9632 },
		{ 'ÿ', 160 },
		{ '\u2007', 1 },
		{ '\ueea4', 164 },
		{ '\ueea6', 166 },
		{ '\ueea8', 168 },
		{ '\ueea9', 169 },
		{ '\ueead', 173 },
		{ '\ueeae', 174 },
		{ '\ueeaf', 175 },
		{ '\ueeb3', 179 },
		{ '\ueeb4', 180 },
		{ '\ueeb8', 184 },
		{ '\ueeb9', 185 },
		{ '\ueebe', 190 },
		{ '\ueec0', 192 },
		{ '\ueec1', 193 },
		{ '\ueec2', 194 },
		{ '\ueec3', 195 },
		{ '\ueec8', 200 },
		{ '\ueeca', 202 },
		{ '\ueecb', 203 },
		{ '\ueecc', 204 },
		{ '\ueecd', 205 },
		{ '\ueece', 206 },
		{ '\ueecf', 207 },
		{ '\ueed0', 208 },
		{ '\ueed2', 210 },
		{ '\ueed3', 211 },
		{ '\ueed4', 212 },
		{ '\ueed5', 213 },
		{ '\ueed7', 215 },
		{ '\ueed8', 216 },
		{ '\ueed9', 217 },
		{ '\ueeda', 218 },
		{ '\ueedb', 219 },
		{ '\ueedd', 221 },
		{ '\ueede', 222 },
		{ '\ueee3', 227 },
		{ '\ueef0', 240 },
		{ '\ueef5', 245 },
		{ '\ueef8', 248 },
		{ '\ueefd', 253 },
		{ '\ueefe', 254 }
	};

	public static Dictionary<char, int> Codepage437Inverse;

	public static long SidebarTick = 0L;

	public static long LastRenderedTick = -1L;

	[NonSerialized]
	public static List<string> Objects = new List<string>();

	public static bool Hidden
	{
		get
		{
			if (Options.ShiftHidesSidebar && Keyboard.bShift && GameManager.Instance?.CurrentGameView == "Stage")
			{
				return !_Hidden;
			}
			return _Hidden;
		}
		set
		{
			_Hidden = value;
		}
	}

	public static int SidebarState
	{
		get
		{
			return _SidebarState;
		}
		set
		{
			The.Game.Player.Messages.Cache_0_12Valid = false;
			_SidebarState = value;
		}
	}

	public static XRL.World.GameObject CurrentTarget
	{
		get
		{
			XRL.World.GameObject.Validate(ref _CurrentTarget);
			return _CurrentTarget;
		}
		set
		{
			if (value != _CurrentTarget && (value == null || !value.IsPlayer()))
			{
				_CurrentTarget = value;
				if (_CurrentTarget != null && !AutoAct.Attacking && _CurrentTarget.IsHostileTowards(The.Player))
				{
					AutoAct.Interrupt(null, null, _CurrentTarget, IsThreat: true);
				}
			}
		}
	}

	[Obsolete]
	public static bool WaitingForHPWarning
	{
		get
		{
			return The.Core.HPWarning;
		}
		set
		{
			The.Core.HPWarning = value;
		}
	}

	public static int CurrentXP
	{
		get
		{
			return _CurrentXP;
		}
		set
		{
			if (_CurrentXP != value)
			{
				bOverlayUpdated = true;
			}
			_CurrentXP = value;
		}
	}

	public static int NextXP
	{
		get
		{
			return _NextXP;
		}
		set
		{
			if (_NextXP != value)
			{
				bOverlayUpdated = true;
			}
			_NextXP = value;
		}
	}

	public static string sPlayerHP
	{
		get
		{
			return _sPlayerHP;
		}
		set
		{
			if (_sPlayerHP != value)
			{
				bOverlayUpdated = true;
			}
			_sPlayerHP = value;
		}
	}

	public static int CurrentHP
	{
		get
		{
			return _CurrentHP;
		}
		set
		{
			if (_CurrentHP != value)
			{
				bOverlayUpdated = true;
			}
			_CurrentHP = value;
		}
	}

	public static int MaxHP
	{
		get
		{
			return _MaxHP;
		}
		set
		{
			if (_MaxHP != value)
			{
				bOverlayUpdated = true;
			}
			_MaxHP = value;
		}
	}

	public static string AbilityText
	{
		get
		{
			return _AbilityText;
		}
		set
		{
			if (_AbilityText != value)
			{
				bOverlayUpdated = true;
			}
			_AbilityText = value;
		}
	}

	public static string LeftText
	{
		get
		{
			return _LeftText;
		}
		set
		{
			if (_LeftText != value)
			{
				bOverlayUpdated = true;
			}
			_LeftText = value;
		}
	}

	public static string RightText
	{
		get
		{
			return _RightText;
		}
		set
		{
			if (_RightText != value)
			{
				bOverlayUpdated = true;
			}
			_RightText = value;
		}
	}

	public static void FormatToRTF(TextBuilder Source, TextBuilder Dest, string opacity = "FF")
	{
		bool flag = false;
		Dest.Clear();
		for (int i = 0; i < Source.Length; i++)
		{
			char c = Source[i];
			switch (c)
			{
			case '&':
				if (i < Source.Length - 1 && Source[i + 1] == '&')
				{
					i++;
					Dest.Append("&");
					break;
				}
				if (flag)
				{
					Dest.Append("</color>");
				}
				flag = true;
				i++;
				if (i < Source.Length)
				{
					Dest.Append("<color=#");
					if (!ConsoleLib.Console.ColorUtility.ColorMap.ContainsKey(Source[i]))
					{
						Debug.Log("Unknown color code: " + Source[i]);
						Dest.Append("DD00DDFF>");
						break;
					}
					Color color = ConsoleLib.Console.ColorUtility.ColorMap[Source[i]];
					Dest.AppendHex2((int)Math.Min(color.r * 255f, 255f));
					Dest.AppendHex2((int)Math.Min(color.g * 255f, 255f));
					Dest.AppendHex2((int)Math.Min(color.b * 255f, 255f));
					Dest.Append(opacity);
					Dest.Append(">");
				}
				break;
			case '^':
				if (i < Source.Length - 1 && Source[i + 1] == '^')
				{
					i++;
					Dest.Append("^");
				}
				else
				{
					i++;
				}
				break;
			default:
			{
				if (Codepage437Mapping.TryGetValue(c, out var value))
				{
					c = (char)value;
				}
				Dest.Append(c);
				break;
			}
			}
		}
		if (flag)
		{
			Dest.Append("</color>");
		}
	}

	public static void UpdateState()
	{
		if (The.Player?.Physics == null)
		{
			return;
		}
		XRL.World.Parts.Physics physics = The.Player.Physics;
		if (physics != null && physics.CurrentCell != null)
		{
			if (physics.CurrentCell.Y > 10 && VState == "bottom")
			{
				SetSidebarVState("top");
			}
			if (physics.CurrentCell.Y < 8 && VState == "top")
			{
				SetSidebarVState("bottom");
			}
			if (physics.CurrentCell.X > 42 && State == "right")
			{
				SetSidebarState("left");
			}
			if (physics.CurrentCell.X < 38 && State == "left")
			{
				SetSidebarState("right");
			}
		}
	}

	public static bool AnyAutogotItems()
	{
		return AutogotItems.Count > 0;
	}

	public static void ClearAutogotItems()
	{
		AutogotItems.Clear();
	}

	public static void AddAutogotItem(XRL.World.GameObject GO)
	{
		if (!AutogotItems.ContainsKey(GO) && GO.Render != null)
		{
			AutogotItems.Add(GO, "<autogot> " + GO.ShortDisplayName);
		}
	}

	public static void SetSidebarVState(string _State)
	{
		VState = _State;
	}

	public static void SetSidebarState(string _State)
	{
		State = _State;
	}

	public static void Update()
	{
		if (PlayerBody == null)
		{
			return;
		}
		if (PlayerBody.Physics == null)
		{
			PlayerBody = null;
			return;
		}
		sPlayerHPColor = PlayerBody.GetHPColor();
		if (PlayerBody.GetIntProperty("Analgesia") > 0)
		{
			sPlayerHP = Strings.WoundLevel(PlayerBody);
			Analgesic = true;
		}
		else
		{
			Analgesic = false;
			int hitpoints = PlayerBody.hitpoints;
			int baseHitpoints = PlayerBody.baseHitpoints;
			if (hitpoints != LastHP || baseHitpoints != LastHPBase)
			{
				LastHP = hitpoints;
				LastHPBase = baseHitpoints;
				sPlayerHP = XRL.World.Event.NewStringBuilder().Append(sPlayerHPColor).Append(hitpoints)
					.Append(" &y/ ")
					.Append(LastHPBase)
					.ToString();
			}
		}
		int carriedWeight = PlayerBody.GetCarriedWeight();
		int maxCarriedWeight = PlayerBody.GetMaxCarriedWeight();
		if (carriedWeight != LastWeight || maxCarriedWeight != LastMaxWeight)
		{
			LastWeight = carriedWeight;
			LastMaxWeight = maxCarriedWeight;
			sWeight = XRL.World.Event.NewStringBuilder().Append('#').Append(carriedWeight)
				.Append('/')
				.Append(maxCarriedWeight)
				.ToString();
		}
	}

	public static void DrawMessageBox(ScreenBuffer _ScreenBuffer, int x1, int y1, int x2, int y2)
	{
	}

	private static void PutTargetInSB()
	{
		string text = CurrentTarget.DisplayName;
		if (ConsoleLib.Console.ColorUtility.LengthExceptFormatting(SB) > 21)
		{
			text = ConsoleLib.Console.ColorUtility.ClipExceptFormatting(text, 21);
		}
		SB.Clear().Append("{{R|[{{y|").Append(text)
			.Append("}}]}}");
	}

	public static string ToCP437(string s)
	{
		if (s == null)
		{
			return null;
		}
		if (Codepage437Inverse == null)
		{
			Codepage437Inverse = Codepage437Mapping.Where((KeyValuePair<char, int> kv) => kv.Value != 32).ToDictionary((Func<KeyValuePair<char, int>, char>)((KeyValuePair<char, int> kv) => (char)kv.Value), (Func<KeyValuePair<char, int>, int>)((KeyValuePair<char, int> kv) => kv.Key));
		}
		using Utf16ValueStringBuilder utf16ValueStringBuilder = ZString.CreateStringBuilder();
		for (int num = 0; num < s.Length; num++)
		{
			char c = s[num];
			if (Codepage437Inverse.TryGetValue(c, out var value))
			{
				c = (char)value;
			}
			utf16ValueStringBuilder.Append(c);
		}
		return utf16ValueStringBuilder.ToString();
	}

	public static string FromCP437(string s)
	{
		if (s == null)
		{
			return null;
		}
		using Utf16ValueStringBuilder utf16ValueStringBuilder = ZString.CreateStringBuilder();
		for (int i = 0; i < s.Length; i++)
		{
			char c = s[i];
			if (Codepage437Mapping.TryGetValue(c, out var value))
			{
				c = (char)value;
			}
			utf16ValueStringBuilder.Append(c);
		}
		return utf16ValueStringBuilder.ToString();
	}

	public static string FormatToRTF(string s, string opacity = "FF")
	{
		if (s == null)
		{
			return "";
		}
		Utf16ValueStringBuilder sb = ZString.CreateStringBuilder();
		try
		{
			FormatToRTF(s, ref sb, opacity);
			return sb.ToString();
		}
		finally
		{
			sb.Dispose();
		}
	}

	public static void FormatToRTF(string s, ref Utf16ValueStringBuilder sb, string opacity = "FF")
	{
		if (s == null)
		{
			return;
		}
		s = Markup.Transform(s);
		bool flag = false;
		for (int i = 0; i < s.Length; i++)
		{
			if (s[i] == '&')
			{
				if (i < s.Length - 1 && s[i + 1] == '&')
				{
					i++;
					sb.Append("&");
					continue;
				}
				if (flag)
				{
					sb.Append("</color>");
				}
				flag = true;
				i++;
				if (i < s.Length)
				{
					sb.Append("<color=#");
					if (!ConsoleLib.Console.ColorUtility.ColorMap.ContainsKey(s[i]))
					{
						Debug.Log("Unknown color code: " + s[i]);
						sb.Append("DD00DDFF>");
						continue;
					}
					Color color = ConsoleLib.Console.ColorUtility.ColorMap[s[i]];
					sb.Append(((int)Math.Min(color.r * 255f, 255f)).ToString("X2"));
					sb.Append(((int)Math.Min(color.g * 255f, 255f)).ToString("X2"));
					sb.Append(((int)Math.Min(color.b * 255f, 255f)).ToString("X2"));
					sb.Append(opacity);
					sb.Append(">");
				}
			}
			else if (s[i] == '^')
			{
				if (i < s.Length - 1 && s[i + 1] == '^')
				{
					i++;
					sb.Append("^");
				}
				else
				{
					i++;
				}
			}
			else
			{
				char c = s[i];
				if (Codepage437Mapping.TryGetValue(c, out var value))
				{
					c = (char)value;
				}
				sb.Append(c);
			}
		}
		if (flag)
		{
			sb.Append("</color>");
		}
	}

	public static void Render(ScreenBuffer _ScreenBuffer)
	{
		if (GameManager.bDraw == 23 || Keyboard.bAlt || GameManager.bDraw == 24)
		{
			return;
		}
		if (Options.ModernUI)
		{
			if (LastRenderedTick == SidebarTick)
			{
				return;
			}
			LastRenderedTick = SidebarTick;
			PlayerBody = The.Player;
			if (PlayerBody == null)
			{
				return;
			}
			if (The.Player.GetIntProperty("Analgesia") > 0)
			{
				sPlayerHP = Strings.WoundLevel(The.Player);
				Analgesic = true;
			}
			else
			{
				Analgesic = false;
				int hitpoints = PlayerBody.hitpoints;
				int baseHitpoints = PlayerBody.baseHitpoints;
				if (hitpoints != LastHP || baseHitpoints != LastHPBase)
				{
					LastHP = hitpoints;
					LastHPBase = baseHitpoints;
					sPlayerHP = XRL.World.Event.NewStringBuilder().Append("&Y").Append(hitpoints)
						.Append(" / ")
						.Append(LastHPBase)
						.ToString();
				}
			}
			int statValue = PlayerBody.GetStatValue("Level");
			int xPForLevel = Leveler.GetXPForLevel(statValue);
			CurrentXP = PlayerBody.GetStatValue("XP");
			NextXP = Leveler.GetXPForLevel(statValue + 1);
			XPBarPercentage = (float)(CurrentXP - xPForLevel) / (float)(NextXP - xPForLevel);
			CurrentHP = PlayerBody.hitpoints;
			MaxHP = PlayerBody.baseHitpoints;
			SB.Length = 0;
			return;
		}
		if (The.Player != PlayerBody)
		{
			PlayerBody = The.Player;
			if (PlayerBody == null)
			{
				return;
			}
			Update();
		}
		if (GameManager.bDraw == 25 || GameManager.bDraw == 26)
		{
			return;
		}
		int num = 0;
		if (Hidden)
		{
			if (State == "left")
			{
				_ScreenBuffer.Goto(0, 0);
			}
			if (State == "right")
			{
				_ScreenBuffer.Goto(77, 0);
			}
			_ScreenBuffer.Write("[&W/&y]");
			if (State == "left")
			{
				_ScreenBuffer.Goto(0, 1);
			}
			if (State == "right")
			{
				_ScreenBuffer.Goto(79 - sPlayerHP.LengthExceptFormatting(), 1);
			}
			_ScreenBuffer.Write(sPlayerHP);
			if (State == "left")
			{
				num = 0;
			}
			if (State == "right")
			{
				num = 67;
			}
			if (CurrentTarget != null)
			{
				string text = Strings.WoundLevel(CurrentTarget);
				_ScreenBuffer.Goto(79 - text.LengthExceptFormatting() - 1, 2);
				_ScreenBuffer.Write(text);
			}
			RenderCurrentCellPopup(_ScreenBuffer);
		}
		else
		{
			if (GameManager.bDraw == 27)
			{
				return;
			}
			int num2 = 60;
			num = 61;
			if (State == "left")
			{
				num2 = 25;
				num = 0;
			}
			else if (State == "right")
			{
				num2 = 55;
				num = 56;
			}
			_ScreenBuffer.Goto(num2, 0);
			for (int i = 0; i < 25; i++)
			{
				_ScreenBuffer.Goto(num2, i);
				_ScreenBuffer.Write(179);
			}
			if (State == "left")
			{
				_ScreenBuffer.Fill(0, 0, num2 - 1, 24, 32, ConsoleLib.Console.ColorUtility.MakeColor(TextColor.Black, TextColor.Black));
			}
			if (State == "right")
			{
				_ScreenBuffer.Fill(num2 + 1, 0, 79, 24, 32, ConsoleLib.Console.ColorUtility.MakeColor(TextColor.Black, TextColor.Black));
			}
			if (GameManager.bDraw == 28)
			{
				return;
			}
			if (SidebarState == 3)
			{
				if (XRLCore.Core.HostileWalkObjects.Count > 0)
				{
					XRLCore.Core.HostileWalkObjects.Sort(new XRLCore.SortObjectBydistanceToPlayer());
					for (int j = 0; j < XRLCore.Core.HostileWalkObjects.Count && j < 14; j++)
					{
						string text2 = XRLCore.Core.HostileWalkObjects[j].DisplayName;
						if (text2.Length > 20)
						{
							text2 = text2.Substring(0, 20);
						}
						if (State == "left")
						{
							_ScreenBuffer.Goto(0, j);
						}
						if (State == "right")
						{
							_ScreenBuffer.Goto(num2 + 1, j);
						}
						XRL.World.GameObject gameObject = XRLCore.Core.HostileWalkObjects[j];
						if (gameObject == CurrentTarget)
						{
							_ScreenBuffer.Write("^r" + text2);
						}
						else
						{
							_ScreenBuffer.Write(text2);
						}
						SB.Length = 0;
						SB.Append(gameObject.hitpoints);
						SB.Append("/");
						SB.Append(gameObject.baseHitpoints);
						SB.Append('\u0003');
						int num3 = 100;
						if (gameObject.baseHitpoints != 0)
						{
							num3 = gameObject.hitpoints * 100 / gameObject.baseHitpoints;
						}
						if (num3 < 15)
						{
							_ScreenBuffer.Write("&r");
							_ScreenBuffer.Write(SB);
						}
						else if (num3 < 33)
						{
							_ScreenBuffer.Write("&R");
							_ScreenBuffer.Write(SB);
						}
						else if (num3 < 66)
						{
							_ScreenBuffer.Write("&W");
							_ScreenBuffer.Write(SB);
						}
						else if (num3 < 100)
						{
							_ScreenBuffer.Write("&G");
							_ScreenBuffer.Write(SB);
						}
						else
						{
							_ScreenBuffer.Write("&Y");
							_ScreenBuffer.Write(SB);
						}
					}
				}
				if (XRLCore.Core.Game.Player.Messages.Cache_0_12Valid)
				{
					if (State == "left")
					{
						Text.DrawBottomToTop(_ScreenBuffer, MessageCache, 0, 16, num2, 24);
					}
					else if (State == "right")
					{
						Text.DrawBottomToTop(_ScreenBuffer, MessageCache, num2 + 1, 16, 79, 24);
					}
				}
				else
				{
					StringBuilder lines = XRLCore.Core.Game.Player.Messages.GetLines(0, 12);
					if (State == "left")
					{
						MessageCache = Text.DrawBottomToTop(_ScreenBuffer, lines, 0, 16, num2, 24);
					}
					else if (State == "right")
					{
						MessageCache = Text.DrawBottomToTop(_ScreenBuffer, lines, num2 + 1, 16, 79, 24);
					}
				}
				if (State == "left")
				{
					_ScreenBuffer.Goto(0, 14);
				}
				if (State == "right")
				{
					_ScreenBuffer.Goto(num2 + 2, 14);
				}
				_ScreenBuffer.Write(sPlayerHP);
				if (State == "left")
				{
					_ScreenBuffer.Goto(0, 15);
				}
				if (State == "right")
				{
					_ScreenBuffer.Goto(num2 + 1, 15);
				}
				if (CurrentTarget != null)
				{
					_ScreenBuffer.Goto(num + 1, 12);
					PutTargetInSB();
					_ScreenBuffer.Write(SB);
					_ScreenBuffer.Goto(num + 1, 13);
					_ScreenBuffer.Write(Strings.WoundLevel(CurrentTarget));
				}
			}
			else if (SidebarState == 2)
			{
				if (XRLCore.Core.Game.Player.Messages.Cache_0_12Valid)
				{
					if (State == "left")
					{
						Text.DrawBottomToTop(_ScreenBuffer, MessageCache, 0, 16, num2, 24);
					}
					else if (State == "right")
					{
						Text.DrawBottomToTop(_ScreenBuffer, MessageCache, num2 + 1, 16, 79, 24);
					}
				}
				else
				{
					StringBuilder lines2 = XRLCore.Core.Game.Player.Messages.GetLines(0, 12);
					if (State == "left")
					{
						MessageCache = Text.DrawBottomToTop(_ScreenBuffer, lines2, 0, 16, num2, 24);
					}
					else if (State == "right")
					{
						MessageCache = Text.DrawBottomToTop(_ScreenBuffer, lines2, num2 + 1, 16, 79, 24);
					}
				}
				if (CurrentTarget != null)
				{
					_ScreenBuffer.Goto(num + 1, 12);
					PutTargetInSB();
					_ScreenBuffer.Write(SB);
					_ScreenBuffer.Goto(num + 1, 13);
					_ScreenBuffer.Write(Strings.WoundLevel(CurrentTarget));
				}
				_ScreenBuffer.Goto(num + 15, 11);
				_ScreenBuffer.Write(sWeight);
				_ScreenBuffer.Goto(num + 1, 11);
				_ScreenBuffer.Write("HP: ");
				_ScreenBuffer.Write(sPlayerHP);
				List<ActivatedAbilityEntry> list = new List<ActivatedAbilityEntry>();
				int num4 = 0;
				ActivatedAbilities activatedAbilities = The.Player.ActivatedAbilities;
				if (activatedAbilities != null)
				{
					foreach (Guid key in activatedAbilities.AbilityByGuid.Keys)
					{
						if (num4 > 11)
						{
							break;
						}
						ActivatedAbilityEntry activatedAbilityEntry = activatedAbilities.AbilityByGuid[key];
						if (!list.CleanContains(activatedAbilityEntry))
						{
							list.Add(activatedAbilityEntry);
							_ScreenBuffer.Goto(num, num4);
							string text3 = "";
							string text4 = "y";
							if (activatedAbilityEntry.Toggleable)
							{
								text4 = (activatedAbilityEntry.ToggleState ? "g" : "r");
							}
							else if (!activatedAbilityEntry.Enabled || activatedAbilityEntry.Cooldown > 0)
							{
								text4 = "K";
							}
							text3 = ((activatedAbilityEntry.Cooldown <= 0) ? (text3 + "{{" + text4 + "|" + activatedAbilityEntry.DisplayName + "}}") : (text3 + "{{Y|({{C|" + activatedAbilityEntry.CooldownRounds + "}})-}}{{" + text4 + "|" + activatedAbilityEntry.DisplayName + "}}"));
							if (ConsoleLib.Console.ColorUtility.LengthExceptFormatting(text3) > 25)
							{
								text3 = ConsoleLib.Console.ColorUtility.ClipExceptFormatting(text3, 25);
							}
							_ScreenBuffer.Write(text3);
							num4++;
						}
					}
				}
			}
			else if (SidebarState == 1)
			{
				if (XRLCore.Core.Game.Player.Messages.Cache_0_12Valid)
				{
					if (State == "left")
					{
						Text.DrawBottomToTop(_ScreenBuffer, MessageCache, 0, 1, num2, 24);
					}
					else if (State == "right")
					{
						Text.DrawBottomToTop(_ScreenBuffer, MessageCache, num2 + 1, 1, 79, 24);
					}
				}
				else
				{
					StringBuilder lines3 = XRLCore.Core.Game.Player.Messages.GetLines(0, 12);
					if (State == "left")
					{
						MessageCache = Text.DrawBottomToTop(_ScreenBuffer, lines3, 0, 1, num2, 24);
					}
					else if (State == "right")
					{
						MessageCache = Text.DrawBottomToTop(_ScreenBuffer, lines3, num2 + 1, 1, 79, 24);
					}
				}
				if (CurrentTarget != null && CurrentTarget.HasStat("Hitpoints"))
				{
					_ScreenBuffer.Goto(num + 1, 1);
					PutTargetInSB();
					_ScreenBuffer.Write(SB);
					_ScreenBuffer.Goto(num + 1, 2);
					_ScreenBuffer.Write(Strings.WoundLevel(CurrentTarget));
				}
				_ScreenBuffer.Goto(num + 15, 0);
				_ScreenBuffer.Write(sWeight);
				_ScreenBuffer.Goto(num + 1, 0);
				_ScreenBuffer.Write("HP: " + sPlayerHP);
			}
			else if (SidebarState == 0)
			{
				if (GameManager.bDraw == 29 || GameManager.bDraw == 30)
				{
					return;
				}
				if (XRLCore.Core.Game.Player.Messages.Cache_0_12Valid)
				{
					if (State == "left")
					{
						Text.DrawBottomToTop(_ScreenBuffer, MessageCache, 0, 16, num2, 24);
					}
					else if (State == "right")
					{
						Text.DrawBottomToTop(_ScreenBuffer, MessageCache, num2 + 1, 16, 79, 24);
					}
				}
				else
				{
					StringBuilder lines4 = XRLCore.Core.Game.Player.Messages.GetLines(0, 12);
					if (State == "left")
					{
						MessageCache = Text.DrawBottomToTop(_ScreenBuffer, lines4, 0, 16, num2, 24);
					}
					else if (State == "right")
					{
						MessageCache = Text.DrawBottomToTop(_ScreenBuffer, lines4, num2 + 1, 16, 79, 24);
					}
				}
				if (GameManager.bDraw == 31)
				{
					return;
				}
				if (CurrentTarget != null && CurrentTarget.HasStat("Hitpoints"))
				{
					_ScreenBuffer.Goto(num + 1, 12);
					PutTargetInSB();
					_ScreenBuffer.Write(SB);
					_ScreenBuffer.Goto(num + 1, 13);
					_ScreenBuffer.Write(Strings.WoundLevel(CurrentTarget));
				}
				if (GameManager.bDraw == 32)
				{
					return;
				}
				_ScreenBuffer.Goto(num + 1, 0);
				if (Options.LocationIntseadOfName)
				{
					if (The.Player?.CurrentZone != null)
					{
						_ScreenBuffer.Write(StringFormat.ClipLine(WorldFactory.Factory.ZoneDisplayName(The.Player.CurrentZone.ZoneID), 22));
					}
					else
					{
						_ScreenBuffer.Write("unknown");
					}
				}
				else if (The.Player.HasEffect<Dominated>())
				{
					if (("Dom: " + The.Player.DisplayName).Length < 24)
					{
						_ScreenBuffer.Write("Dom: " + The.Player.DisplayName);
					}
					else
					{
						_ScreenBuffer.Write(("Dom: " + The.Player.DisplayName).Substring(0, 24));
					}
				}
				else
				{
					_ScreenBuffer.Write(XRLCore.Core.Game.PlayerName);
				}
				if (GameManager.bDraw == 33)
				{
					return;
				}
				_ScreenBuffer.Goto(num + 1, 1);
				StringBuilder stringBuilder = XRL.World.Event.NewStringBuilder();
				stringBuilder.Append("&YST&y: &C").Append(PlayerBody.Statistics["Strength"].Value);
				_ScreenBuffer.Write(stringBuilder);
				_ScreenBuffer.Goto(num + 9, 1);
				stringBuilder.Length = 0;
				stringBuilder.Append("&WAG&y: &C").Append(PlayerBody.Statistics["Agility"].Value);
				_ScreenBuffer.Write(stringBuilder);
				_ScreenBuffer.Goto(num + 17, 1);
				int num5 = PlayerBody.Speed;
				if (PlayerBody.Physics.Temperature < PlayerBody.Physics.FreezeTemperature && PlayerBody.Physics.Temperature > PlayerBody.Physics.BrittleTemperature)
				{
					num5 -= (int)(100.0 * (0.5 - (double)(Math.Abs((float)(PlayerBody.Physics.Temperature - PlayerBody.Physics.BrittleTemperature) / (float)(PlayerBody.Physics.FreezeTemperature - PlayerBody.Physics.BrittleTemperature)) * 0.5f)));
				}
				_ScreenBuffer.Write(XRL.World.Event.NewStringBuilder().AppendFormat("QN: {0}", num5));
				_ScreenBuffer.Goto(num + 17, 2);
				_ScreenBuffer.Write(XRL.World.Event.NewStringBuilder().AppendFormat("MS: {0}", 100 - PlayerBody.Statistics["MoveSpeed"].Value + 100));
				_ScreenBuffer.Goto(num + 1, 2);
				_ScreenBuffer.Write(XRL.World.Event.NewStringBuilder().AppendFormat("&RTO&y: &C{0}", PlayerBody.Statistics["Toughness"].Value));
				_ScreenBuffer.Goto(num + 9, 2);
				_ScreenBuffer.Write(XRL.World.Event.NewStringBuilder().AppendFormat("&GWI&y: &C{0}", PlayerBody.Statistics["Willpower"].Value));
				_ScreenBuffer.Goto(num + 17, 3);
				_ScreenBuffer.Write(XRL.World.Event.NewStringBuilder().AppendFormat("T: {0}", PlayerBody.Physics.Temperature));
				_ScreenBuffer.Goto(num + 1, 3);
				_ScreenBuffer.Write(XRL.World.Event.NewStringBuilder().AppendFormat("&BIN&y: &C{0}", PlayerBody.Statistics["Intelligence"].Value));
				_ScreenBuffer.Goto(num + 9, 3);
				_ScreenBuffer.Write(XRL.World.Event.NewStringBuilder().AppendFormat("&MEG&y: &C{0}", PlayerBody.Statistics["Ego"].Value));
				_ScreenBuffer.Goto(num + 1, 4);
				_ScreenBuffer.Write(XRL.World.Event.NewStringBuilder().AppendFormat("AV: {0}", PlayerBody.Statistics["AV"].Value));
				_ScreenBuffer.Goto(num + 9, 4);
				_ScreenBuffer.Write(XRL.World.Event.NewStringBuilder().AppendFormat("DV: {0}", Stats.GetCombatDV(PlayerBody)));
				_ScreenBuffer.Goto(num + 17, 4);
				_ScreenBuffer.Write(XRL.World.Event.NewStringBuilder().AppendFormat("MA: {0}", Stats.GetCombatMA(PlayerBody)));
				if (GameManager.bDraw == 34)
				{
					return;
				}
				_ScreenBuffer.Goto(num + 1, 5);
				_ScreenBuffer.Write(XRL.World.Event.NewStringBuilder().AppendFormat("XP: {0}&K / {1}", PlayerBody.Statistics["XP"].Value, Leveler.GetXPForLevel(PlayerBody.Statistics["Level"].Value + 1)));
				Stomach part = PlayerBody.GetPart<Stomach>();
				if (part != null)
				{
					_ScreenBuffer.Goto(num + 1, 6);
					_ScreenBuffer.Write(part.WaterStatus());
					if (PlayerBody.Physics.CurrentCell != null && !PlayerBody.Physics.CurrentCell.ParentZone.IsWorldMap())
					{
						_ScreenBuffer.Write("&y,");
						_ScreenBuffer.Write(part.FoodStatus());
					}
				}
				_ScreenBuffer.Goto(num + 15, 7);
				_ScreenBuffer.Write(sWeight);
				_ScreenBuffer.Goto(num + 1, 7);
				_ScreenBuffer.Write("HP: ");
				_ScreenBuffer.Write(sPlayerHP);
				_ScreenBuffer.Goto(num + 1, 9);
				_ScreenBuffer.Write(Calendar.GetDay());
				_ScreenBuffer.Write(" of ");
				_ScreenBuffer.Write(Calendar.GetMonth());
				_ScreenBuffer.Goto(num + 1, 10);
				_ScreenBuffer.Write(Calendar.GetTime());
				RenderMissleStatus(PlayerBody, num + 1, 11, _ScreenBuffer);
				if (GameManager.bDraw == 35)
				{
					return;
				}
			}
			if (Options.GetOption("OptionShowSidebarAbilities") == "Yes")
			{
				RenderAbilityStatus(PlayerBody, num2, 3, _ScreenBuffer);
			}
			_ScreenBuffer.Goto(num2, 19);
			_ScreenBuffer.Write(193);
			_ScreenBuffer.Goto(num2, 20);
			_ScreenBuffer.Write("&W*&y");
			_ScreenBuffer.Goto(num2, 21);
			_ScreenBuffer.Write("&W/&y");
			_ScreenBuffer.Goto(num2, 22);
			_ScreenBuffer.Write(194);
			if (GameManager.bDraw != 36)
			{
				RenderCurrentCellPopup(_ScreenBuffer);
				_ = GameManager.bDraw;
				_ = 37;
			}
		}
	}

	private static void RenderAbilityStatus(XRL.World.GameObject Player, int xp, int yp, ScreenBuffer _Buffer)
	{
		if (!Player.HasPart("ActivatedAbilities"))
		{
			return;
		}
		ActivatedAbilities part = Player.GetPart<ActivatedAbilities>();
		if (part.AbilityByGuid == null || part.AbilityByGuid.Count <= 0)
		{
			return;
		}
		int num = yp;
		_Buffer.Goto(xp, num);
		_Buffer.Write("Á");
		foreach (ActivatedAbilityEntry item in part.GetAbilityListOrderedByPreference())
		{
			num++;
			_Buffer.Goto(xp, num);
			_Buffer.Write(item.GetUITile());
		}
		num++;
		_Buffer.Goto(xp, num);
		_Buffer.Write("Â");
	}

	private static void RenderMissleStatus(XRL.World.GameObject Player, int xp, int yp, ScreenBuffer _Buffer)
	{
		List<XRL.World.GameObject> missileWeapons = Player.GetMissileWeapons();
		if (missileWeapons == null || missileWeapons.Count <= 0)
		{
			return;
		}
		int num = 0;
		while (num < missileWeapons.Count && num < 4)
		{
			MissileWeapon part = missileWeapons[num].GetPart<MissileWeapon>();
			if (part != null)
			{
				string text = part.Status();
				if (ConsoleLib.Console.ColorUtility.LengthExceptFormatting(text) > 24)
				{
					text = ConsoleLib.Console.ColorUtility.ClipExceptFormatting(text, 24);
				}
				_Buffer.WriteAt(xp, yp, text);
				yp++;
				num++;
			}
		}
	}

	private static void RenderCurrentCellPopup(ScreenBuffer _Buffer)
	{
		if (Options.GetOption("OptionShowCurrentCellPopup") != "Yes")
		{
			return;
		}
		XRL.World.GameObject Player = The.Player;
		if (Player.Physics.CurrentCell == null)
		{
			return;
		}
		Objects.Clear();
		foreach (XRL.World.GameObject key in AutogotItems.Keys)
		{
			Objects.Add(AutogotItems[key]);
		}
		foreach (XRL.World.GameObject item in Player.CurrentCell.LoopObjectsWithPart("Physics", (XRL.World.GameObject GO) => GO != Player && GO.IsTakeable()))
		{
			Objects.Add(item.ShortDisplayName);
		}
		List<string> list = new List<string>(Objects.Count);
		int num = 0;
		int num2 = 0;
		foreach (string @object in Objects)
		{
			num2++;
			if (num2 == 10)
			{
				list.Add("<more...>");
				break;
			}
			list.Add(@object + "\n");
			string text = ConsoleLib.Console.ColorUtility.StripFormatting(@object);
			if (text.Length > num)
			{
				num = text.Length;
			}
		}
		num++;
		int num3;
		int num4;
		if ((Hidden && State == "left") || (!Hidden && State == "right"))
		{
			num3 = 0;
			num4 = ((VState == "bottom") ? (24 - Objects.Count - 1) : (Hidden ? 3 : 0));
		}
		else
		{
			num3 = 80 - num - 3;
			num4 = ((VState == "bottom") ? (24 - Objects.Count - 1) : (Hidden ? 3 : 0));
		}
		if (num > 0 && Objects.Count > 0)
		{
			_Buffer.Fill(num3, num4, num3 + num + 2, num4 + list.Count + 1, 32, ConsoleLib.Console.ColorUtility.MakeColor(TextColor.Black, TextColor.Black));
			_Buffer.ThickSingleBox(num3, num4, num3 + num + 2, num4 + list.Count + 1, ConsoleLib.Console.ColorUtility.MakeColor(ConsoleLib.Console.ColorUtility.Bright(TextColor.Black), TextColor.Black));
		}
		int num5 = 0;
		foreach (string item2 in list)
		{
			_Buffer.Goto(num3 + 2, num4 + 1 + num5);
			_Buffer.Write(item2);
			num5++;
		}
	}
}
