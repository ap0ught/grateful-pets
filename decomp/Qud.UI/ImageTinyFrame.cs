using UnityEngine;
using UnityEngine.UI;
using XRL.UI;
using XRL.UI.Framework;

namespace Qud.UI;

[ExecuteAlways]
[RequireComponent(typeof(LayoutElement))]
public class ImageTinyFrame : MonoBehaviour
{
	public Sprite sprite;

	public Color borderColor;

	public bool colorBasedOnSelection;

	public Color selectedBorderColor;

	public Color unselectedBorderColor;

	public Vector2 imageSize;

	public Vector2 totalSize;

	public Image image;

	public Image borderImage;

	public bool useThreeColor;

	public UIThreeColorProperties ThreeColor;

	public Color selectedForegroundColor = Color.white;

	public Color unselectedForegroundColor = Color.gray;

	public Color selectedDetailColor = Color.yellow;

	public Color unselectedDetailColor = Color.black;

	public bool useImageSwap;

	public Sprite unselectedImageSwap;

	public Sprite selectedImageSwap;

	private Vector2 _lastImageSize;

	private Color _lastBorderColor;

	private RectTransform _rectTransform;

	private LayoutElement _layoutElement;

	private Sprite _lastSprite;

	private bool? _lastActive;

	private bool? _lastActiveSwap;

	private FrameworkContext _frameworkContext;

	private void Start()
	{
	}

	private void Update()
	{
		Sync();
	}

	public void Sync(bool force = false)
	{
		if (_rectTransform == null)
		{
			_rectTransform = GetComponent<RectTransform>();
		}
		if (_layoutElement == null)
		{
			_layoutElement = GetComponent<LayoutElement>();
		}
		if (_lastImageSize != imageSize && image != null)
		{
			if (totalSize.magnitude < 1f)
			{
				image.rectTransform.sizeDelta = (_lastImageSize = imageSize);
				Vector2 vector = (_rectTransform.sizeDelta = imageSize + new Vector2(16f, 14f));
				totalSize = vector;
			}
			_layoutElement.preferredWidth = totalSize.x;
			_layoutElement.preferredHeight = totalSize.y;
		}
		if (useImageSwap)
		{
			if (_frameworkContext == null)
			{
				_frameworkContext = GetComponentInParent<FrameworkContext>();
			}
			bool valueOrDefault = _frameworkContext?.context?.IsActive() == true;
			if ((valueOrDefault != _lastActiveSwap) | force)
			{
				_lastActiveSwap = valueOrDefault;
				image.sprite = (valueOrDefault ? selectedImageSwap : unselectedImageSwap);
			}
		}
		if (colorBasedOnSelection)
		{
			if (_frameworkContext == null)
			{
				_frameworkContext = GetComponentInParent<FrameworkContext>();
			}
			bool valueOrDefault2 = _frameworkContext?.context?.IsActive() == true;
			if ((valueOrDefault2 != _lastActive) | force)
			{
				_lastActive = valueOrDefault2;
				borderColor = (valueOrDefault2 ? selectedBorderColor : unselectedBorderColor);
				if (useThreeColor)
				{
					ThreeColor.SetColors(valueOrDefault2 ? selectedForegroundColor : unselectedForegroundColor, valueOrDefault2 ? selectedDetailColor : unselectedDetailColor, Color.clear);
				}
				UITextSkin uITextSkin = GetComponent<TitledIconButton>()?.TitleText;
				if (uITextSkin != null)
				{
					uITextSkin.color = borderColor;
					uITextSkin.StripFormatting = !valueOrDefault2;
					uITextSkin.Apply();
				}
			}
		}
		if (_lastBorderColor != borderColor && borderImage != null)
		{
			borderImage.color = (_lastBorderColor = borderColor);
		}
		if (_lastSprite != sprite)
		{
			image.sprite = (_lastSprite = sprite);
		}
	}
}
