using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using XRL.UI;
using XRL.UI.Framework;

namespace Qud.UI;

[ExecuteInEditMode]
public class KeybindRow : MonoBehaviour, IFrameworkControl, IFrameworkControlSubcontexts
{
	public enum SubContextTypes
	{
		box1,
		box2,
		box3,
		box4,
		category
	}

	public class KeybindRowSubContext : ScrollChildContext
	{
		public SubContextTypes contextType;

		public KeybindRowSubContext(SubContextTypes contextType)
		{
			this.contextType = contextType;
		}
	}

	public Image background;

	public GameObject bindingDisplay;

	public GameObject categoryDisplay;

	public UITextSkin categoryDescription;

	public UITextSkin categoryExpander;

	public LayoutElement layoutElement;

	public UITextSkin description;

	public KeybindBox box1;

	public KeybindBox box2;

	public KeybindBox box3;

	public KeybindBox box4;

	public ScrollContext<int, KeybindRowSubContext> subscroller;

	public NavigationContext categoryContext;

	public FrameworkContext frameworkContext;

	public bool selectedMode;

	public KeybindDataRow dataRow = new KeybindDataRow
	{
		KeyDescription = "Interact Nearby",
		Bind1 = "Ctrl+Space",
		Bind2 = null
	};

	public KeybindCategoryRow categoryRow;

	public bool editorUpdateDataRow;

	public UnityEvent<KeybindDataRow, int, KeybindRow> onRebind = new UnityEvent<KeybindDataRow, int, KeybindRow>();

	private const string NO_BIND_DISPLAY = "{{K|None}}";

	private bool? wasSelected;

	public void SetupContexts(ScrollChildContext scontext)
	{
		if (categoryContext?.parentContext != scontext || subscroller?.parentContext != scontext)
		{
			categoryContext = null;
			subscroller = null;
			box1.context.context = null;
			box2.context.context = null;
			box3.context.context = null;
			box4.context.context = null;
			if (scontext.IsActive() && NavigationController.instance.activeContext is KeybindRowSubContext keybindRowSubContext)
			{
				if (keybindRowSubContext.contextType == SubContextTypes.box1 || keybindRowSubContext.contextType == SubContextTypes.box2 || keybindRowSubContext.contextType == SubContextTypes.box3 || keybindRowSubContext.contextType == SubContextTypes.box4)
				{
					subscroller = keybindRowSubContext.parentContext as ScrollContext<int, KeybindRowSubContext>;
					box1.context.context = subscroller.contexts[0];
					box2.context.context = subscroller.contexts[1];
					box3.context.context = subscroller.contexts[2];
					box4.context.context = subscroller.contexts[3];
				}
				if (keybindRowSubContext.contextType == SubContextTypes.category)
				{
					categoryContext = keybindRowSubContext;
				}
			}
		}
		if (categoryContext == null)
		{
			categoryContext = new KeybindRowSubContext(SubContextTypes.category)
			{
				parentContext = scontext
			};
		}
		if (subscroller == null)
		{
			subscroller = new ScrollContext<int, KeybindRowSubContext>
			{
				parentContext = scontext
			};
			box1.context.context = new KeybindRowSubContext(SubContextTypes.box1);
			box1.context.context.buttonHandlers = new Dictionary<InputButtonTypes, Action> { 
			{
				InputButtonTypes.AcceptButton,
				XRL.UI.Framework.Event.Helpers.Handle(() =>
				{
					onRebind?.Invoke(dataRow, 0, this);
				})
			} };
			box2.context.context = new KeybindRowSubContext(SubContextTypes.box2);
			box2.context.context.buttonHandlers = new Dictionary<InputButtonTypes, Action> { 
			{
				InputButtonTypes.AcceptButton,
				XRL.UI.Framework.Event.Helpers.Handle(() =>
				{
					onRebind?.Invoke(dataRow, 1, this);
				})
			} };
			box3.context.context = new KeybindRowSubContext(SubContextTypes.box3);
			box3.context.context.buttonHandlers = new Dictionary<InputButtonTypes, Action> { 
			{
				InputButtonTypes.AcceptButton,
				XRL.UI.Framework.Event.Helpers.Handle(() =>
				{
					onRebind?.Invoke(dataRow, 2, this);
				})
			} };
			box4.context.context = new KeybindRowSubContext(SubContextTypes.box4);
			box4.context.context.buttonHandlers = new Dictionary<InputButtonTypes, Action> { 
			{
				InputButtonTypes.AcceptButton,
				XRL.UI.Framework.Event.Helpers.Handle(() =>
				{
					onRebind?.Invoke(dataRow, 3, this);
				})
			} };
			subscroller.SetAxis(InputAxisTypes.NavigationXAxis);
			subscroller.wraps = false;
			subscroller.data.Add(0);
			subscroller.data.Add(1);
			subscroller.data.Add(2);
			subscroller.data.Add(3);
			subscroller.contexts.Add(box1.context.context as KeybindRowSubContext);
			subscroller.contexts.Add(box2.context.context as KeybindRowSubContext);
			subscroller.contexts.Add(box3.context.context as KeybindRowSubContext);
			subscroller.contexts.Add(box4.context.context as KeybindRowSubContext);
		}
		if (categoryRow != null)
		{
			scontext.proxyTo = categoryContext;
		}
		else
		{
			scontext.proxyTo = subscroller;
			box1.context.context.disabled = !box1.gameObject.activeInHierarchy;
			box2.context.context.disabled = !box2.gameObject.activeInHierarchy;
			box3.context.context.disabled = !box3.gameObject.activeInHierarchy;
			box4.context.context.disabled = !box4.gameObject.activeInHierarchy;
		}
		scontext.Setup();
	}

	public NavigationContext GetNavigationContext()
	{
		return frameworkContext.context;
	}

	public void setData(FrameworkDataElement data)
	{
		if (data is KeybindDataRow keybindDataRow)
		{
			categoryDisplay.SetActive(value: false);
			bindingDisplay.SetActive(value: true);
			categoryRow = null;
			dataRow = keybindDataRow;
			description.text = "{{C|" + keybindDataRow.KeyDescription + "}}";
			description.Apply();
			layoutElement.preferredHeight = 25 * (1 + keybindDataRow.KeyDescription.Length / 26);
			layoutElement.minHeight = 25 * (1 + keybindDataRow.KeyDescription.Length / 26);
			if (string.IsNullOrEmpty(keybindDataRow.Bind1))
			{
				box1.boxText = "{{K|None}}";
				box2.boxText = "{{K|None}}";
				box3.boxText = "{{K|None}}";
				box4.boxText = "{{K|None}}";
			}
			else
			{
				box1.boxText = "{{w|" + keybindDataRow.Bind1 + "}}";
				box2.gameObject.SetActive(value: true);
				if (string.IsNullOrEmpty(keybindDataRow.Bind2))
				{
					box2.boxText = "{{K|None}}";
					box3.boxText = "{{K|None}}";
					box4.boxText = "{{K|None}}";
				}
				else
				{
					box2.boxText = "{{w|" + keybindDataRow.Bind2 + "}}";
					box3.gameObject.SetActive(value: true);
					if (string.IsNullOrEmpty(keybindDataRow.Bind3))
					{
						box3.boxText = "{{K|None}}";
						box4.boxText = "{{K|None}}";
					}
					else
					{
						box3.boxText = "{{w|" + keybindDataRow.Bind3 + "}}";
						box4.gameObject.SetActive(value: true);
						box4.boxText = (string.IsNullOrEmpty(keybindDataRow.Bind4) ? "{{K|None}}" : ("{{w|" + keybindDataRow.Bind4 + "}}"));
					}
				}
			}
			box1.forceUpdate = (box2.forceUpdate = (box3.forceUpdate = (box4.forceUpdate = true)));
			bool flag = SingletonWindowBase<KeybindsScreen>.instance.currentControllerType == ControlManager.InputDeviceType.Keyboard;
			if (flag)
			{
				(box1.transform as RectTransform).sizeDelta = new Vector2(-648f, 0f);
				(box1.transform as RectTransform).localPosition = new Vector2(-57f, 0f);
				(box1.transform as RectTransform).anchoredPosition = new Vector2(-57f, 0f);
				(box2.transform as RectTransform).sizeDelta = new Vector2(-648f, 0f);
				(box2.transform as RectTransform).localPosition = new Vector2(70f, 0f);
				(box2.transform as RectTransform).anchoredPosition = new Vector2(70f, 0f);
			}
			else
			{
				(box1.transform as RectTransform).sizeDelta = new Vector2(-534f, 0f);
				(box1.transform as RectTransform).localPosition = new Vector2(0f, 0f);
				(box1.transform as RectTransform).anchoredPosition = new Vector2(0f, 0f);
				(box2.transform as RectTransform).sizeDelta = new Vector2(-534f, 0f);
				(box2.transform as RectTransform).localPosition = new Vector2(230f, 0f);
				(box2.transform as RectTransform).anchoredPosition = new Vector2(230f, 0f);
			}
			box3.gameObject.SetActive(flag);
			box4.gameObject.SetActive(flag);
		}
		else if (data is KeybindCategoryRow keybindCategoryRow)
		{
			layoutElement.preferredHeight = 25f;
			layoutElement.minHeight = 25f;
			categoryDisplay.SetActive(value: true);
			bindingDisplay.SetActive(value: false);
			categoryRow = keybindCategoryRow;
			dataRow = null;
			categoryDescription.text = "{{C|" + categoryRow.CategoryDescription.ToUpper() + "}}";
			categoryDescription.Apply();
			if (keybindCategoryRow.Collapsed)
			{
				categoryExpander.SetText("{{C|[+]}}");
			}
			else
			{
				categoryExpander.SetText("{{C|[-]}}");
			}
		}
		GetNavigationContext();
	}

	public void Update()
	{
		if (editorUpdateDataRow)
		{
			editorUpdateDataRow = false;
			if (dataRow != null && !string.IsNullOrEmpty(dataRow.KeyDescription))
			{
				setData(dataRow);
			}
			else if (categoryRow != null)
			{
				setData(categoryRow);
			}
		}
		bool? flag = GetNavigationContext()?.IsActive();
		if (wasSelected == flag)
		{
			return;
		}
		wasSelected = flag;
		selectedMode = flag == true;
		if (selectedMode)
		{
			if (background.color.a != 0.2470588f)
			{
				background.color = new Color(background.color.r, background.color.g, background.color.b, 0.2470588f);
			}
		}
		else if (background.color.a != 0f)
		{
			background.color = new Color(background.color.r, background.color.g, background.color.b, 0f);
		}
	}
}
