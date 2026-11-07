using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleLib.Console;
using XRL.Collections;
using XRL.UI;
using XRL.World.Text;

namespace XRL.World.Parts;

[Serializable]
public class ActivatedAbilities : IPart
{
	[HasModSensitiveStaticCache]
	public static class XmlData
	{
		public enum UITileStates
		{
			Default,
			ToggleOn,
			CoolingDown,
			Disabled
		}

		public class Data
		{
			public string Command;

			public Dictionary<UITileStates, Renderable> UITiles = new Dictionary<UITileStates, Renderable>();

			public Dictionary<string, string> Tags;

			public Templates.Template Description
			{
				get
				{
					if (!Templates.TemplateByID.TryGetValue("ActivatedAbility." + Command, out var value))
					{
						return null;
					}
					return value;
				}
			}

			public bool TryGetTag(string Name, out string Value)
			{
				if (Tags == null)
				{
					Value = null;
					return false;
				}
				return Tags.TryGetValue(Name, out Value);
			}

			public bool TryAddTag(string Name, string Value)
			{
				if (Tags == null)
				{
					Tags = new Dictionary<string, string>();
				}
				return Tags.TryAdd(Name, Value);
			}

			public bool HasTag(string Name)
			{
				if (Tags != null)
				{
					return Tags.ContainsKey(Name);
				}
				return false;
			}
		}

		[ModSensitiveStaticCache(true)]
		public static Dictionary<string, Data> DataByCommand = new Dictionary<string, Data>();

		public static Dictionary<string, Action<XmlDataHelper>> XmlNodes = new Dictionary<string, Action<XmlDataHelper>>
		{
			{ "activatedabilities", HandleNodes },
			{ "ability", HandleAbilityNode }
		};

		public static Data currentData;

		public static Dictionary<string, Action<XmlDataHelper>> AbilityNodes = new Dictionary<string, Action<XmlDataHelper>>
		{
			{ "description", HandleDescriptionNode },
			{ "tag", HandleTagNode },
			{ "UITile", HandleUITileNode }
		};

		public static Data DataForCommand(string command)
		{
			if (!DataByCommand.TryGetValue(command, out var value))
			{
				return new Data
				{
					Command = command
				};
			}
			return value;
		}

		[ModSensitiveCacheInit]
		public static void Init()
		{
			foreach (XmlDataHelper item in DataManager.YieldXMLStreamsWithRoot("activatedabilities"))
			{
				HandleNodes(item);
			}
		}

		public static void HandleNodes(XmlDataHelper xml)
		{
			xml.HandleNodes(XmlNodes);
		}

		public static void HandleAbilityNode(XmlDataHelper xml)
		{
			string text = xml.ParseAttribute<string>("Command", null, required: true);
			if (!DataByCommand.TryGetValue(text, out var value))
			{
				value = new Data
				{
					Command = text
				};
				DataByCommand.Add(text, value);
			}
			currentData = value;
			xml.HandleNodes(AbilityNodes);
		}

		public static void HandleDescriptionNode(XmlDataHelper xml)
		{
			Templates.LoadTemplateFromExternal("ActivatedAbility." + currentData.Command, xml);
		}

		public static void HandleTagNode(XmlDataHelper xml)
		{
			string text = xml.ParseAttribute("Name", "");
			string value = xml.ParseAttribute("Value", "");
			if (!xml.IsEmptyElement)
			{
				xml.Read();
				string value2 = xml.Value;
				if (text.IsNullOrEmpty())
				{
					text = value2;
				}
				else if (value.IsNullOrEmpty())
				{
					value = value2;
				}
				xml.Read();
			}
			currentData.TryAddTag(text, value);
		}

		public static UITileStates ParseTileState(string state)
		{
			return state switch
			{
				"Default" => UITileStates.Default, 
				"CoolingDown" => UITileStates.CoolingDown, 
				"ToggleOn" => UITileStates.ToggleOn, 
				"Disabled" => UITileStates.Disabled, 
				_ => throw new Exception("Unable to parse UITile State \"" + state + "\""), 
			};
		}

		public static void HandleUITileNode(XmlDataHelper xml)
		{
			UITileStates key = xml.ParseAttribute("State", UITileStates.Default, required: false, ParseTileState);
			Renderable value = Renderable.UITile(xml.ParseAttribute("Tile", "", required: true), xml.ParseAttribute("Foreground", 'w'), xml.ParseAttribute("Detail", 'W'));
			currentData.UITiles[key] = value;
		}
	}

	public const string DEFAULT_ICON = "\a";

	[NonSerialized]
	public Dictionary<Guid, ActivatedAbilityEntry> AbilityByGuid;

	[NonSerialized]
	public List<CommandCooldown> Cooldowns;

	[NonSerialized]
	public bool Silent;

	[NonSerialized]
	private static List<string> _PreferenceOrder = null;

	private static StringMap<ActivatedAbilityEntry> _abilityByCommand = new StringMap<ActivatedAbilityEntry>();

	[NonSerialized]
	private List<ActivatedAbilityEntry> _AbilityListOrderedByPreferenceCache;

	public override int Priority => 90000;

	public static IEnumerable<string> PreferenceOrder
	{
		get
		{
			if (_PreferenceOrder == null)
			{
				string option = Options.GetOption("AbilityListOrder");
				if (!string.IsNullOrEmpty(option))
				{
					_PreferenceOrder = option.Split(",").ToList();
				}
				else
				{
					_PreferenceOrder = new List<string> { "CommandToggleRunning", "CommandSurvivalCamp" };
				}
			}
			return _PreferenceOrder;
		}
		set
		{
			List<string> list = new List<string>(value);
			HashSet<string> hashSet = new HashSet<string>(list);
			int num = 0;
			for (int i = 0; i < _PreferenceOrder.Count; i++)
			{
				if (hashSet.Contains(_PreferenceOrder[i]))
				{
					hashSet.Remove(_PreferenceOrder[i]);
					_PreferenceOrder[i] = list[num++];
				}
			}
			while (num < list.Count)
			{
				_PreferenceOrder.Add(list[num++]);
			}
			string value2 = _PreferenceOrder.Where((string cmd) => !Guid.TryParse(cmd, out var _)).Distinct().Aggregate("", (string a, string b) => (!string.IsNullOrEmpty(a)) ? (a + "," + b) : b);
			Options.SetOption("AbilityListOrder", value2);
			ClearCache();
		}
	}

	public override void Attach()
	{
		ParentObject.Abilities = this;
	}

	public override void Remove()
	{
		if (ParentObject?.Abilities == this)
		{
			ParentObject.Abilities = null;
			if (ParentObject.IsPlayer())
			{
				AbilityManager.RefreshPlayerAbilities();
			}
		}
	}

	public static int MinimumValueForCooldown(int Cooldown)
	{
		return Math.Max((int)Math.Round((double)Cooldown * 0.2, MidpointRounding.AwayFromZero), Math.Min(50, Cooldown));
	}

	public int GetAbilityCount()
	{
		if (AbilityByGuid != null)
		{
			return AbilityByGuid.Count;
		}
		return 0;
	}

	public ActivatedAbilityEntry GetAbilityByCommand(string command)
	{
		if (AbilityByGuid != null)
		{
			foreach (KeyValuePair<Guid, ActivatedAbilityEntry> item in AbilityByGuid)
			{
				if (item.Value.Command == command)
				{
					return item.Value;
				}
			}
		}
		return null;
	}

	public ActivatedAbilityEntry GetAbility(Guid ID)
	{
		if (AbilityByGuid != null && AbilityByGuid.TryGetValue(ID, out var value))
		{
			return value;
		}
		return null;
	}

	public override void Write(GameObject Basis, SerializationWriter Writer)
	{
		if (AbilityByGuid.IsNullOrEmpty())
		{
			Writer.WriteOptimized(0);
		}
		else
		{
			Writer.WriteOptimized(AbilityByGuid.Count);
			foreach (KeyValuePair<Guid, ActivatedAbilityEntry> item in AbilityByGuid)
			{
				Writer.WriteComposite(item.Value);
			}
		}
		if (Cooldowns.IsNullOrEmpty())
		{
			Writer.WriteOptimized(0);
			return;
		}
		Writer.WriteOptimized(Cooldowns.Count);
		foreach (CommandCooldown cooldown in Cooldowns)
		{
			Writer.WriteTokenized(cooldown);
		}
	}

	public override void Read(GameObject Basis, SerializationReader Reader)
	{
		int num = Reader.ReadOptimizedInt32();
		AbilityByGuid = null;
		if (num > 0)
		{
			AbilityByGuid = new Dictionary<Guid, ActivatedAbilityEntry>(num);
			for (int i = 0; i < num; i++)
			{
				ActivatedAbilityEntry activatedAbilityEntry = Reader.ReadComposite<ActivatedAbilityEntry>();
				activatedAbilityEntry.Abilities = this;
				AbilityByGuid.Add(activatedAbilityEntry.ID, activatedAbilityEntry);
			}
		}
		num = Reader.ReadOptimizedInt32();
		if (num > 0)
		{
			Cooldowns = new List<CommandCooldown>(num);
			for (int j = 0; j < num; j++)
			{
				Cooldowns.Add((CommandCooldown)Reader.ReadTokenized());
			}
		}
	}

	public override IPart DeepCopy(GameObject Parent, Func<GameObject, GameObject> MapInv)
	{
		ActivatedAbilities activatedAbilities = new ActivatedAbilities();
		activatedAbilities._ParentObject = Parent;
		if (AbilityByGuid != null)
		{
			activatedAbilities.AbilityByGuid = new Dictionary<Guid, ActivatedAbilityEntry>();
			foreach (Guid key in AbilityByGuid.Keys)
			{
				ActivatedAbilityEntry activatedAbilityEntry = new ActivatedAbilityEntry(AbilityByGuid[key]);
				activatedAbilityEntry.Abilities = activatedAbilities;
				activatedAbilities.AbilityByGuid.Add(key, activatedAbilityEntry);
				if (activatedAbilityEntry.CommandCooldown.Segments > 0)
				{
					activatedAbilities.AddCooldown(activatedAbilityEntry.CommandCooldown);
				}
			}
		}
		return activatedAbilities;
	}

	public void AddCooldown(CommandCooldown Cooldown)
	{
		if (Cooldowns == null)
		{
			Cooldowns = new List<CommandCooldown>();
		}
		Cooldowns.Add(Cooldown);
	}

	public void RemoveCooldown(CommandCooldown Cooldown)
	{
		Cooldowns?.Remove(Cooldown);
	}

	public CommandCooldown FindCommandCooldown(string Command)
	{
		if (Cooldowns != null)
		{
			foreach (CommandCooldown cooldown in Cooldowns)
			{
				if (cooldown.Command == Command)
				{
					return cooldown;
				}
			}
		}
		return null;
	}

	public void TickCooldowns(int Segments)
	{
		if (Cooldowns == null)
		{
			return;
		}
		for (int num = Cooldowns.Count - 1; num >= 0; num--)
		{
			CommandCooldown commandCooldown = Cooldowns[num];
			commandCooldown.Segments -= Segments;
			if (commandCooldown.Segments <= 0)
			{
				commandCooldown.Segments = 0;
				Cooldowns.RemoveAt(num);
				if (ParentObject.IsPlayer())
				{
					SoundManager.PlayUISound("sfx_cooldown_end", 0.25f);
				}
			}
		}
	}

	public void ClearCooldowns()
	{
		if (Cooldowns != null)
		{
			for (int num = Cooldowns.Count - 1; num >= 0; num--)
			{
				Cooldowns[num].Segments = 0;
				Cooldowns.RemoveAt(num);
			}
		}
		foreach (KeyValuePair<Guid, ActivatedAbilityEntry> item in AbilityByGuid)
		{
			item.Deconstruct(out var _, out var value);
			value.CommandCooldown.Segments = 0;
		}
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public string GetNextAvailableCommandString(string Base)
	{
		RecalculateAbilityByCommand();
		int num = 1;
		using TextBuilder textBuilder = TextBuilder.Get(Base.Length + 4).Append(Base).Append(":");
		int length = textBuilder.Length;
		while (_abilityByCommand.ContainsKey(textBuilder.Append(num).AsSpan()))
		{
			num++;
			textBuilder.Length = length;
			if (num > 100)
			{
				MetricsManager.LogError("Next command string seems to be very far down the list: ${Base} ${count}");
				break;
			}
		}
		return textBuilder.ToString();
	}

	public Guid AddAbility(string Name, string Command, string Class, string Description = null, string Icon = "\a", string DisabledMessage = null, bool Toggleable = false, bool DefaultToggleState = false, bool ActiveToggle = false, bool IsAttack = false, bool IsRealityDistortionBased = false, bool IsWorldMapUsable = false, bool Silent = false, bool AIDisable = false, bool AlwaysAllowToggleOff = true, bool AffectedByWillpower = true, bool TickPerTurn = false, bool Distinct = false, int Cooldown = -1, string CommandForDescription = null, Renderable UITileDefault = null, Renderable UITileToggleOn = null, Renderable UITileDisabled = null, Renderable UITileCoolingDown = null)
	{
		if (Distinct)
		{
			ActivatedAbilityEntry abilityByCommand = GetAbilityByCommand(Command);
			if (abilityByCommand != null)
			{
				return abilityByCommand.ID;
			}
		}
		Guid guid = Guid.NewGuid();
		if (AbilityByGuid == null)
		{
			AbilityByGuid = new Dictionary<Guid, ActivatedAbilityEntry>();
		}
		ActivatedAbilityEntry activatedAbilityEntry = new ActivatedAbilityEntry();
		activatedAbilityEntry.Abilities = this;
		activatedAbilityEntry.ID = guid;
		activatedAbilityEntry.DisplayName = Name;
		activatedAbilityEntry.Command = Command;
		activatedAbilityEntry.CommandForDescription = CommandForDescription;
		activatedAbilityEntry.Class = Class;
		activatedAbilityEntry.Description = Description;
		activatedAbilityEntry.Icon = Icon;
		activatedAbilityEntry.DisabledMessage = DisabledMessage;
		activatedAbilityEntry.Toggleable = Toggleable;
		activatedAbilityEntry.ToggleState = DefaultToggleState;
		activatedAbilityEntry.ActiveToggle = ActiveToggle;
		activatedAbilityEntry.IsAttack = IsAttack;
		activatedAbilityEntry.IsRealityDistortionBased = IsRealityDistortionBased;
		activatedAbilityEntry.IsWorldMapUsable = IsWorldMapUsable;
		activatedAbilityEntry.AIDisable = AIDisable;
		activatedAbilityEntry.AlwaysAllowToggleOff = AlwaysAllowToggleOff;
		activatedAbilityEntry.AffectedByWillpower = AffectedByWillpower;
		activatedAbilityEntry.TickPerTurn = TickPerTurn;
		activatedAbilityEntry.UITileDefault = UITileDefault;
		activatedAbilityEntry.UITileToggleOn = UITileToggleOn;
		activatedAbilityEntry.UITileDisabled = UITileDisabled;
		activatedAbilityEntry.UITileCoolingDown = UITileCoolingDown;
		if (Description == null && Templates.TemplateByID.TryGetValue("ActivatedAbility." + activatedAbilityEntry.CommandForDescription, out var value))
		{
			activatedAbilityEntry.Description = value.Build((Templates.StatCollector)null);
		}
		activatedAbilityEntry.CommandCooldown = FindCommandCooldown(Command);
		if (activatedAbilityEntry.CommandCooldown == null)
		{
			activatedAbilityEntry.CommandCooldown = new CommandCooldown();
			activatedAbilityEntry.CommandCooldown.Command = Command;
			if (Cooldown > 0)
			{
				activatedAbilityEntry.Cooldown = Cooldown;
			}
		}
		AbilityByGuid.Add(guid, activatedAbilityEntry);
		if (ParentObject.IsPlayer())
		{
			ClearCache();
			AbilityManager.UpdateFavorites();
			if (!Silent && !this.Silent)
			{
				StringBuilder stringBuilder = Event.NewStringBuilder();
				stringBuilder.Append("You have gained the activated ability {{Y|").Append(Name).Append("}}.");
				if (ParentObject.GetIntProperty("HasAccessedAbilities") < 3 && CommandBindingManager.GetKeyFromCommand("CmdAbilities") == 65)
				{
					stringBuilder.Append("\n(press {{W|a}} to use activated abilities)");
				}
				Popup.Show(stringBuilder.ToString());
			}
		}
		return guid;
	}

	public bool RemoveAbility(Guid ID)
	{
		if (AbilityByGuid.TryGetValue(ID, out var value))
		{
			return RemoveAbility(value);
		}
		return false;
	}

	public bool RemoveAbilityByCommand(string Command)
	{
		foreach (KeyValuePair<Guid, ActivatedAbilityEntry> item in AbilityByGuid)
		{
			ActivatedAbilityEntry value = item.Value;
			if (value.Command == Command)
			{
				return RemoveAbility(value);
			}
		}
		return false;
	}

	public bool RemoveAbility(ActivatedAbilityEntry Entry)
	{
		if (!AbilityByGuid.Remove(Entry.ID))
		{
			return false;
		}
		if (ParentObject.IsPlayer())
		{
			ClearCache();
			AbilityManager.UpdateFavorites();
		}
		return true;
	}

	public IEnumerable<ActivatedAbilityEntry> YieldAbilitiesWithClass(string Class)
	{
		if (AbilityByGuid.IsNullOrEmpty())
		{
			yield break;
		}
		foreach (var (_, activatedAbilityEntry2) in AbilityByGuid)
		{
			if (activatedAbilityEntry2.Class == Class)
			{
				yield return activatedAbilityEntry2;
			}
		}
	}

	private static void ClearCache()
	{
		The.Player?.GetPart<ActivatedAbilities>()?._AbilityListOrderedByPreferenceCache?.Clear();
	}

	public IEnumerable<ActivatedAbilityEntry> GetAbilityListOrderedByPreference()
	{
		if (_AbilityListOrderedByPreferenceCache == null)
		{
			_AbilityListOrderedByPreferenceCache = new List<ActivatedAbilityEntry>(32);
		}
		if (_AbilityListOrderedByPreferenceCache.Count == 0)
		{
			_AbilityListOrderedByPreferenceCache.AddRange(_GetAbilityListOrderedByPreference());
		}
		return _AbilityListOrderedByPreferenceCache;
	}

	private void RecalculateAbilityByCommand()
	{
		if (_abilityByCommand == null)
		{
			_abilityByCommand = new StringMap<ActivatedAbilityEntry>();
		}
		_abilityByCommand?.Clear();
		if (AbilityByGuid == null)
		{
			return;
		}
		foreach (ActivatedAbilityEntry value in AbilityByGuid.Values)
		{
			if (value != null)
			{
				_abilityByCommand?.TryAdd(value.Command, value);
			}
		}
	}

	private IEnumerable<ActivatedAbilityEntry> _GetAbilityListOrderedByPreference()
	{
		if (AbilityByGuid == null)
		{
			yield break;
		}
		RecalculateAbilityByCommand();
		HashSet<ActivatedAbilityEntry> used = new HashSet<ActivatedAbilityEntry>();
		foreach (string item in PreferenceOrder)
		{
			if (_abilityByCommand.TryGetValue(item, out var Value) && !used.Contains(Value))
			{
				used.Add(Value);
				yield return Value;
			}
		}
		if (Enumerable.Count(used) == Enumerable.Count(_abilityByCommand))
		{
			yield break;
		}
		foreach (ActivatedAbilityEntry value in AbilityByGuid.Values)
		{
			if (!used.Contains(value))
			{
				_PreferenceOrder.Add(value.Command);
				yield return value;
			}
		}
	}
}
