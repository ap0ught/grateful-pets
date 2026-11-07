using UnityEngine;
using UnityEngine.UI;

namespace RedShadow.CommonDialogs;

public static class DialogUtility
{
	public static string prettyPrintSize(long size)
	{
		string[] array = new string[5] { "B", "KB", "MB", "GB", "TB" };
		int num = 0;
		while (size >= 1024 && num < array.Length - 1)
		{
			num++;
			size /= 1024;
		}
		return $"{size:0.##} {array[num]}";
	}

	public static void centerOnItem(GameObject contentPanel, RectTransform target)
	{
		RectTransform component = contentPanel.GetComponent<RectTransform>();
		RectTransform component2 = contentPanel.transform.parent.GetComponent<RectTransform>();
		RectTransform component3 = component2.transform.parent.GetComponent<RectTransform>();
		ScrollRect component4 = component2.transform.parent.GetComponent<ScrollRect>();
		Vector3 vector = component2.position + (Vector3)component2.rect.center;
		Vector3 position = target.position;
		Vector3 vector2 = vector - position;
		vector2.z = 0f;
		Vector2 vector3 = new Vector2(vector2.x / (component.rect.size.x - component3.rect.size.x), vector2.y / (component.rect.size.y - component3.rect.size.y));
		Vector2 normalizedPosition = component4.normalizedPosition - vector3;
		if (component4.movementType != ScrollRect.MovementType.Unrestricted)
		{
			normalizedPosition.x = Mathf.Clamp01(normalizedPosition.x);
			normalizedPosition.y = Mathf.Clamp01(normalizedPosition.y);
		}
		component4.normalizedPosition = normalizedPosition;
	}

	public static Vector2 sizeToParent(RawImage image, float padding = 0f)
	{
		RectTransform componentInParent = image.transform.parent.GetComponentInParent<RectTransform>();
		RectTransform component = image.GetComponent<RectTransform>();
		if (!componentInParent)
		{
			return component.sizeDelta;
		}
		float num = (float)image.texture.width / (float)image.texture.height;
		Rect rect = new Rect(0f, 0f, componentInParent.rect.width, componentInParent.rect.height);
		if (Mathf.RoundToInt(component.eulerAngles.z) % 180 == 90)
		{
			rect.size = new Vector2(rect.height, rect.width);
		}
		float num2 = rect.height;
		float num3 = num2 * num;
		if (num3 > rect.width)
		{
			num3 = rect.width;
			num2 = num3 / num;
		}
		component.sizeDelta = new Vector2(num3 - padding * 2f, num2 - padding * 2f);
		return component.sizeDelta;
	}

	public static Rect rectTransformToScreenSpace(RectTransform transform)
	{
		Vector2 vector = Vector2.Scale(transform.rect.size, transform.lossyScale);
		float x = transform.position.x - transform.anchoredPosition.x;
		float y = (float)Screen.height - transform.position.y - transform.anchoredPosition.y;
		return new Rect(x, y, vector.x, vector.y);
	}

	public static Rect getScreenCoordinates(RectTransform transform)
	{
		Vector3[] array = new Vector3[4];
		transform.GetWorldCorners(array);
		return new Rect(array[0].x, array[0].y, array[2].x - array[0].x, array[2].y - array[0].y);
	}
}
