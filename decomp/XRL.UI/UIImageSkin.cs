using System.Collections.Generic;
using ConsoleLib.Console;
using UnityEngine;
using UnityEngine.UI;

namespace XRL.UI;

[ExecuteAlways]
[RequireComponent(typeof(Image))]
public class UIImageSkin : MonoBehaviour
{
	public enum ImageColor
	{
		unset = -1,
		borderLine,
		darkBackground,
		mediumBackground,
		lightBackground,
		brightBackground,
		darkFrame,
		mediumFrame,
		brightFrame
	}

	public enum ImageMaterial
	{
		unset = -1,
		diag,
		horiz
	}

	private RectTransform _rt;

	private Image _img;

	public static Dictionary<ImageColor, Color> colorMap = new Dictionary<ImageColor, Color>
	{
		{
			ImageColor.borderLine,
			ConsoleLib.Console.ColorUtility.FromWebColor("4D6E7A")
		},
		{
			ImageColor.darkBackground,
			ConsoleLib.Console.ColorUtility.FromWebColor("041A1A")
		},
		{
			ImageColor.mediumBackground,
			ConsoleLib.Console.ColorUtility.FromWebColor("0A2625")
		},
		{
			ImageColor.lightBackground,
			ConsoleLib.Console.ColorUtility.FromWebColor("1A343B")
		},
		{
			ImageColor.brightBackground,
			ConsoleLib.Console.ColorUtility.FromWebColor("203C3F")
		},
		{
			ImageColor.darkFrame,
			ConsoleLib.Console.ColorUtility.FromWebColor("101F23")
		},
		{
			ImageColor.mediumFrame,
			ConsoleLib.Console.ColorUtility.FromWebColor("1A343B")
		},
		{
			ImageColor.brightFrame,
			ConsoleLib.Console.ColorUtility.FromWebColor("475D64")
		}
	};

	public static Dictionary<ImageMaterial, Material> _materialMap = null;

	public ImageColor color;

	public ImageMaterial material = ImageMaterial.unset;

	private ImageMaterial lastMaterial = ImageMaterial.unset;

	public RectTransform rectTransform => _rt ?? (_rt = GetComponent<RectTransform>());

	private Image img
	{
		get
		{
			if (_img == null)
			{
				_img = GetComponent<Image>();
			}
			return _img;
		}
	}

	public static Dictionary<ImageMaterial, Material> materialMap
	{
		get
		{
			if (_materialMap == null)
			{
				_materialMap = new Dictionary<ImageMaterial, Material>
				{
					{
						ImageMaterial.diag,
						(Material)Resources.Load("Materials/TextureOverlay-Diag")
					},
					{
						ImageMaterial.horiz,
						(Material)Resources.Load("Materials/TextureOverlay-Horiz")
					}
				};
			}
			return _materialMap;
		}
	}

	private Image Updated()
	{
		Apply();
		return img;
	}

	public void Apply()
	{
		if (color != ImageColor.unset)
		{
			img.color = colorMap[color];
		}
		if (material != lastMaterial)
		{
			lastMaterial = material;
			img.material = materialMap[material];
		}
	}

	public void Start()
	{
		Apply();
	}
}
