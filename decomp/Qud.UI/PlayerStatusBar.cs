using System;
using System.Collections.Generic;
using System.Text;
using ConsoleLib.Console;
using Kobold;
using UnityEngine;
using UnityEngine.UI;
using XRL;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World;
using XRL.World.Capabilities;
using XRL.World.Parts;

namespace Qud.UI;

[ExecuteAlways]
[UIView("PlayerStatusBar", false, false, false, null, null, false, 0, false, NavCategory = "Adventure", UICanvas = "PlayerStatusBar", UICanvasHost = 1)]
public class PlayerStatusBar : SingletonWindowBase<PlayerStatusBar>
{
	private struct BarValue
	{
		public int Min;

		public int Max;

		public int Value;

		public void SetBar(HPBar bar)
		{
			bar.BarStart = Min;
			bar.BarEnd = Max;
			bar.BarValue = Value;
			bar.WantsUpdate = true;
		}
	}

	private enum BoolDataType
	{
		AutoExploreActive,
		ObjectFinderActive,
		MinimapActive,
		WindowsPinned
	}

	private enum StringDataType
	{
		FoodWater,
		Time,
		Temp,
		Weight,
		Zone,
		HPBar,
		PlayerName,
		ZoneOnly
	}

	public UIThreeColorProperties PlayerBody3c;

	public UnityEngine.GameObject TopLeftSecondaryStatBlock;

	public UnityEngine.GameObject TimeText;

	public UITextSkin FoodStatusText;

	public UITextSkin TempText;

	public UITextSkin WeightText;

	public UITextSkin ZoneText;

	public UITextSkin PlayerNameText;

	public Image TimeClockImage;

	public List<Sprite> QudTimeImages;

	public ActiveButton WindowPinButton;

	public ActiveButton ExploreButton;

	public ActiveButton FindButton;

	public ActiveButton MinimapButton;

	public List<UnityEngine.GameObject> Spacers;

	public HPBar HPBar;

	public HPBar XPBar;

	private StringBuilder SB = new StringBuilder();

	private ConsoleChar playerChar;

	private ConsoleChar lastPlayerChar;

	private bool playerCharDirty;

	private Dictionary<string, string> playerStats = new Dictionary<string, string>
	{
		{ "Level", "1" },
		{ "XP", "0" }
	};

	private Dictionary<string, string> lastPlayerStats = new Dictionary<string, string>();

	private bool playerStatsDirty;

	private object barLock = new object();

	private BarValue hpbarValue;

	private bool barValueDirty;

	private object boolLock = new object();

	private Dictionary<BoolDataType, bool> boolData = new Dictionary<BoolDataType, bool>
	{
		{
			BoolDataType.ObjectFinderActive,
			false
		},
		{
			BoolDataType.AutoExploreActive,
			false
		},
		{
			BoolDataType.MinimapActive,
			false
		},
		{
			BoolDataType.WindowsPinned,
			false
		}
	};

	private Dictionary<BoolDataType, bool> lastBoolData = new Dictionary<BoolDataType, bool>();

	private bool boolDataDirty;

	private string customClock;

	private string lastCustomClock;

	private int timeOfDayImage = -2;

	private int lastTimeOfDayImage = -2;

	private bool timeOfDayImageDirty;

	private object timeLock = new object();

	private Dictionary<StringDataType, string> playerStringData = new Dictionary<StringDataType, string> { 
	{
		StringDataType.FoodWater,
		""
	} };

	private Dictionary<StringDataType, string> lastPlayerStringData = new Dictionary<StringDataType, string>();

	private bool playerStringsDirty;

	private bool playerStringsSizeDirty;

	private StringBuilder sb = new StringBuilder(1024);

	private StringBuilder UpdateSB = new StringBuilder();

	private float LastWidth;

	private bool RelayoutTop = true;

	private Dictionary<string, Sprite> customClocks = new Dictionary<string, Sprite>();

	public void Start()
	{
		playerChar = new ConsoleChar();
		lastPlayerChar = new ConsoleChar();
	}

	private void UpdateStat(string statName, Statistic stat, XRL.World.GameObject Player)
	{
		string text = stat.GetDisplayValue();
		if (stat.Name == "DV")
		{
			text = Stats.GetCombatDV(Player).ToStringCached();
		}
		else if (stat.Name == "MA")
		{
			text = Stats.GetCombatMA(Player).ToStringCached();
		}
		if (lastPlayerStats.TryGetValue(statName, out var value) && value == text)
		{
			return;
		}
		lock (playerStats)
		{
			Dictionary<string, string> dictionary = lastPlayerStats;
			string value2 = (playerStats[statName] = text);
			dictionary[statName] = value2;
			playerStatsDirty = true;
		}
	}

	private void UpdateBool(BoolDataType dataType, bool value)
	{
		if (lastBoolData.TryGetValue(dataType, out var value2) && value2 == value)
		{
			return;
		}
		lock (boolLock)
		{
			boolData[dataType] = value;
			lastBoolData[dataType] = value;
			boolDataDirty = true;
		}
	}

	private bool Compare(StringBuilder s1, string s2)
	{
		if (s1.Length != s2.Length)
		{
			return false;
		}
		for (int i = 0; i < s1.Length; i++)
		{
			if (s1[i] != s2[i])
			{
				return false;
			}
		}
		return true;
	}

	private void UpdateString(StringDataType type, StringBuilder data, bool toRTF = false)
	{
		if (lastPlayerStringData.TryGetValue(type, out var value) && Compare(data, value))
		{
			return;
		}
		lock (playerStringData)
		{
			lastPlayerStringData[type] = data.ToString();
			playerStringData[type] = data.ToString();
			playerStringsDirty = true;
			if ((lastPlayerStringData[type]?.Length ?? 0) != data.ToString().Length)
			{
				playerStringsSizeDirty = true;
			}
		}
	}

	private void UpdateString(StringDataType type, string data, bool toRTF = false)
	{
		if (lastPlayerStringData.TryGetValue(type, out var value) && value == data)
		{
			return;
		}
		lock (playerStringData)
		{
			lastPlayerStringData[type] = data;
			playerStringData[type] = data;
			playerStringsDirty = true;
			if ((lastPlayerStringData[type]?.Length ?? 0) != data.Length)
			{
				playerStringsSizeDirty = true;
			}
		}
	}

	public override void Init()
	{
		XRLCore.RegisterAfterRenderCallback(AfterRender);
		XRLCore.RegisterOnBeginPlayerTurnCallback(BeginEndTurn);
		XRLCore.RegisterOnEndPlayerTurnCallback(BeginEndTurn, Single: true);
		XRLCore.RegisterOnPassedTenPlayerTurnCallback(BeginEndTurn);
	}

	private void AfterRender(XRLCore core, ScreenBuffer buffer)
	{
		XRL.World.GameObject player = The.Player;
		if (player != null && player.Render.Visible)
		{
			Cell currentCell = player.GetCurrentCell();
			if (currentCell != null)
			{
				ConsoleChar consoleChar = buffer[currentCell.X, currentCell.Y];
				if (consoleChar != lastPlayerChar && playerChar != null)
				{
					lock (playerChar)
					{
						playerChar.Copy(consoleChar);
						lastPlayerChar.Copy(consoleChar);
						playerCharDirty = true;
					}
				}
			}
		}
		BeginEndTurn(core);
	}

	public void Awake()
	{
		LastWidth = 0f;
		RelayoutTop = true;
	}

	private void BeginEndTurn(XRLCore core)
	{
		XRL.World.GameObject player = The.Player;
		if (player == null)
		{
			return;
		}
		UpdateBool(BoolDataType.AutoExploreActive, AutoAct.IsAnyExploration());
		Cell currentCell = player.GetCurrentCell();
		if (currentCell != null && currentCell.ParentZone != null)
		{
			UpdateString(StringDataType.Zone, currentCell.ParentZone.DisplayName);
			UpdateString(StringDataType.ZoneOnly, currentCell.ParentZone.DisplayName);
		}
		foreach (KeyValuePair<string, Statistic> statistic in player.Statistics)
		{
			UpdateStat(statistic.Key, statistic.Value, player);
		}
		UpdateString(StringDataType.PlayerName, player.DisplayNameOnlyDirect);
		Stomach part = player.GetPart<Stomach>();
		if (part != null)
		{
			sb.Length = 0;
			sb.Append(part.FoodStatus()).Append(" ").Append(part.WaterStatus());
			UpdateString(StringDataType.FoodWater, sb);
		}
		Inventory inventory = player.Inventory;
		Body body = player.Body;
		if (inventory != null && body != null)
		{
			int carriedWeight = player.GetCarriedWeight();
			int maxCarriedWeight = player.GetMaxCarriedWeight();
			int freeDrams = player.GetFreeDrams();
			sb.Length = 0;
			sb.Append(carriedWeight).Append("/").Append(maxCarriedWeight.ToStringCached())
				.Append("# {{blue|")
				.Append(freeDrams.ToStringCached())
				.Append("$}}");
			UpdateString(StringDataType.Weight, sb);
		}
		sb.Length = 0;
		sb.Append("T:").Append(player.Physics.Temperature.ToStringCached()).Append('ø');
		UpdateString(StringDataType.Temp, sb);
		sb.Length = 0;
		sb.Append(Calendar.GetTime()).Append(" ").Append(Calendar.GetDay())
			.Append(" of ")
			.Append(Calendar.GetMonth());
		UpdateString(StringDataType.Time, sb);
		int num = -1;
		string text = player.GetCurrentZone()?.ResolveZoneWorld();
		if (text == "JoppaWorld")
		{
			int num2 = Calendar.CurrentDaySegment / 10;
			num2 += 875;
			num2 %= 1200;
			int num3 = 675;
			if (num2 < num3)
			{
				num = num2 * 7 / num3;
			}
			else
			{
				num2 -= num3;
				num3 = 525;
				num = 7 + num2 * 3 / num3;
			}
		}
		customClock = ((text == null) ? null : WorldFactory.Factory?.getWorld(text)?.CustomClock);
		if (num != lastTimeOfDayImage || customClock != lastCustomClock)
		{
			lock (timeLock)
			{
				lastCustomClock = customClock;
				lastTimeOfDayImage = (timeOfDayImage = num);
				timeOfDayImageDirty = true;
			}
		}
		if (player.GetIntProperty("Analgesia") > 0)
		{
			sb.Length = 0;
			sb.Append("HP: ").Append(Strings.WoundLevel(player));
			UpdateString(StringDataType.HPBar, sb);
			int num4 = Math.Max(0, Strings.WoundLevelN(player));
			if (hpbarValue.Value != num4 || hpbarValue.Max != 5)
			{
				lock (barLock)
				{
					hpbarValue.Min = 0;
					hpbarValue.Value = num4;
					hpbarValue.Max = 5;
					barValueDirty = true;
					return;
				}
			}
			return;
		}
		sb.Length = 0;
		sb.Append("{{Y|HP: {{").Append(Strings.HealthStatusColor(player)).Append("|")
			.Append(player.hitpoints.ToStringCached())
			.Append("}} / ")
			.Append(player.baseHitpoints.ToStringCached())
			.Append("}}");
		UpdateString(StringDataType.HPBar, sb);
		if (hpbarValue.Value != player.hitpoints || hpbarValue.Max != player.baseHitpoints)
		{
			lock (barLock)
			{
				hpbarValue.Value = player.hitpoints;
				hpbarValue.Max = player.baseHitpoints;
				barValueDirty = true;
			}
		}
	}

	public override bool AllowPassthroughInput()
	{
		return true;
	}

	public void Update()
	{
		if (playerCharDirty)
		{
			lock (playerChar)
			{
				PlayerBody3c.FromRenderable(The.Player?.RenderForUI("PlayerStatus"));
				if (PlayerBody3c.Background == The.Color.DarkBlack)
				{
					PlayerBody3c.Background = Color.clear;
				}
				playerCharDirty = false;
			}
		}
		if (playerStatsDirty)
		{
			lock (playerStats)
			{
				TopLeftSecondaryStatBlock.GetComponent<StatusBarStatBlock>().UpdateStats(playerStats);
				XPBar.text.SetText(string.Format("LVL: {0} Exp: {1} / {2}", playerStats["Level"], playerStats["XP"], Leveler.GetXPForLevel(Convert.ToInt32(playerStats["Level"]) + 1)));
				XPBar.BarStart = Leveler.GetXPForLevel(Convert.ToInt32(playerStats["Level"]));
				XPBar.BarEnd = Leveler.GetXPForLevel(Convert.ToInt32(playerStats["Level"]) + 1);
				XPBar.BarValue = Convert.ToInt32(playerStats["XP"]);
				XPBar.UpdateBar();
				playerStatsDirty = false;
			}
		}
		if (timeOfDayImageDirty)
		{
			lock (timeLock)
			{
				if (!customClock.IsNullOrEmpty())
				{
					if (!customClocks.TryGetValue(customClock, out var value))
					{
						value = SpriteManager.GetUnitySprite(customClock);
					}
					TimeClockImage.sprite = value;
				}
				else if (timeOfDayImage >= 0)
				{
					TimeClockImage.sprite = QudTimeImages[timeOfDayImage];
				}
				else
				{
					TimeClockImage.sprite = null;
				}
				timeOfDayImageDirty = false;
			}
		}
		if (Application.isPlaying)
		{
			UpdateBool(BoolDataType.ObjectFinderActive, UIManager.getWindow("NearbyItems").Visible);
			UpdateBool(BoolDataType.MinimapActive, UIManager.getWindow("Minimap").Visible);
			UpdateBool(BoolDataType.WindowsPinned, UIManager.WindowFramePin == 0);
		}
		if (boolDataDirty)
		{
			lock (boolLock)
			{
				ExploreButton.IsActive = boolData[BoolDataType.AutoExploreActive];
				FindButton.IsActive = boolData[BoolDataType.ObjectFinderActive];
				MinimapButton.IsActive = boolData[BoolDataType.MinimapActive];
				WindowPinButton.IsActive = boolData[BoolDataType.WindowsPinned];
				boolDataDirty = false;
			}
		}
		if (playerStringsDirty)
		{
			lock (playerStringData)
			{
				if (FoodStatusText.text != playerStringData[StringDataType.FoodWater])
				{
					FoodStatusText.SetText(playerStringData[StringDataType.FoodWater]);
				}
				UITextSkin component = TimeText.GetComponent<UITextSkin>();
				if (playerStringData.ContainsKey(StringDataType.Time))
				{
					if (component.text != playerStringData[StringDataType.Time])
					{
						component.SetText(playerStringData[StringDataType.Time]);
					}
					TimeText.GetComponent<LayoutElement>().preferredWidth = Math.Min(350f, component.preferredWidth + 5f);
				}
				if (playerStringData.ContainsKey(StringDataType.Temp) && TempText.text != playerStringData[StringDataType.Temp])
				{
					TempText.SetText(playerStringData[StringDataType.Temp]);
				}
				if (playerStringData.ContainsKey(StringDataType.Weight) && WeightText.text != playerStringData[StringDataType.Weight])
				{
					WeightText.SetText(playerStringData[StringDataType.Weight]);
				}
				if (playerStringData.ContainsKey(StringDataType.Zone) && ZoneText.text != playerStringData[StringDataType.Zone])
				{
					ZoneText.SetText(playerStringData[StringDataType.Zone]);
					GameManager.Instance.minimapLabel?.SetText(" " + playerStringData[StringDataType.ZoneOnly]);
				}
				if (playerStringData.ContainsKey(StringDataType.HPBar) && HPBar.text.text != playerStringData[StringDataType.HPBar])
				{
					HPBar.text.SetText(playerStringData[StringDataType.HPBar]);
				}
				if (playerStringData.ContainsKey(StringDataType.PlayerName) && PlayerNameText.text != playerStringData[StringDataType.PlayerName])
				{
					PlayerNameText.SetText(playerStringData[StringDataType.PlayerName]);
				}
				playerStringsDirty = false;
			}
		}
		if (playerStringsSizeDirty)
		{
			lock (playerStringData)
			{
				RelayoutTop = true;
				playerStringsSizeDirty = false;
			}
		}
		if (barValueDirty)
		{
			barValueDirty = false;
			hpbarValue.SetBar(HPBar);
		}
		HandleSizeUpdate();
	}

	public void HandleSizeUpdate()
	{
		if (RelayoutTop)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(topRectTransform);
		}
		if (LastWidth != rectTransform.rect.width)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(bottomRectTransform);
		}
		if (LastWidth != rectTransform.rect.width || RelayoutTop)
		{
			if (!Spacers[0].activeSelf)
			{
				Spacers[0].SetActive(value: true);
			}
			if (!PlayerNameText.gameObject.activeSelf)
			{
				PlayerNameText.gameObject.SetActive(value: true);
			}
			if (!Spacers[1].activeSelf)
			{
				Spacers[1].SetActive(value: true);
			}
			if (!TimeText.gameObject.activeSelf)
			{
				TimeText.gameObject.SetActive(value: true);
			}
			if (!Spacers[2].activeSelf)
			{
				Spacers[2].SetActive(value: true);
			}
			if (!TopLeftSecondaryStatBlock.gameObject.activeSelf)
			{
				TopLeftSecondaryStatBlock.gameObject.SetActive(value: true);
			}
			if (!Spacers[3].activeSelf)
			{
				Spacers[3].SetActive(value: true);
			}
			if (!ZoneText.gameObject.activeSelf)
			{
				ZoneText.gameObject.SetActive(value: true);
			}
			LayoutRebuilder.ForceRebuildLayoutImmediate(topRectTransform);
			if (Spacers[0].activeInHierarchy && Spacers[0].GetComponent<RectTransform>().sizeDelta.x < 15f)
			{
				Spacers[0].SetActive(value: false);
				PlayerNameText.gameObject.SetActive(value: false);
				LayoutRebuilder.ForceRebuildLayoutImmediate(topRectTransform);
			}
			if (Spacers[2].activeInHierarchy && Spacers[2].GetComponent<RectTransform>().sizeDelta.x < 15f)
			{
				Spacers[2].SetActive(value: false);
				TimeText.gameObject.SetActive(value: false);
				LayoutRebuilder.ForceRebuildLayoutImmediate(topRectTransform);
			}
			if (Spacers[3].activeInHierarchy && Spacers[1].activeInHierarchy && Spacers[1].GetComponent<RectTransform>().sizeDelta.x < 15f)
			{
				Spacers[3].SetActive(value: false);
				ZoneText.gameObject.SetActive(value: false);
				LayoutRebuilder.ForceRebuildLayoutImmediate(topRectTransform);
			}
			if (Spacers[1].activeInHierarchy && Spacers[1].GetComponent<RectTransform>().sizeDelta.x < 15f)
			{
				Spacers[1].SetActive(value: false);
				TopLeftSecondaryStatBlock.gameObject.SetActive(value: false);
				LayoutRebuilder.ForceRebuildLayoutImmediate(topRectTransform);
			}
		}
		LastWidth = rectTransform.rect.width;
		RelayoutTop = false;
	}

	public void ClickButton(string button)
	{
		switch (button)
		{
		case "Explore":
			Keyboard.PushMouseEvent("Command:CmdAutoExplore");
			break;
		case "Up":
			Keyboard.PushMouseEvent("Command:CmdMoveU");
			break;
		case "Down":
			Keyboard.PushMouseEvent("Command:CmdMoveD");
			break;
		case "Char":
			Keyboard.PushMouseEvent("Command:CmdCharacter");
			break;
		case "Rest":
			Keyboard.PushMouseEvent("Command:CmdWaitMenu");
			break;
		case "Look":
			Keyboard.PushMouseEvent("Command:CmdLook");
			break;
		case "SystemMenu":
			Keyboard.PushMouseEvent("Command:CmdSystemMenu");
			break;
		case "POI":
			Keyboard.PushMouseEvent("Command:CmdMoveToPointOfInterest");
			break;
		case "Finder":
			UIManager.getWindow<NearbyItemsWindow>("NearbyItems").TogglePreferredState();
			break;
		case "Minimap":
			UIManager.getWindow<MinimapWindow>("Minimap").TogglePreferredState();
			break;
		case "WindowLock":
			if (UIManager.WindowFramePin == 1)
			{
				UIManager.WindowFramePin = 0;
			}
			else
			{
				UIManager.WindowFramePin = 1;
			}
			break;
		}
	}
}
