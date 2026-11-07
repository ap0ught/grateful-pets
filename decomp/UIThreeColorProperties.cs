using ConsoleLib.Console;
using Kobold;
using UnityEngine;
using UnityEngine.UI;
using XRL.World;

public class UIThreeColorProperties : MonoBehaviour
{
	[SerializeField]
	private Color _Foreground = Color.white;

	[SerializeField]
	private Color _Detail = Color.black;

	[SerializeField]
	private Color _Background = new Color(0f, 0f, 0f, 0f);

	public Image image;

	public bool Updated;

	public Color _currentForeground = new Color(1f, 2f, 3f, 4f);

	public Color _currentDetail = new Color(1f, 2f, 3f, 4f);

	public Color _currentBackground = new Color(1f, 2f, 3f, 4f);

	private static long val;

	private bool set;

	public Color Foreground
	{
		get
		{
			return _Foreground;
		}
		set
		{
			if (_Foreground != value)
			{
				_Foreground = value;
				UpdateColors();
			}
		}
	}

	public Color Detail
	{
		get
		{
			return _Detail;
		}
		set
		{
			if (_Detail != value)
			{
				_Detail = value;
				UpdateColors();
			}
		}
	}

	public Color Background
	{
		get
		{
			return _Background;
		}
		set
		{
			if (_Background != value)
			{
				_Background = value;
				UpdateColors();
			}
		}
	}

	public void SetColors(Color F, Color D, Color B)
	{
		_Foreground = F;
		_Detail = D;
		_Background = B;
		UpdateColors();
	}

	public void FromConsoleChar(ConsoleChar c)
	{
		if (c.Char == '\0')
		{
			image.sprite = SpriteManager.GetUnitySprite(c.Tile);
			SetColors(c.Foreground, c.Detail, c.Background);
		}
		else
		{
			image.sprite = SpriteManager.GetUnitySprite("Text/" + (int)c.Char + ".bmp");
			SetColors(c.Background, c.Foreground, c.Foreground);
		}
	}

	public void FromRenderEvent(RenderEvent e)
	{
		FromRenderable(e);
	}

	public void SetHFlip(bool Value)
	{
		Transform transform = image.transform;
		Vector3 localScale = transform.localScale;
		if (Value)
		{
			transform.localScale = new Vector3(Mathf.Abs(localScale.x) * -1f, localScale.y, localScale.z);
		}
		else
		{
			transform.localScale = new Vector3(Mathf.Abs(localScale.x), localScale.y, localScale.z);
		}
	}

	public void SetVFlip(bool Value)
	{
		Transform transform = image.transform;
		Vector3 localScale = transform.localScale;
		if (Value)
		{
			transform.localScale = new Vector3(localScale.x, Mathf.Abs(localScale.y) * -1f, localScale.z);
		}
		else
		{
			transform.localScale = new Vector3(localScale.x, Mathf.Abs(localScale.y), localScale.z);
		}
	}

	public void FromRenderable(IRenderable e, bool transparentBlackBackgrounds = true)
	{
		if (e == null || (e.getTile() == null && e.getRenderString() == null))
		{
			SetColors(Color.clear, Color.clear, Color.clear);
			image.transform.localScale = new Vector3(Mathf.Abs(image.transform.localScale.x), Mathf.Abs(image.transform.localScale.y), image.transform.localScale.z);
			return;
		}
		SetHFlip(e.getHFlip());
		SetVFlip(e.getVFlip());
		ColorChars value = (e?.getColorChars()).Value;
		char c = value.background;
		if (transparentBlackBackgrounds && value.background == 'k')
		{
			c = '\0';
		}
		if (e != null && e.getTile() != null)
		{
			image.sprite = SpriteManager.GetUnitySprite(e.getTile());
			SetColors(ConsoleLib.Console.ColorUtility.colorFromChar(value.foreground), ConsoleLib.Console.ColorUtility.colorFromChar(value.detail), ConsoleLib.Console.ColorUtility.colorFromChar(c));
		}
		else if (e != null && e.getRenderString() != null)
		{
			image.sprite = SpriteManager.GetUnitySprite("Text/" + (int)e.getRenderString()[0] + ".bmp");
			SetColors(ConsoleLib.Console.ColorUtility.colorFromChar(value.background), ConsoleLib.Console.ColorUtility.colorFromChar(value.foreground), ConsoleLib.Console.ColorUtility.colorFromChar(c));
		}
		else
		{
			MetricsManager.LogWarning("What is this render" + e);
		}
	}

	private void UpdateColors()
	{
		if ((_Foreground == _currentForeground && _Background == _currentBackground && _Detail == _currentDetail) || image == null)
		{
			return;
		}
		if (!set)
		{
			if (image.material == null || !image.material.name.StartsWith("UI Tile Material"))
			{
				image.material = Resources.Load<Material>("Materials/UI Tile Material");
			}
			Material material = Object.Instantiate(image.material);
			material.name = $"UI Tile Material ({++val})";
			image.material = material;
			set = true;
		}
		image.material.SetColor("_Foreground", _Foreground);
		image.material.SetColor("_Detail", _Detail);
		image.material.SetColor("_Background", _Background);
		image.enabled = false;
		image.enabled = true;
		_currentForeground = _Foreground;
		_currentBackground = _Background;
		_currentDetail = _Detail;
	}

	private void Awake()
	{
		UpdateColors();
	}

	private void Start()
	{
	}

	private void Update()
	{
	}
}
