using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using XRL.UI;
using XRL.UI.Framework;

namespace Qud.UI;

public class CharacterMutationLine : MonoBehaviour, IFrameworkControl, IFrameworkControlSubcontexts, IPointerClickHandler, IEventSystemHandler
{
	public class Context : NavigationContext
	{
		public CharacterMutationLineData data;
	}

	private Context context = new Context();

	public UITextSkin text;

	public StringBuilder SB = new StringBuilder();

	public static List<MenuOption> categoryExpandOptions = new List<MenuOption>
	{
		new MenuOption
		{
			Id = "Accept",
			InputCommand = "Accept",
			Description = "Expand"
		}
	};

	public static List<MenuOption> categoryCollapseOptions = new List<MenuOption>
	{
		new MenuOption
		{
			Id = "Accept",
			InputCommand = "Accept",
			Description = "Collapse"
		}
	};

	public bool selected => context?.IsActive() ?? false;

	public NavigationContext GetNavigationContext()
	{
		return context;
	}

	public void SetupContexts(ScrollChildContext scrollContext)
	{
		scrollContext.proxyTo = this.context;
		Context context = this.context;
		if (context.axisHandlers == null)
		{
			context.axisHandlers = new Dictionary<InputAxisTypes, Action>();
		}
		context = this.context;
		if (context.commandHandlers == null)
		{
			context.commandHandlers = new Dictionary<string, Action>();
		}
	}

	public void setData(FrameworkDataElement data)
	{
		if (!(data is CharacterMutationLineData characterMutationLineData))
		{
			return;
		}
		context.data = characterMutationLineData;
		if (!characterMutationLineData.mutation.ShouldShowLevel())
		{
			text.SetText("{{y|" + characterMutationLineData.mutation.GetDisplayName() + "}}");
			return;
		}
		string arg = "C";
		if (characterMutationLineData.mutation.Level > characterMutationLineData.mutation.BaseLevel)
		{
			arg = ((characterMutationLineData.mutation.Level <= characterMutationLineData.mutation.GetMutationCap()) ? "G" : "M");
		}
		else if (characterMutationLineData.mutation.Level < characterMutationLineData.mutation.BaseLevel)
		{
			arg = "R";
		}
		text.SetText($"{{{{y|{characterMutationLineData.mutation.GetDisplayName()} ({{{{{arg}|{characterMutationLineData.mutation.GetUIDisplayLevel()}}}}})}}}}");
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		context.IsActive();
	}
}
