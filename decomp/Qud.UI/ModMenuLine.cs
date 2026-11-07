using System.Collections.Generic;
using UnityEngine;
using XRL;
using XRL.UI;

namespace Qud.UI;

public class ModMenuLine : ControlledSelectable
{
	public ModInfo modInfo;

	public UITextSkin titleText;

	public GameObject authorSpacer;

	public UITextSkin authorText;

	public InfoChip version;

	public InfoChip size;

	public InfoChip tags;

	public InfoChip location;

	public ImageTinyFrame imageFrame;

	public RectTransform taggedArea;

	public GameObject taggedPrefab;

	private string _lastAuthor = "\0";

	private string _lastTitle = "\0";

	private string _lastPath = "\0";

	private long _lastSize;

	private Version _lastVersion;

	private string[] _lastTags;

	private List<GameObject> _tagged = new List<GameObject>();

	private ModState? _lastState;

	private bool _lastHasUpdate;

	private Sprite _sprite;

	private string _imgPath;

	public void SetTag(string Text, ref int Index, bool State = true)
	{
		if (State)
		{
			GameObject gameObject = null;
			if (Index < _tagged.Count)
			{
				gameObject = _tagged[Index];
			}
			else
			{
				gameObject = taggedPrefab.Instantiate();
				gameObject.transform.SetParent(taggedArea, worldPositionStays: false);
				_tagged.Add(gameObject);
			}
			gameObject.GetComponentInChildren<UITextSkin>()?.SetText(Text);
			gameObject.SetActive(value: true);
			Index++;
		}
	}

	public override void Update()
	{
		if (modInfo != data)
		{
			modInfo = data as ModInfo;
		}
		base.Update();
		if (modInfo == null)
		{
			return;
		}
		if (modInfo.DisplayTitle != _lastTitle || _lastState != modInfo.State)
		{
			_lastTitle = modInfo.DisplayTitle;
			if (modInfo.State == ModState.MissingDependency)
			{
				titleText.SetText("{{W|" + modInfo.DisplayTitle + "}}");
			}
			else if (modInfo.State == ModState.Failed)
			{
				titleText.SetText("{{R|" + modInfo.DisplayTitle + "}}");
			}
			else if (modInfo.State == ModState.Disabled)
			{
				titleText.SetText("{{K|" + modInfo.DisplayTitle + "}}");
			}
			else if (modInfo.State == ModState.Enabled)
			{
				titleText.SetText("{{Y|" + modInfo.DisplayTitle + "}}");
			}
		}
		if (modInfo.Manifest.Author != _lastAuthor)
		{
			_lastAuthor = modInfo.Manifest.Author;
			if (string.IsNullOrEmpty(modInfo.Manifest.Author))
			{
				authorSpacer.SetActive(value: false);
				authorText.gameObject.SetActive(value: false);
			}
			else
			{
				authorSpacer.SetActive(value: true);
				authorText.gameObject.SetActive(value: true);
				authorText.SetText("{{y|by " + _lastAuthor + "}}");
			}
		}
		if (version != null && modInfo.Manifest.Version != _lastVersion)
		{
			_lastVersion = modInfo.Manifest.Version;
			version.value = modInfo.Manifest.Version.ToString();
			version.gameObject.SetActive(!_lastVersion.IsZero());
		}
		if (tags != null && modInfo.Manifest.Tags != _lastTags)
		{
			_lastTags = modInfo.Manifest.Tags;
			tags.value = (_lastTags.IsNullOrEmpty() ? "" : string.Join(", ", _lastTags));
			tags.gameObject.SetActive(!tags.value.IsNullOrEmpty());
		}
		if (modInfo.Path != _lastPath)
		{
			_lastPath = modInfo.Path;
			location.value = DataManager.SanitizePathForDisplay(modInfo.Path);
		}
		if (modInfo.Size != _lastSize)
		{
			_lastSize = modInfo.Size;
			double num = _lastSize;
			if (num >= 1048576.0)
			{
				num /= 1048576.0;
				size.value = $"{num:0.00} MB";
			}
			else if (_lastSize >= 1024)
			{
				num /= 1024.0;
				size.value = $"{num:0} KB";
			}
			else
			{
				size.value = _lastSize + " bytes";
			}
		}
		if (CapabilityManager.CurrentPlatformClassification() == CapabilityManager.PlatformClassification.Console)
		{
			location.gameObject.SetActive(value: false);
			size.gameObject.SetActive(value: false);
		}
		if (_lastState != modInfo.State || _lastHasUpdate != modInfo.HasUpdate)
		{
			int Index = 0;
			_lastState = modInfo.State;
			_lastHasUpdate = modInfo.HasUpdate;
			switch (modInfo.State)
			{
			case ModState.MissingDependency:
				SetTag("{{yellow|MISSING DEPENDENCY}}", ref Index);
				imageFrame.borderColor = The.Color.Yellow;
				break;
			case ModState.Enabled:
				SetTag("{{green|ENABLED}}", ref Index);
				imageFrame.borderColor = The.Color.DarkGreen;
				break;
			case ModState.Disabled:
				SetTag("{{black|DISABLED}}", ref Index);
				imageFrame.borderColor = The.Color.Black;
				break;
			case ModState.Failed:
				SetTag("{{red|FAILED}}", ref Index);
				imageFrame.borderColor = The.Color.DarkRed;
				break;
			}
			if (modInfo.GetSprite() != null && imageFrame.sprite != modInfo.GetSprite())
			{
				imageFrame.sprite = modInfo.GetSprite();
			}
			SetTag("{{W|# UPDATE AVAILABLE}}", ref Index, _lastHasUpdate);
			SetTag("{{w|# SCRIPTING}}", ref Index, modInfo.IsScripting);
			SetTag("{{W|# HARMONY PATCHES}}", ref Index, modInfo.Harmony != null);
			for (int num2 = _tagged.Count - 1; num2 >= Index; num2--)
			{
				_tagged[num2].SetActive(value: false);
			}
		}
	}
}
