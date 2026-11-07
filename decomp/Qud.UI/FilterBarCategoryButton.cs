using System;
using System.Collections.Generic;
using System.Text;
using Kobold;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XRL.UI;
using XRL.UI.Framework;

namespace Qud.UI;

public class FilterBarCategoryButton : FrameworkUnityScrollChild, IFrameworkControl, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
	public class Context : ScrollChildContext
	{
		public FilterBarCategoryButtonData data;
	}

	private Context _context = new Context();

	public UITextSkin check;

	public UITextSkin hotkey;

	public GameObject rightFloatSpacer;

	public UITextSkin rightFloatText;

	public UIThreeColorProperties icon;

	public UITextSkin text;

	public GameObject iconSpacer;

	public TradeScreen screen;

	public StringBuilder SB = new StringBuilder();

	public static Dictionary<string, string> categoryImageMap = new Dictionary<string, string>
	{
		{ "Light Sources", "Items/sw_torch_lit.png" },
		{ "Melee Weapons", "Items/sw_longblade_14.bmp" },
		{ "Thrown Weapons", "Items/sw_throwing_axe.bmp" },
		{ "Miscellaneous", "Items/sw_hookah.bmp" },
		{ "Food", "Items/sw_apple.bmp" },
		{ "Corpses", "Items/sw_splat1.bmp" },
		{ "Plants", "Creatures/natural-weapon-frond.bmp" },
		{ "Missile Weapons", "Items/sw_revolver.bmp" },
		{ "Projectiles", "Items/sw_arrow.bmp" },
		{ "Ammo", "Items/sw_arrow.bmp" },
		{ "Armor", "Items/sw_leather_armor.bmp" },
		{ "Shields", "Items/sw_shield5.bmp" },
		{ "Grenades", "Items/sw_grenade_mki.bmp" },
		{ "Creatures", "Creatures/sw_snapjaw.bmp" },
		{ "Applicators", "Items/sw_spray.bmp" },
		{ "Energy Cells", "Items/sw_cell_chem.bmp" },
		{ "Natural Weapons", "Creatures/natural-weapon-claw.bmp" },
		{ "Natural Missile Weapons", "Creatures/natural-weapon-arc.bmp" },
		{ "Natural Missile Weapon", "Creatures/natural-weapon-arc.bmp" },
		{ "Natural Armor", "Items/sw_carapace.bmp" },
		{ "Meds", "Items/sw_bandage.bmp" },
		{ "Tonics", "Items/sw_injector.bmp" },
		{ "Water Containers", "Items/sw_waterskin.bmp" },
		{ "Books", "Items/sw_book1.bmp" },
		{ "Tools", "Tiles/sw_box.bmp" },
		{ "Artifacts", "Items/sw_gadget.bmp" },
		{ "Clothes", "Items/sw_cloak.bmp" },
		{ "Trade Goods", "Items/sw_rough_gemstone.bmp" },
		{ "Quest Items", "Items/sw_key.bmp" },
		{ "Data Disks", "items/sw_data_disc.bmp" },
		{ "Scrap", "Items/bit11.bmp" },
		{ "Trinkets", "Items/unidentified_artifact_trinket.bmp" },
		{ "Cybernetic Implants", "Items/sw_carbidehand.bmp" },
		{ "CommonMods", "Items/sw_vase.bmp" },
		{ "WeaponMods", "items/sw_axe_2.bmp" },
		{ "BladeMods", "Items/sw_shortblade_1.bmp" },
		{ "LongBladeMods", "Items/sw_longblade_14.bmp" },
		{ "CudgelMods", "Items/sw_cudgel_1.bmp" },
		{ "AxeMods", "Items/sw_axe_3.bmp" },
		{ "ThrownWeaponMods", "Items/sw_throwing_axe.bmp" },
		{ "GrenadeMods", "Items/sw_grenade_mki.bmp" },
		{ "MissileWeaponMods", "items/sw_flamethrower.bmp" },
		{ "BowMods", "items/sw_bow_1.bmp" },
		{ "FirearmMods", "Items/sw_musket.bmp" },
		{ "RifleMods", "Items/sw_rifle.bmp" },
		{ "PistolMods", "items/sw_revolver.bmp" },
		{ "BeamWeaponMods", "Items/sw_techrifle_1.bmp" },
		{ "MagazineMods", "items/sw_bullet.bmp" },
		{ "BodyMods", "Items/sw_leather_armor.bmp" },
		{ "CloakMods", "Items/sw_cloak.bmp" },
		{ "MaskMods", "Items/sw_gentlingmask.bmp" },
		{ "BootMods", "Items/sw_leather_boots.bmp" },
		{ "GauntletMods", "Items/sw_metal_gloves.bmp" },
		{ "HeadwearMods", "Items/sw_hat.bmp" },
		{ "HelmetMods", "Items/sw_metal_helmet.bmp" },
		{ "EyewearMods", "Items/sw_goggles.bmp" },
		{ "ShieldMods", "Items/sw_shield5.bmp" },
		{ "GloveMods", "Items/sw_metal_gloves.bmp" },
		{ "BracerMods", "Items/sw_leather_bracers.bmp" },
		{ "WingsMods", "items/sw_mechanical_wings.bmp" },
		{ "ExoskeletonMods", "Items/sw_exoskeleton.bmp" },
		{ "EnergyCellMods", "Items/sw_cell_chem.bmp" },
		{ "ElectronicsMods", "Items/bit11.bmp" },
		{ "Locations", "Terrain/sw_joppa.bmp" },
		{ "Gossip and Lore", "creatures/caste_13.bmp" },
		{ "Sultan Histories", "Terrain/sw_resheph_sultanstatue.bmp" },
		{ "Village Histories", "Terrain/sw_monument5.bmp" },
		{ "Chronology", "Items/sw_clockthing.bmp" },
		{ "General Notes", "Items/sw_crayons.bmp" },
		{ "Recipes", "Items/sw_oven.bmp" }
	};

	public static Dictionary<string, string> categoryTextMap = new Dictionary<string, string> { { "*All", "ALL" } };

	public string Tooltip;

	public GameObject tooltipObject;

	public UITextSkin tooltipText;

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

	public static List<MenuOption> itemOptions = new List<MenuOption>
	{
		new MenuOption
		{
			Id = "Accept",
			InputCommand = "Accept",
			Description = "Select"
		}
	};

	public Image background;

	public bool wasSelected;

	public bool wasEnabled;

	public bool categoryEnabled;

	private List<AbilityManagerSpacer> spacers = new List<AbilityManagerSpacer>();

	public string category;

	private bool inside;

	public bool selected => GetNavigationContext()?.IsActive() ?? false;

	public NavigationContext GetNavigationContext()
	{
		return context;
	}

	public void Awake()
	{
		if (tooltipObject != null)
		{
			tooltipObject.SetActive(value: false);
		}
	}

	public void Update()
	{
		if (tooltipObject != null && Tooltip != null)
		{
			bool flag = selected;
			if (tooltipObject.activeSelf != flag)
			{
				tooltipObject.SetActive(flag);
			}
		}
	}

	public void SetCategory(string category, string tooltip = null)
	{
		Tooltip = tooltip ?? category;
		this.category = category;
		tooltipText?.SetText(Tooltip);
		string value = null;
		categoryImageMap.TryGetValue(category, out value);
		if (value != null)
		{
			icon.gameObject.SetActive(value: true);
			if (categoryImageMap.ContainsKey(category))
			{
				value = categoryImageMap[category];
			}
			icon.image.sprite = SpriteManager.GetUnitySprite(value);
			icon.SetColors(new Color(0.596f, 0.529f, 0.372f), new Color(0.545f, 0.4f, 0.18f), new Color(0f, 0f, 0f, 0f));
		}
		else
		{
			icon.gameObject.SetActive(value: false);
		}
		string value2 = null;
		categoryTextMap.TryGetValue(category, out value2);
		if (value == null && value2 == null)
		{
			value2 = category;
		}
		if (value2 != null)
		{
			text.SetText(value2);
			text.gameObject.SetActive(value: true);
		}
		else
		{
			text.gameObject.SetActive(value: false);
		}
	}

	public void HandleNavigationVAxis()
	{
		screen.HandleVAxis(NavigationController.currentEvent.axisValue);
		NavigationController.currentEvent.Handle();
	}

	public void HandleNavigationXAxis()
	{
		screen.HandleXAxis(NavigationController.currentEvent.axisValue);
		NavigationController.currentEvent.Handle();
	}

	public void LateUpdate()
	{
		if ((selected != wasSelected || categoryEnabled != wasEnabled) && background != null)
		{
			Color color;
			if (categoryEnabled)
			{
				if (selected)
				{
					ColorUtility.TryParseHtmlString("#FFFFFF", out color);
				}
				else
				{
					ColorUtility.TryParseHtmlString("#858951", out color);
				}
			}
			else if (selected)
			{
				ColorUtility.TryParseHtmlString("#4A757E", out color);
			}
			else
			{
				ColorUtility.TryParseHtmlString("#134F4E", out color);
			}
			background.color = color;
		}
		wasSelected = selected;
		wasEnabled = categoryEnabled;
	}

	public void setData(FrameworkDataElement data)
	{
		if (data is FilterBarCategoryButtonData filterBarCategoryButtonData)
		{
			_context.data = filterBarCategoryButtonData;
			SetCategory(filterBarCategoryButtonData.category, filterBarCategoryButtonData.tooltip);
			filterBarCategoryButtonData.button = this;
		}
		NavigationContext navigationContext = context;
		if (navigationContext.commandHandlers == null)
		{
			navigationContext.commandHandlers = new Dictionary<string, Action>();
		}
		context.commandHandlers["Accept"] = HandleSelect;
	}

	public void FiltersUpdated(FilterBar bar)
	{
		categoryEnabled = bar.enabledCategories.Contains(category);
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
	}

	public int HighlightClosestSpacer(Vector2 screenPosition)
	{
		AbilityManagerSpacer abilityManagerSpacer = ((spacers.Count > 0) ? spacers[0] : null);
		float num = float.MaxValue;
		int result = -1;
		Vector3[] array = new Vector3[4];
		int num2 = 0;
		foreach (AbilityManagerSpacer spacer in spacers)
		{
			(spacer.transform as RectTransform).GetWorldCorners(array);
			float magnitude = (new Vector2((array[0].x + array[2].x) / 2f, (array[0].y + array[2].y) / 2f) - screenPosition).magnitude;
			if (magnitude < num)
			{
				abilityManagerSpacer = spacer;
				num = magnitude;
				result = num2;
			}
			spacer.image.enabled = false;
			num2++;
		}
		if (abilityManagerSpacer != null)
		{
			abilityManagerSpacer.image.enabled = true;
		}
		return result;
	}

	public void OnDrag(PointerEventData eventData)
	{
	}

	public void OnEndDrag(PointerEventData eventData)
	{
	}

	public void HandleSelect()
	{
		_context.data?.onSelect(category);
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (GetNavigationContext().IsActive() && eventData.button == PointerEventData.InputButton.Left)
		{
			HandleSelect();
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		inside = true;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		inside = false;
	}
}
