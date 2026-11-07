using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleLib.Console;
using Cysharp.Text;
using Genkit;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using XRL;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World;
using XRL.World.Parts;

namespace Qud.UI;

[UIView("AbilityBar", false, false, false, null, null, false, 0, false, NavCategory = "Adventure", UICanvas = "AbilityBar", UICanvasHost = 1)]
public class AbilityBar : SingletonWindowBase<AbilityBar>
{
	private class AbilityDescription : IEquatable<AbilityDescription>
	{
		public int KeyCode;

		public ActivatedAbilityEntry Entry;

		public static Queue<ActivatedAbilityEntry> entryPool = new Queue<ActivatedAbilityEntry>();

		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
			{
				return false;
			}
			AbilityDescription abilityDescription = (AbilityDescription)obj;
			if (KeyCode == abilityDescription.KeyCode)
			{
				return Entry.Equals(abilityDescription.Entry);
			}
			return false;
		}

		public bool Equals(AbilityDescription obj)
		{
			if (KeyCode == obj.KeyCode)
			{
				return Entry.Equals(obj.Entry);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return new { KeyCode, Entry }.GetHashCode();
		}

		public static AbilityDescription next()
		{
			if (abilityDescriptionPool.Count > 0)
			{
				return abilityDescriptionPool.Dequeue();
			}
			return new AbilityDescription();
		}

		public void free()
		{
			if (Entry != null)
			{
				Entry.Clear();
				lock (entryPool)
				{
					entryPool.Enqueue(Entry);
				}
			}
			Entry = null;
			KeyCode = 0;
			abilityDescriptionPool.Enqueue(this);
		}

		public static ActivatedAbilityEntry nextEntry()
		{
			lock (entryPool)
			{
				if (entryPool.Count > 0)
				{
					return entryPool.Dequeue();
				}
				return new ActivatedAbilityEntry();
			}
		}

		public AbilityDescription Set(int KeyCode, ActivatedAbilityEntry Entry)
		{
			this.KeyCode = KeyCode;
			this.Entry = nextEntry().CopyFrom(Entry);
			return this;
		}
	}

	public UITextSkin EffectText;

	public RectTransform ButtonArea;

	public RectTransform TargetArea;

	public UITextSkin TargetText;

	public UITextSkin TargetHealthText;

	public UnityEngine.GameObject ButtonPrefab;

	public TextMeshProUGUI PageText;

	public UnityEngine.GameObject PagerArea;

	public UITextSkin AbilityCommandText;

	public UITextSkin CycleCommandText;

	public TextMeshProUGUI AbilityScreenHotkey;

	public MissileWeaponArea missileWeaponArea;

	private List<UnityEngine.GameObject> AbilityButtons = new List<UnityEngine.GameObject>();

	private List<string> Descriptions = new List<string>(16);

	private StringBuilder SB = new StringBuilder();

	private object effectLock = new object();

	private string effectText;

	private string lastEffectText;

	private uint LastEffectsHash;

	private bool effectTextDirty;

	private object targetLock = new object();

	private string targetText;

	private string targetHealthText;

	private string lastTargetText = "not null";

	private string lastTargetHealthText = "";

	private bool targetTextDirty;

	private int offset;

	private int numPerPage = 9;

	private static Queue<AbilityDescription> abilityDescriptionPool = new Queue<AbilityDescription>();

	private List<AbilityDescription> abilities = new List<AbilityDescription>();

	private List<AbilityDescription> lastAbilities = new List<AbilityDescription>();

	private bool abilitiesDirty;

	private List<AbilityDescription> currentAbilities;

	public static List<string> ABILITY_COMMANDS = new List<string> { "CmdAbility1", "CmdAbility2", "CmdAbility3", "CmdAbility4", "CmdAbility5", "CmdAbility6", "CmdAbility7", "CmdAbility8", "CmdAbility9", "CmdAbility10" };

	public static Dictionary<string, ActivatedAbilityEntry> hotkeyAbilityCommands = new Dictionary<string, ActivatedAbilityEntry>();

	public HashSet<string> usedHotkeys = new HashSet<string>();

	private int SelectedAbility;

	private int VisibleAbilityButtons;

	private int lastCount = -1;

	private int lastMax = -1;

	private int lastOffset = -1;

	public ActivatedAbilityEntry currentlySelectedAbility
	{
		get
		{
			if (SelectedAbility >= 0 && SelectedAbility < AbilityButtons.Count)
			{
				int num = offset + SelectedAbility;
				if (num >= 0 && num < abilities.Count)
				{
					return abilities[num].Entry;
				}
			}
			return null;
		}
	}

	public AbilityBar()
	{
		ControlManager.onActiveDeviceChanged += (InputDevice d) =>
		{
			markDirty();
		};
	}

	public static void markDirty()
	{
		if (SingletonWindowBase<AbilityBar>.instance != null)
		{
			SingletonWindowBase<AbilityBar>.instance.abilitiesDirty = true;
		}
	}

	public override bool AllowPassthroughInput()
	{
		return true;
	}

	public override void Init()
	{
		base.Init();
		XRLCore.RegisterAfterRenderCallback(AfterRender);
		missileWeaponArea.Init();
	}

	public static void UpdateActiveEffects()
	{
		XRL.World.GameObject player = The.Player;
		if (player != null)
		{
			SingletonWindowBase<AbilityBar>.instance.InternalUpdateActiveEffects(player);
		}
	}

	private void InternalUpdateActiveEffects(XRL.World.GameObject Object)
	{
		Descriptions.Clear();
		uint num = 16777619u;
		foreach (Effect effect in Object.Effects)
		{
			if (!effect.SuppressInStageDisplay() && effect.Duration > 0)
			{
				string description = effect.GetDescription();
				if (!description.IsNullOrEmpty())
				{
					Descriptions.Add(description);
					num = Hash.FNV1A32(description, num);
				}
			}
		}
		if (num == LastEffectsHash)
		{
			return;
		}
		LastEffectsHash = num;
		string text = "";
		Utf16ValueStringBuilder sb = ZString.CreateStringBuilder();
		try
		{
			sb.Append("{{Y|<color=#508d75>ACTIVE EFFECTS:</color>}} ");
			int i = 0;
			for (int count = Descriptions.Count; i < count; i++)
			{
				if (i != 0)
				{
					sb.Append(", ");
				}
				Sidebar.FormatToRTF(Markup.Wrap(Descriptions[i]), ref sb);
			}
			text = sb.ToString();
		}
		finally
		{
			sb.Dispose();
		}
		lock (effectLock)
		{
			effectText = text;
			effectTextDirty = true;
		}
	}

	private void AfterRender(XRLCore core, ScreenBuffer sb)
	{
		XRL.World.GameObject player = The.Player;
		if (player == null)
		{
			return;
		}
		string text = null;
		using (Utf16ValueStringBuilder utf16ValueStringBuilder = ZString.CreateStringBuilder())
		{
			utf16ValueStringBuilder.Append("{{Y|<color=#508d75>ACTIVE EFFECTS:</color>}} ");
			bool flag = true;
			foreach (Effect effect in player.Effects)
			{
				if (effect.SuppressInStageDisplay())
				{
					continue;
				}
				string description = effect.GetDescription();
				if (!string.IsNullOrEmpty(description))
				{
					if (!flag)
					{
						utf16ValueStringBuilder.Append(", ");
					}
					else
					{
						flag = false;
					}
					utf16ValueStringBuilder.Append(Markup.Wrap(description));
				}
			}
			text = utf16ValueStringBuilder.ToString();
		}
		if (lastEffectText != text)
		{
			lock (effectLock)
			{
				effectText = ((text != null) ? text.ToRTFCached() : null) ?? "";
				lastEffectText = text;
				effectTextDirty = true;
			}
		}
		ActivatedAbilities activatedAbilities = player.ActivatedAbilities;
		if (activatedAbilities != null && activatedAbilities.AbilityByGuid != null)
		{
			if (currentAbilities == null)
			{
				currentAbilities = new List<AbilityDescription>(activatedAbilities.AbilityByGuid.Count);
			}
			currentAbilities.Clear();
			foreach (ActivatedAbilityEntry item in activatedAbilities.GetAbilityListOrderedByPreference())
			{
				int keyCode = 0;
				currentAbilities.Add(AbilityDescription.next().Set(keyCode, item));
			}
		}
		else
		{
			if (currentAbilities == null && currentAbilities == null)
			{
				currentAbilities = new List<AbilityDescription>();
			}
			currentAbilities.Clear();
		}
		if (currentAbilities.Count != lastAbilities.Count || !currentAbilities.SequenceEqual(lastAbilities))
		{
			lock (abilities)
			{
				for (int i = 0; i < lastAbilities.Count; i++)
				{
					lastAbilities[i].free();
				}
				lastAbilities.Clear();
				lastAbilities.AddRange(currentAbilities);
				abilities.Clear();
				abilities.AddRange(currentAbilities);
				abilitiesDirty = true;
			}
		}
		else
		{
			for (int j = 0; j < currentAbilities.Count; j++)
			{
				currentAbilities[j].free();
			}
			currentAbilities.Clear();
		}
		XRL.World.GameObject currentTarget = Sidebar.CurrentTarget;
		if (currentTarget != null)
		{
			string text2 = null;
			string text3 = null;
			lock (SB)
			{
				SB.Clear().Append("{{C|<color=#3e83a5>TARGET:</color> ").Append(currentTarget.DisplayName)
					.Append("}}");
				text3 = SB.ToString();
				Description part = currentTarget.GetPart<Description>();
				SB.Clear().Append(Strings.WoundLevel(currentTarget));
				if (part != null)
				{
					if (!string.IsNullOrEmpty(part.GetFeelingDescription()))
					{
						SB.Append(", ").Append(Markup.Wrap(part.GetFeelingDescription()));
					}
					if (!string.IsNullOrEmpty(part.GetDifficultyDescription()))
					{
						SB.Append(", ").Append(Markup.Wrap(part.GetDifficultyDescription()));
					}
				}
				text2 = SB.ToString();
			}
			if (lastTargetText != text3 || lastTargetHealthText != text2)
			{
				lock (targetLock)
				{
					lastTargetText = text3;
					lastTargetHealthText = text2;
					targetText = text3 ?? "";
					targetHealthText = text2 ?? "";
					targetTextDirty = true;
					return;
				}
			}
			return;
		}
		string text4 = "{{K|TARGET: [none]}}";
		if (lastTargetText != text4)
		{
			lock (targetLock)
			{
				lastTargetText = text4;
				lastTargetHealthText = "";
				targetText = lastTargetText ?? "";
				targetHealthText = "";
				targetTextDirty = true;
			}
		}
	}

	public void OnTargetAreaClicked()
	{
		ConsoleLib.Console.Keyboard.PushMouseEvent("Command:CmdTarget");
	}

	public void OnEffectsAreaClicked()
	{
		ConsoleLib.Console.Keyboard.PushMouseEvent("Command:CmdShowEffects");
	}

	public override void Show()
	{
		base.Show();
	}

	public void OnEffectsClick()
	{
		Debug.Log("Click effects");
	}

	public void Update()
	{
		if (!canvas.enabled)
		{
			return;
		}
		if (GameManager.Instance.CurrentGameView == "Stage")
		{
			if (ControlManager.isCommandDown("Ability Page Up"))
			{
				MovePage(-1);
			}
			if (ControlManager.isCommandDown("Ability Page Down"))
			{
				MovePage(1);
			}
			if (ControlManager.isCommandDown("Next Ability"))
			{
				MoveSelection(1);
			}
			if (ControlManager.isCommandDown("Previous Ability"))
			{
				MoveSelection(-1);
			}
			if (ControlManager.isCommandDown("Use Ability"))
			{
				currentlySelectedAbility?.TrySendCommandEventOnPlayer();
			}
		}
		string commandInputDescription = ControlManager.getCommandInputDescription("CmdAbilities");
		if (AbilityScreenHotkey.text != commandInputDescription)
		{
			AbilityScreenHotkey.text = commandInputDescription;
		}
		if (effectTextDirty)
		{
			lock (effectLock)
			{
				if (EffectText.text != effectText)
				{
					EffectText.SetText(effectText);
				}
				effectTextDirty = false;
			}
		}
		if (targetTextDirty)
		{
			lock (targetLock)
			{
				if (TargetArea.gameObject.activeSelf != (targetText != null))
				{
					TargetArea.gameObject.SetActive(targetText != null);
				}
				if (TargetText.text != targetText)
				{
					TargetText.SetText(targetText);
				}
				if (TargetHealthText.text != targetHealthText)
				{
					TargetHealthText.SetText(targetHealthText);
				}
				targetTextDirty = false;
			}
		}
		if (abilitiesDirty)
		{
			lock (abilities)
			{
				lock (AbilityDescription.entryPool)
				{
					foreach (ActivatedAbilityEntry value in hotkeyAbilityCommands.Values)
					{
						if (value != null)
						{
							value.Clear();
							AbilityDescription.entryPool.Enqueue(value);
						}
					}
				}
				hotkeyAbilityCommands.Clear();
				if (AbilityButtons.Count != abilities.Count)
				{
					foreach (Transform item in ButtonArea.transform)
					{
						if (!AbilityButtons.Contains(item.gameObject))
						{
							UnityEngine.Object.Destroy(item.gameObject);
						}
					}
					while (AbilityButtons.Count > abilities.Count)
					{
						AbilityButtons[AbilityButtons.Count - 1].Destroy();
						AbilityButtons.RemoveAt(AbilityButtons.Count - 1);
					}
					while (AbilityButtons.Count < abilities.Count)
					{
						UnityEngine.GameObject gameObject = ButtonPrefab.Instantiate();
						gameObject.transform.SetParent(ButtonArea.transform, worldPositionStays: false);
						AbilityButtons.Add(gameObject);
					}
				}
				usedHotkeys.Clear();
				for (int i = 0; i < abilities.Count; i++)
				{
					if (abilities[i].Entry != null)
					{
						string commandInputDescription2 = ControlManager.getCommandInputDescription(abilities[i].Entry.Command);
						abilities[i].Entry.DisplayForHotkey = commandInputDescription2;
						if (!string.IsNullOrEmpty(commandInputDescription2))
						{
							usedHotkeys.Add(commandInputDescription2);
						}
					}
				}
				for (int j = 0; j < abilities.Count; j++)
				{
					AbilityBarButton component = AbilityButtons[j].GetComponent<AbilityBarButton>();
					component.Icon.FromRenderable(abilities[j].Entry.GetUITile());
					ControlId.Assign(component.gameObject, "AbilityBar:Button:" + abilities[j].Entry?.Command);
					using Utf16ValueStringBuilder utf16ValueStringBuilder = ZString.CreateStringBuilder();
					utf16ValueStringBuilder.Append((Options.AbilityBarMode == 1) ? "" : ((abilities[j].Entry.Enabled ? "&C" : "&c") + abilities[j].Entry.DisplayName));
					if (!abilities[j].Entry.Enabled)
					{
						utf16ValueStringBuilder.Append(" {{K|[disabled]}}");
					}
					else if (abilities[j].Entry.Cooldown > 0)
					{
						utf16ValueStringBuilder.Append(" {{C|[" + abilities[j].Entry.CooldownRounds + "]}}");
					}
					if (abilities[j].Entry.Toggleable)
					{
						if (abilities[j].Entry.ToggleState)
						{
							utf16ValueStringBuilder.Append(" {{K|[{{g|on}}]}}");
						}
						else
						{
							utf16ValueStringBuilder.Append(" {{K|[{{y|off}}]}}");
						}
					}
					if (Options.ModernUI)
					{
						string text = abilities[j].Entry.DisplayForHotkey;
						string commandInputDescription3 = ControlManager.getCommandInputDescription("CmdAbility" + (j - offset + 1));
						if (!usedHotkeys.Contains(commandInputDescription3))
						{
							hotkeyAbilityCommands.Add("CmdAbility" + (j - offset + 1), AbilityDescription.nextEntry().CopyFrom(abilities[j].Entry));
							if (string.IsNullOrEmpty(text))
							{
								text = commandInputDescription3;
							}
						}
						if (!string.IsNullOrEmpty(text))
						{
							utf16ValueStringBuilder.Append(" {{Y|<{{w|" + text + "}}>}}");
						}
					}
					else if (CapabilityManager.AllowKeyboardHotkeys && abilities[j].KeyCode != int.MaxValue)
					{
						utf16ValueStringBuilder.Append(" {{Y|<{{w|" + ConsoleLib.Console.Keyboard.MetaToString(abilities[j].KeyCode) + "}}>}}");
					}
					component.disabled = !abilities[j].Entry.Enabled || abilities[j].Entry.Cooldown > 0;
					component.command = abilities[j].Entry.Command;
					string text2 = utf16ValueStringBuilder.ToString();
					string text3 = ((text2 != null) ? text2.ToRTFCached() : null) ?? "";
					if (component.Text.text != text3)
					{
						TextMeshProUGUI text4 = component.Text;
						string text5 = utf16ValueStringBuilder.ToString();
						text4.SetText(((text5 != null) ? text5.ToRTFCached() : null) ?? "");
					}
				}
				abilitiesDirty = false;
			}
			UpdateAbilitiesText();
		}
		int num = 175;
		if (Options.AbilityBarMode == 1)
		{
			num = 90;
		}
		if (ButtonArea != null && Math.Floor(ButtonArea.rect.width / (float)num) != (double)numPerPage)
		{
			if (numPerPage != 0)
			{
				offset /= Math.Max(1, numPerPage);
			}
			numPerPage = Math.Max(1, (int)ButtonArea.rect.width / num);
			offset *= numPerPage;
		}
		if (numPerPage > 0)
		{
			while (offset >= AbilityButtons.Count)
			{
				offset -= numPerPage;
			}
			while (offset >= abilities.Count)
			{
				offset -= numPerPage;
			}
			offset = Math.Max(0, offset);
		}
		PageText.text = ((numPerPage == 0) ? "0" : (offset / Math.Max(1, numPerPage) + 1).ToStringCached());
		UpdateAbilitiesText();
		PagerArea.SetActive(AbilityButtons.Count == 0 || AbilityButtons.Count > numPerPage);
		VisibleAbilityButtons = 0;
		for (int k = 0; k < AbilityButtons.Count; k++)
		{
			bool flag = k >= offset && k < offset + numPerPage;
			if (AbilityButtons[k].activeSelf != flag)
			{
				AbilityButtons[k].SetActive(flag);
			}
			if (flag)
			{
				VisibleAbilityButtons++;
			}
		}
		if (SelectedAbility < 0)
		{
			SelectedAbility = numPerPage - 1;
		}
		if (SelectedAbility >= VisibleAbilityButtons)
		{
			SelectedAbility = 0;
		}
		bool flag2 = ControlManager.isCommandMapped("Use Ability");
		for (int l = offset; l < offset + numPerPage && l < AbilityButtons.Count; l++)
		{
			AbilityButtons[l].GetComponent<AbilityBarButton>().highlighted = (l - offset == SelectedAbility) & flag2;
		}
	}

	public void AbilitiesButtonClicked()
	{
		ConsoleLib.Console.Keyboard.PushMouseEvent("Command:CmdAbilities");
	}

	public void MoveSelection(int direction)
	{
		SelectedAbility += direction;
		if (SelectedAbility < 0)
		{
			SelectedAbility = VisibleAbilityButtons - 1;
		}
		if (SelectedAbility >= VisibleAbilityButtons)
		{
			SelectedAbility = 0;
		}
		abilitiesDirty = true;
	}

	public void MoveToNextPage()
	{
		MovePage(1);
	}

	public void MoveToPreviousPage()
	{
		MovePage(-1);
	}

	public void MoveToPage(int page)
	{
		offset = 0;
		for (int i = 0; i < page; i++)
		{
			MovePage(1);
		}
		abilitiesDirty = true;
	}

	public void UpdateAbilitiesText()
	{
		int count = AbilityButtons.Count;
		int num = Math.Max(1, numPerPage);
		if (count != lastCount || num != lastMax || offset != lastOffset)
		{
			lastCount = count;
			lastMax = num;
			lastOffset = offset;
			if (AbilityButtons.Count / Math.Max(1, numPerPage) > 0)
			{
				CycleCommandText.GetComponent<UITextSkin>().text = ControlManager.getCommandInputDescription("Ability Page Up", mapGlyphs: true, allowAlt: true) + " " + ControlManager.getCommandInputDescription("Ability Page Down", mapGlyphs: true, allowAlt: true);
				CycleCommandText.GetComponent<UITextSkin>().Apply();
				AbilityCommandText.GetComponent<UITextSkin>().text = $"ABILITIES\npage {offset / Math.Max(1, numPerPage) + 1} of {(AbilityButtons.Count - 1) / Math.Max(1, numPerPage) + 1}";
				AbilityCommandText.GetComponent<UITextSkin>().Apply();
			}
			else
			{
				CycleCommandText.GetComponent<UITextSkin>().text = "";
				CycleCommandText.GetComponent<UITextSkin>().Apply();
				AbilityCommandText.GetComponent<UITextSkin>().text = "ABILITIES";
				AbilityCommandText.GetComponent<UITextSkin>().Apply();
			}
		}
	}

	public void MovePage(int direction)
	{
		offset += direction * numPerPage;
		if (offset >= AbilityButtons.Count)
		{
			offset = 0;
		}
		if (offset < 0)
		{
			offset = Math.Max(0, (int)Math.Floor(((float)AbilityButtons.Count - 1f) / (float)Math.Max(1, numPerPage)) * numPerPage);
		}
		abilitiesDirty = true;
		UpdateAbilitiesText();
	}
}
