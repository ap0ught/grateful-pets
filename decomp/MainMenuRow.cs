using UnityEngine;
using UnityEngine.UI;
using XRL;
using XRL.UI.Framework;

public class MainMenuRow : MonoBehaviour, IFrameworkControl
{
	public Image alert;

	public Text text;

	private FrameworkContext _context;

	public MainMenuOptionData.AlertMode alertMode;

	public bool buttonenabled;

	public MainMenuOptionData data;

	private bool? wasSelected;

	private Color orig = Color.magenta;

	public FrameworkContext context => _context ?? (_context = GetComponent<FrameworkContext>());

	public void setData(FrameworkDataElement data)
	{
		this.data = null;
		if (data is MainMenuOptionData mainMenuOptionData)
		{
			this.data = mainMenuOptionData;
			buttonenabled = mainMenuOptionData.Enabled;
			text.text = mainMenuOptionData.Text;
			alertMode = mainMenuOptionData.Alert;
			wasSelected = null;
			Update();
		}
	}

	public void Update()
	{
		if (orig == Color.magenta)
		{
			orig = text.color;
		}
		bool valueOrDefault = context?.context?.IsActive() == true;
		if (valueOrDefault != wasSelected)
		{
			wasSelected = valueOrDefault;
			float num = (data.Enabled ? 1f : 0.6f);
			if (valueOrDefault)
			{
				text.color = new Color(orig.r * num, orig.g * num, orig.b * num, 1f);
			}
			else
			{
				text.color = new Color(orig.r * num, orig.g * num, orig.b * num, 0.35f);
			}
		}
		if (alertMode != MainMenuOptionData.AlertMode.ModStatus || !(alert != null))
		{
			return;
		}
		if (ModManager.IsAnyModFailed())
		{
			if (!alert.gameObject.activeInHierarchy)
			{
				alert.gameObject.SetActive(value: true);
				alert.color = Color.red;
			}
		}
		else if (ModManager.IsAnyModMissingDependency())
		{
			if (!alert.gameObject.activeInHierarchy)
			{
				alert.gameObject.SetActive(value: true);
				alert.color = Color.yellow;
			}
		}
		else if (ModManager.IsScriptingUndetermined())
		{
			if (!alert.gameObject.activeInHierarchy)
			{
				alert.gameObject.SetActive(value: true);
				alert.color = Color.white;
			}
		}
		else if (alert.gameObject.activeInHierarchy)
		{
			alert.gameObject.SetActive(value: false);
		}
	}

	public NavigationContext GetNavigationContext()
	{
		return null;
	}
}
