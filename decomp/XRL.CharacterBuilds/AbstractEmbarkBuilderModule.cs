using System;
using System.Collections.Generic;
using Genkit;
using XRL.UI.Framework;

namespace XRL.CharacterBuilds;

/// <summary>
///     Abstract base class for all EmbarkBuilder modules.  Implements the basics and provides many override points.
/// </summary>
public abstract class AbstractEmbarkBuilderModule
{
	/// <summary>
	///     A reference to the current EmbarkBuilder
	/// </summary>
	public EmbarkBuilder builder;

	/// <summary>
	///     A dictionary of string keyed window descriptors.
	/// </summary>
	public Dictionary<string, EmbarkBuilderModuleWindowDescriptor> windows = new Dictionary<string, EmbarkBuilderModuleWindowDescriptor>();

	/// <summary>
	/// holds the internal state of the enabled switch
	/// </summary>
	protected bool _enabled = true;

	private AbstractEmbarkBuilderModuleData _values;

	/// <summary>
	///     Stores the EmbarkBuilderModuleWindowDescriptor being currently being loaded by the XmlDataHelper.
	/// </summary>
	protected EmbarkBuilderModuleWindowDescriptor CurrentLoadingWindowDescriptor;

	/// <summary>
	///     The string name of the module class.
	/// </summary>
	public string type => GetType().Name;

	/// <summary>
	/// use enable() and disable() to change state
	/// </summary>
	public bool enabled => _enabled;

	/// <summary>
	/// The default values when a module is initialized or re-initialized.
	/// </summary>
	public virtual AbstractEmbarkBuilderModuleData DefaultData => null;

	/// <summary>
	///     XmlNodes used when parsing the modules file with <c>XmlDataHelper</c>
	/// </summary>
	public virtual Dictionary<string, Action<XmlDataHelper>> XmlNodes => new Dictionary<string, Action<XmlDataHelper>> { { "window", HandleWindowNode } };

	/// <summary>
	///     Nodes that exist for parsing XmlWindowNodes
	/// </summary>
	protected virtual Dictionary<string, Action<XmlDataHelper>> XmlWindowNodes => new Dictionary<string, Action<XmlDataHelper>>
	{
		{ "icon", HandleWindowIconNode },
		{ "name", HandleWindowNameNode },
		{ "title", HandleWindowTitleNode }
	};

	/// <summary>
	/// Called after the data has been replaced, when the data for a module is changing.
	/// </summary>
	public event Action<AbstractEmbarkBuilderModuleData, AbstractEmbarkBuilderModuleData> OnChange;

	/// <summary>
	///     Check if the modules data should be included in the build code.
	///     Defaults to module is enabled and has data.
	/// </summary>
	public virtual bool IncludeInBuildCodes()
	{
		if (enabled)
		{
			return getData() != null;
		}
		return false;
	}

	/// <summary>
	/// Return the name of your mod if you want to have a notice about required mod for the build code.
	/// Leave null to silently ignore this data when your module is not found.
	/// </summary>
	public virtual string GetRequiredMod()
	{
		return null;
	}

	/// <summary>
	///     Get the summary info block presented by this module.
	/// </summary>
	/// <returns>A SummaryBlockData or null.</returns>
	public virtual SummaryBlockData GetSummaryBlock()
	{
		return null;
	}

	public virtual bool shouldBeEnabled()
	{
		return enabled;
	}

	public virtual bool shouldBeEditable()
	{
		return true;
	}

	/// <summary>
	/// Return a list of window descriptors. Each descriptor defines a window to be displayed when the module is enabled.
	/// </summary>
	/// <param name="windows" />
	public virtual void assembleWindowDescriptors(List<EmbarkBuilderModuleWindowDescriptor> windows)
	{
		windows.AddRange(this.windows.Values);
	}

	/// <summary>
	/// Enable the module. OnEnabled() will be called if the window was not already enabled.
	/// </summary>
	public void enable()
	{
		if (!_enabled)
		{
			_enabled = true;
			OnEnabled();
		}
	}

	/// <summary>
	/// Called after enable() if the window was not already enabled.
	/// </summary>
	public virtual void OnEnabled()
	{
	}

	/// <summary>
	/// Disable the module. OnDisabled() will be caled if the window was not already disabled.
	/// </summary>
	public void disable()
	{
		if (_enabled)
		{
			_enabled = false;
			OnDisabled();
		}
	}

	/// <summary>
	/// Called after disable() if the window was not already disabled.
	/// </summary>
	public virtual void OnDisabled()
	{
	}

	/// <summary>
	/// Called when the data for any other module changes.
	/// Immediately after this call the module's shouldBeEnabled state will be checked and the module will be enabled or disabled appropriately.
	/// </summary>
	/// <param name="module" />
	/// <param name="oldValues" />
	/// <param name="newValues" />
	public virtual void handleModuleDataChange(AbstractEmbarkBuilderModule module, AbstractEmbarkBuilderModuleData oldValues, AbstractEmbarkBuilderModuleData newValues)
	{
	}

	/// <summary>
	/// Called before the data has been replaced, when the data for a module is changing.
	/// </summary>
	/// <param name="oldValues" />
	/// <param name="newValues" />
	public virtual void OnBeforeDataChange(AbstractEmbarkBuilderModuleData oldValues, AbstractEmbarkBuilderModuleData newValues)
	{
	}

	/// <summary>
	/// Called to perform the data replacement, when the data for a module is changing.
	/// You should probably call the base if you override this.
	/// </summary>
	/// <param name="oldValues" />
	/// <param name="newValues" />
	public virtual void OnDataChange(AbstractEmbarkBuilderModuleData oldValues, AbstractEmbarkBuilderModuleData newValues)
	{
		_values = newValues;
	}

	/// <summary>
	/// Called after the data has been replaced, when the data for a module is changing.
	/// </summary>
	/// <param name="oldValues" />
	/// <param name="newValues" />
	public virtual void OnAfterDataChange(AbstractEmbarkBuilderModuleData oldValues, AbstractEmbarkBuilderModuleData newValues)
	{
		builder.NotifyModuleChanges(this, oldValues, newValues);
	}

	/// <summary>
	/// Set the data for a module. OnBeforeDataChange, OnDataChange, OnChange and OnAfterDataChange will be called.
	/// </summary>
	/// <param name="values" />
	public virtual void setData(AbstractEmbarkBuilderModuleData values)
	{
		AbstractEmbarkBuilderModuleData values2 = _values;
		OnBeforeDataChange(values2, values);
		OnDataChange(values2, values);
		if (OnChange != null)
		{
			OnChange(values2, values);
		}
		OnAfterDataChange(values2, values);
	}

	/// <summary>
	/// Set the data for a module. OnBeforeDataChange, OnDataChange, OnChange and OnAfterDataChange WILL NOT be called.
	/// </summary>
	/// <param name="values" />
	public virtual void setDataDirect(AbstractEmbarkBuilderModuleData values)
	{
		_values = values;
	}

	/// <summary>
	/// Returns the typeof the data.
	/// </summary>
	/// <returns />
	public virtual Type getDataType()
	{
		return typeof(AbstractEmbarkBuilderModuleData);
	}

	/// <summary>
	/// Returns the module data.
	/// </summary>
	/// <returns />
	public virtual AbstractEmbarkBuilderModuleData getData()
	{
		return _values;
	}

	/// <summary>
	/// Called to determine if the module is well-configured. By default it is true only if both DataErrors and DataWarnings return null.
	/// </summary>
	/// <returns />
	public virtual bool IsDataValid()
	{
		if (DataErrors() == null)
		{
			return DataWarnings() == null;
		}
		return false;
	}

	/// <summary>
	/// A List of errors to print (one per line)
	/// </summary>
	/// <returns />
	public virtual string DataErrors()
	{
		if (getData() == null)
		{
			return "You must make a selection before advancing.";
		}
		return null;
	}

	/// <summary>
	/// A List of errors to print (one per line)
	/// </summary>
	/// <returns />
	public virtual string DataWarnings()
	{
		return null;
	}

	/// <summary>
	/// Get a Random sequence that is seeded with the module type and given seed.
	/// </summary>
	/// <param name="seed" />
	/// <returns />
	protected Random getModuleRngFromSeed(string seed)
	{
		return new Random(Hash.String(type + seed));
	}

	/// <summary>
	/// Given a seed, fully initialize your modules values to a valid configuration.
	/// </summary>
	/// <param name="seed" />
	public virtual void InitFromSeed(string seed)
	{
	}

	/// <summary>
	/// Initialize your modules values to a random valid configuration.
	/// </summary>
	public virtual void RandomSelection()
	{
	}

	public virtual void RandomSelectionNoUI()
	{
	}

	/// <summary>
	/// Set the value to DefaultData.
	/// </summary>
	public virtual void ResetSelection()
	{
		setData(DefaultData);
	}

	/// <summary>
	/// Perform initialization before the UI is configured.
	/// </summary>
	public virtual void Init()
	{
	}

	/// <summary>
	/// Perform module specific game boot behavior.
	/// </summary>
	/// <param name="game" />
	/// <param name="info" />
	public virtual void bootGame(XRLGame game, EmbarkInfo info)
	{
	}

	/// <summary>
	/// Called for any UI event. If no changes or updates to an element are performed, the element should be returned.
	/// </summary>
	/// <param name="ID" />
	/// <param name="Element" />
	/// <returns />
	public virtual object handleUIEvent(string ID, object Element)
	{
		return Element;
	}

	/// <summary>
	/// Called for any game boot event. If no changes or updates to an element are performed, the element should be returned.
	/// </summary>
	/// <param name="id" />
	/// <param name="game" />
	/// <param name="info" />
	/// <param name="element" />
	/// <returns />
	public virtual object handleBootEvent(string id, XRLGame game, EmbarkInfo info, object element = null)
	{
		return element;
	}

	/// <summary>
	/// Behavior when advancing to the next screen. By default builder.advance() is called.
	/// </summary>
	public virtual void onNext()
	{
		builder.advance();
	}

	/// <summary>
	/// Behavior when returning to the previous screen. By default builder.back() is called.
	/// </summary>
	public virtual void onBack()
	{
		builder.back();
	}

	/// <summary>
	///     Parse the XML configuration for this module.
	/// </summary>
	/// <param name="xml">Handle to xml to parse</param>
	public virtual void HandleNodes(XmlDataHelper xml)
	{
		xml.HandleNodes(XmlNodes);
	}

	/// <summary>
	///     Parses a &lt;window ID="key"&gt; xml node, adding it to the dictionary.
	///     Uses a new Guid string if no ID is present(worst case, not reusable).
	///     Checks "Class" attribute and looks for the UI type.
	/// </summary>
	/// <param name="xml">A handle the XML window node.</param>
	public virtual void HandleWindowNode(XmlDataHelper xml)
	{
		string text = xml.GetAttribute("ID") ?? Guid.NewGuid().ToString();
		if (!windows.TryGetValue(text, out CurrentLoadingWindowDescriptor))
		{
			CurrentLoadingWindowDescriptor = new EmbarkBuilderModuleWindowDescriptor(this);
			CurrentLoadingWindowDescriptor.viewID = text;
			windows.Set(text, CurrentLoadingWindowDescriptor);
		}
		CurrentLoadingWindowDescriptor.prefab = xml.GetAttributeString("Prefab", CurrentLoadingWindowDescriptor.prefab);
		try
		{
			string attributeString = xml.GetAttributeString("Class", null);
			Type type = ModManager.ResolveType(attributeString);
			if (type == null)
			{
				xml.ParseWarning("Unresolved type " + attributeString);
			}
			if (type != null)
			{
				CurrentLoadingWindowDescriptor.windowType = type;
			}
		}
		catch
		{
			xml.ParseWarning("Error finding Class");
		}
		xml.HandleNodes(XmlWindowNodes);
		CurrentLoadingWindowDescriptor = null;
	}

	public virtual void HandleWindowIconNode(XmlDataHelper xml)
	{
		CurrentLoadingWindowDescriptor.tile = xml.GetAttribute("Tile");
		xml.DoneWithElement();
	}

	public virtual void HandleWindowNameNode(XmlDataHelper xml)
	{
		CurrentLoadingWindowDescriptor.name = xml.GetTextNode();
	}

	public virtual void HandleWindowTitleNode(XmlDataHelper xml)
	{
		CurrentLoadingWindowDescriptor.title = xml.GetTextNode();
	}
}
