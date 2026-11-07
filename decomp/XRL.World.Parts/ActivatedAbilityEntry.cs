using System;
using System.CodeDom.Compiler;
using ConsoleLib.Console;
using Occult.Engine.CodeGeneration;
using XRL.Messages;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
[GenerateSerializationPartial]
public class ActivatedAbilityEntry : IEquatable<ActivatedAbilityEntry>, IComposite
{
	public const int FLAG_ENABLED = 1;

	public const int FLAG_VISIBLE = 2;

	public const int FLAG_TOGGLE = 4;

	public const int FLAG_TOGGLE_STATE = 8;

	public const int FLAG_TOGGLE_ACTIVE = 16;

	public const int FLAG_TOGGLE_ALLOW_OFF = 32;

	public const int FLAG_ATTACK = 64;

	public const int FLAG_REALITY_DISTORT = 128;

	public const int FLAG_WILLPOWER = 256;

	public const int FLAG_TICK_TURN = 512;

	public const int FLAG_WORLD_MAP = 1024;

	public const int FLAG_AI_DISABLE = 2048;

	public Guid ID;

	public string DisplayName;

	public string Command;

	public string Class;

	public string Description;

	public string Icon;

	public string DisabledMessage;

	public int Flags = 289;

	public CommandCooldown CommandCooldown;

	public Renderable UITileDefault;

	public Renderable UITileToggleOn;

	public Renderable UITileDisabled;

	public Renderable UITileCoolingDown;

	[NonSerialized]
	public string DisplayForHotkey;

	/// <summary>
	///     This property is only public to make it easy to serialize, please use <see cref="P:XRL.World.Parts.ActivatedAbilityEntry.CommandForDescription" /> instead.
	/// </summary>
	public string _DescriptionCommand;

	[NonSerialized]
	public ActivatedAbilities Abilities;

	[NonSerialized]
	private ActivatedAbilities.XmlData.Data _Data;

	private static long SoundSegment = -1L;

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public virtual bool WantFieldReflection => false;

	public bool Toggleable
	{
		get
		{
			return Flags.HasBit(4);
		}
		set
		{
			Flags.SetBit(4, value);
		}
	}

	public bool ToggleState
	{
		get
		{
			return Flags.HasBit(8);
		}
		set
		{
			Flags.SetBit(8, value);
		}
	}

	/// <summary>
	///     Though not nessecarily <see cref="P:XRL.World.Parts.ActivatedAbilityEntry.Toggleable" />, this ability uses ToggleState to show the "Active" part of its
	///     Cooldown, and should be allowed to Toggle Off while active (and on cooldown).
	/// </summary>
	public bool ActiveToggle
	{
		get
		{
			return Flags.HasBit(16);
		}
		set
		{
			Flags.SetBit(16, value);
		}
	}

	public bool Enabled
	{
		get
		{
			return Flags.HasBit(1);
		}
		set
		{
			Flags.SetBit(1, value);
		}
	}

	public bool IsAttack
	{
		get
		{
			return Flags.HasBit(64);
		}
		set
		{
			Flags.SetBit(64, value);
		}
	}

	public bool IsRealityDistortionBased
	{
		get
		{
			return Flags.HasBit(128);
		}
		set
		{
			Flags.SetBit(128, value);
		}
	}

	public bool IsWorldMapUsable
	{
		get
		{
			return Flags.HasBit(1024);
		}
		set
		{
			Flags.SetBit(1024, value);
		}
	}

	public bool AIDisable
	{
		get
		{
			return Flags.HasBit(2048);
		}
		set
		{
			Flags.SetBit(2048, value);
		}
	}

	public bool AlwaysAllowToggleOff
	{
		get
		{
			return Flags.HasBit(32);
		}
		set
		{
			Flags.SetBit(32, value);
		}
	}

	public bool Visible
	{
		get
		{
			return Flags.HasBit(2);
		}
		set
		{
			Flags.SetBit(2, value);
		}
	}

	public bool AffectedByWillpower
	{
		get
		{
			return Flags.HasBit(256);
		}
		set
		{
			Flags.SetBit(256, value);
		}
	}

	public bool TickPerTurn
	{
		get
		{
			return Flags.HasBit(512);
		}
		set
		{
			Flags.SetBit(512, value);
		}
	}

	/// <summary>
	///     The description and icon entry data to use from the <see cref="F:XRL.World.Parts.ActivatedAbilities.XmlData.DataByCommand" />.
	///     Defaults to <see cref="!:CommandForHotkey" />, should probably just be the same unless you want to share hotkeys,
	///     but have different icon and description, such as the case of rocket skates and sprint. 
	///
	///     <para>
	///         Changing this value will cause all UITile attributes to be reset to null and filled in with the XmlData defaults
	///         on next access.
	///     </para>
	/// </summary>
	public string CommandForDescription
	{
		get
		{
			return _DescriptionCommand ?? (_DescriptionCommand = Command);
		}
		set
		{
			_DescriptionCommand = value;
			UITileCoolingDown = null;
			UITileDefault = null;
			UITileDisabled = null;
			UITileToggleOn = null;
		}
	}

	public GameObject ParentObject => Abilities._ParentObject;

	public int Cooldown
	{
		get
		{
			if (!AlwaysAllowToggleOff || !ToggleState || !Toggleable)
			{
				return CommandCooldown.Segments;
			}
			return 0;
		}
		set
		{
			if (CommandCooldown.Segments <= 0 && value > 0)
			{
				Abilities.AddCooldown(CommandCooldown);
			}
			else if (CommandCooldown.Segments > 0 && value <= 0)
			{
				Abilities.RemoveCooldown(CommandCooldown);
			}
			CommandCooldown.Segments = value;
		}
	}

	[Obsolete("Use CooldownRounds instead of CooldownTurns")]
	public int CooldownTurns => CooldownRounds;

	public int CooldownRounds => (int)Math.Ceiling((double)Cooldown / 10.0);

	public string CooldownDescription => CooldownRounds.Things("round");

	public bool IsUsable
	{
		get
		{
			if (!Enabled)
			{
				return false;
			}
			if (Cooldown > 0 && (!ToggleState || !ActiveToggle))
			{
				return false;
			}
			return true;
		}
	}

	public bool IsAIUsable
	{
		get
		{
			if (IsUsable)
			{
				return !AIDisable;
			}
			return false;
		}
	}

	public string NotUsableDescription
	{
		get
		{
			if (!Enabled)
			{
				if (!DisabledMessage.IsNullOrEmpty())
				{
					return DisabledMessage;
				}
				return Markup.Color("C", DisplayName) + " can't be used at this time.";
			}
			if (Cooldown > 0 && (!ToggleState || !ActiveToggle))
			{
				return "You must wait " + Markup.Color("C", CooldownDescription) + " before using " + Markup.Color("C", DisplayName) + ".";
			}
			return null;
		}
	}

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public virtual void Write(SerializationWriter Writer)
	{
		Writer.Write(ID);
		Writer.WriteOptimized(DisplayName);
		Writer.WriteOptimized(Command);
		Writer.WriteOptimized(Class);
		Writer.WriteOptimized(Description);
		Writer.WriteOptimized(Icon);
		Writer.WriteOptimized(DisabledMessage);
		Writer.WriteOptimized(Flags);
		Writer.WriteTokenized(CommandCooldown);
		Writer.Write(UITileDefault);
		Writer.Write(UITileToggleOn);
		Writer.Write(UITileDisabled);
		Writer.Write(UITileCoolingDown);
		Writer.WriteOptimized(_DescriptionCommand);
	}

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public virtual void Read(SerializationReader Reader)
	{
		ID = Reader.ReadGuid();
		DisplayName = Reader.ReadOptimizedString();
		Command = Reader.ReadOptimizedString();
		Class = Reader.ReadOptimizedString();
		Description = Reader.ReadOptimizedString();
		Icon = Reader.ReadOptimizedString();
		DisabledMessage = Reader.ReadOptimizedString();
		Flags = Reader.ReadOptimizedInt32();
		CommandCooldown = (CommandCooldown)Reader.ReadTokenized();
		UITileDefault = (Renderable)Reader.ReadComposite();
		UITileToggleOn = (Renderable)Reader.ReadComposite();
		UITileDisabled = (Renderable)Reader.ReadComposite();
		UITileCoolingDown = (Renderable)Reader.ReadComposite();
		_DescriptionCommand = Reader.ReadOptimizedString();
	}

	public ActivatedAbilityEntry()
	{
	}

	public ActivatedAbilityEntry(ActivatedAbilityEntry Source)
		: this()
	{
		CopyFrom(Source);
	}

	public ActivatedAbilityEntry Clear()
	{
		ID = Guid.Empty;
		DisplayName = null;
		Command = null;
		Class = null;
		Description = null;
		Icon = null;
		DisabledMessage = null;
		Flags = 0;
		if (CommandCooldown != null)
		{
			CommandCooldown.Command = null;
			CommandCooldown.Segments = 0;
		}
		CommandForDescription = null;
		UITileDefault = null;
		UITileCoolingDown = null;
		UITileDisabled = null;
		UITileToggleOn = null;
		_Data = null;
		return this;
	}

	public ActivatedAbilityEntry CopyFrom(ActivatedAbilityEntry Source)
	{
		ID = Source.ID;
		DisplayName = Source.DisplayName;
		Command = Source.Command;
		Class = Source.Class;
		Description = Source.Description;
		Icon = Source.Icon;
		DisabledMessage = Source.DisabledMessage;
		Flags = Source.Flags;
		if (CommandCooldown == null)
		{
			CommandCooldown = new CommandCooldown();
		}
		CommandCooldown.Command = Source.CommandCooldown?.Command;
		CommandCooldown.Segments = Source.CommandCooldown?.Segments ?? 0;
		CommandForDescription = Source.CommandForDescription;
		UITileDefault = Source.UITileDefault;
		UITileCoolingDown = Source.UITileCoolingDown;
		UITileDisabled = Source.UITileDisabled;
		UITileToggleOn = Source.UITileToggleOn;
		_Data = null;
		return this;
	}

	public ActivatedAbilities.XmlData.Data GetData()
	{
		if (_Data == null)
		{
			ActivatedAbilities.XmlData.DataByCommand.TryGetValue(CommandForDescription, out _Data);
		}
		return _Data;
	}

	public bool TryGetTag(string Name, out string Value)
	{
		ActivatedAbilities.XmlData.Data data = GetData();
		if (data == null)
		{
			Value = null;
			return false;
		}
		return data.TryGetTag(Name, out Value);
	}

	public bool HasTag(string Name)
	{
		return GetData()?.HasTag(Name) ?? false;
	}

	public Renderable GetUITile()
	{
		ActivatedAbilities.XmlData.Data data = GetData();
		if (data != null)
		{
			if (UITileDefault == null && data.UITiles.TryGetValue(ActivatedAbilities.XmlData.UITileStates.Default, out var value))
			{
				UITileDefault = value;
			}
			if (UITileCoolingDown == null && data.UITiles.TryGetValue(ActivatedAbilities.XmlData.UITileStates.CoolingDown, out var value2))
			{
				UITileCoolingDown = value2;
			}
			if (UITileDisabled == null && data.UITiles.TryGetValue(ActivatedAbilities.XmlData.UITileStates.CoolingDown, out var value3))
			{
				UITileDisabled = value3;
			}
			if (UITileToggleOn == null && data.UITiles.TryGetValue(ActivatedAbilities.XmlData.UITileStates.ToggleOn, out var value4))
			{
				UITileToggleOn = value4;
			}
		}
		if (UITileDefault == null)
		{
			UITileDefault = new Renderable(null, Icon ?? "", "&w", null, 'W');
		}
		if (ToggleState)
		{
			return UITileToggleOn ?? (UITileToggleOn = new Renderable(UITileDefault.Tile?.Replace("_off", "_on"), UITileDefault.RenderString, UITileDefault.ColorString?.ToUpper(), null, char.ToUpper(UITileDefault.DetailColor)));
		}
		if (Cooldown > 0)
		{
			return UITileCoolingDown ?? (UITileCoolingDown = new Renderable(UITileDefault, null, null, "&c", "&c", 'c'));
		}
		if (!Enabled)
		{
			return UITileDisabled ?? (UITileDisabled = new Renderable(UITileDefault, null, null, "&K", "&K", 'K'));
		}
		return UITileDefault;
	}

	public bool Equals(ActivatedAbilityEntry Entry)
	{
		if (Entry != null && ID == Entry.ID && DisplayName == Entry.DisplayName && Command == Entry.Command && CommandForDescription == Entry.CommandForDescription && Class == Entry.Class && Description == Entry.Description && Icon == Entry.Icon && DisabledMessage == Entry.DisabledMessage && Toggleable == Entry.Toggleable && ToggleState == Entry.ToggleState && ActiveToggle == Entry.ActiveToggle && IsAttack == Entry.IsAttack && IsRealityDistortionBased == Entry.IsRealityDistortionBased && AlwaysAllowToggleOff == Entry.AlwaysAllowToggleOff && Visible == Entry.Visible && AffectedByWillpower == Entry.AffectedByWillpower && TickPerTurn == Entry.TickPerTurn && Cooldown == Entry.Cooldown)
		{
			return GetUITile() == Entry.GetUITile();
		}
		return false;
	}

	public override bool Equals(object Object)
	{
		return Equals(Object as ActivatedAbilityEntry);
	}

	public override int GetHashCode()
	{
		return new
		{
			ID, DisplayName, Command, CommandForDescription, Class, Description, Icon, DisabledMessage, Toggleable, ActiveToggle,
			IsAttack, IsRealityDistortionBased, AlwaysAllowToggleOff, Visible, AffectedByWillpower, TickPerTurn
		}.GetHashCode();
	}

	public int GetBaseHashCode()
	{
		return new { Command, Class, Toggleable, ActiveToggle, IsAttack, IsRealityDistortionBased, AlwaysAllowToggleOff, Visible, AffectedByWillpower, TickPerTurn }.GetHashCode();
	}

	public override string ToString()
	{
		string text = DisplayName + " (" + Command + ") " + Class + "/" + DisplayName + " [" + ID.ToString() + "]";
		if (!Enabled)
		{
			text += " {{K|[disabled]}}";
		}
		return text;
	}

	public void TrySendCommandEventOnPlayer()
	{
		string executeCommand = Command;
		string failMessage = NotUsableDescription;
		GameManager.Instance.gameQueue.queueTask(() =>
		{
			if (The.Player.OnWorldMap() && !AbilityManager.IsWorldMapUsable(executeCommand))
			{
				MessageQueue.AddPlayerMessage("You cannot do that on the world map.");
			}
			else if (failMessage == null)
			{
				CommandEvent.Send(The.Player, executeCommand);
			}
			else
			{
				MessageQueue.AddPlayerMessage(failMessage);
			}
		});
	}

	/// <param name="Source">The source of the ability refresh, e.g. a mutation, skill, or item.</param>
	public void Refresh(object Source = null)
	{
		Cooldown = 0;
		long num = The.Game?.Segments ?? (-1);
		if (num != SoundSegment)
		{
			Abilities?.PlayWorldSound("sfx_characterTrigger_cooldown_refreshed");
			SoundSegment = num;
		}
	}

	public void AddScaledCooldown(int Cooldown)
	{
		if (The.Core.cool && ParentObject != null && ParentObject.IsPlayer())
		{
			this.Cooldown = 0;
			return;
		}
		int num = GetCooldownEvent.GetFor(ParentObject, this, Cooldown);
		this.Cooldown += num;
	}

	public void SetScaledCooldown(int Cooldown)
	{
		if (The.Core.cool && ParentObject != null && ParentObject.IsPlayer())
		{
			this.Cooldown = 0;
			return;
		}
		int num = GetCooldownEvent.GetFor(ParentObject, this, Cooldown);
		if (TickPerTurn)
		{
			num += 10;
		}
		else if (ParentObject?.Energy != null)
		{
			double a = (1000.0 - (double)ParentObject.Energy.Value) / (double)((ParentObject.Speed > 0) ? ParentObject.Speed : 100);
			num += Math.Max(0, (int)Math.Ceiling(a));
		}
		this.Cooldown = num;
	}
}
