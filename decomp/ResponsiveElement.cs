using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XRL.UI;

public class ResponsiveElement : MonoBehaviour
{
	public Media.SizeClass MinimumClass;

	public Media.SizeClass MaximumClass;

	public bool controlTMPFontSize;

	public int smallTMPSize;

	public int mediumTMPSize;

	public int largeTMPSize;

	private TextMeshProUGUI _tmp;

	public bool controlImage;

	public Image _image;

	public bool controlCanvas;

	public Canvas _canvas;

	public bool controlLayoutGroup;

	public LayoutElement _layoutElement;

	private Media.SizeClass lastClass = Media.SizeClass.Unset;

	public TextMeshProUGUI tmp
	{
		get
		{
			if (_tmp == null)
			{
				_tmp = gameObject.GetComponent<TextMeshProUGUI>();
			}
			return _tmp;
		}
	}

	public Image image
	{
		get
		{
			if (_image == null)
			{
				_image = gameObject.GetComponent<Image>();
			}
			if (_image == null && controlImage)
			{
				_image = gameObject.AddComponent<Image>();
			}
			return _image;
		}
	}

	public Canvas canvas
	{
		get
		{
			if (_canvas == null)
			{
				_canvas = gameObject.GetComponent<Canvas>();
			}
			if (_canvas == null && controlCanvas)
			{
				_canvas = gameObject.AddComponent<Canvas>();
			}
			return _canvas;
		}
	}

	public LayoutElement layoutElement
	{
		get
		{
			if (_layoutElement == null)
			{
				_layoutElement = gameObject.GetComponent<LayoutElement>();
			}
			if (_layoutElement == null && controlLayoutGroup)
			{
				_layoutElement = gameObject.AddComponent<LayoutElement>();
			}
			return _layoutElement;
		}
	}

	private void Check()
	{
		if (lastClass == Media.sizeClass)
		{
			return;
		}
		lastClass = Media.sizeClass;
		if ((MinimumClass == Media.SizeClass.Unset || Media.sizeClass >= MinimumClass) && (MaximumClass == Media.SizeClass.Unset || Media.sizeClass <= MaximumClass))
		{
			if (controlImage && !image.enabled)
			{
				image.enabled = true;
			}
			if (controlCanvas && !canvas.enabled)
			{
				canvas.enabled = true;
			}
			if (controlLayoutGroup && layoutElement.ignoreLayout)
			{
				layoutElement.ignoreLayout = false;
			}
			if (controlTMPFontSize)
			{
				if (Media.sizeClass <= Media.SizeClass.Small)
				{
					tmp.fontSize = smallTMPSize;
				}
				else if (Media.sizeClass == Media.SizeClass.Medium)
				{
					tmp.fontSize = mediumTMPSize;
				}
				else if (Media.sizeClass >= Media.SizeClass.Large)
				{
					tmp.fontSize = largeTMPSize;
				}
			}
		}
		else
		{
			if (controlImage && image.enabled)
			{
				image.enabled = false;
			}
			if (controlCanvas && canvas.enabled)
			{
				canvas.enabled = false;
			}
			if (controlLayoutGroup && !layoutElement.ignoreLayout)
			{
				layoutElement.ignoreLayout = true;
			}
		}
	}

	private void Awake()
	{
		Check();
	}

	private void Update()
	{
		Check();
	}
}
