using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ModelShark;

public static class ExtensionMethods
{
	public static void FillParameterizedTextFields(this Text[] textFields, ref List<ParameterizedTextField> parameterizedTextFields, string delimiter)
	{
		List<string> fieldNames = new List<string>();
		foreach (Text obj in textFields)
		{
			foreach (Match item in Regex.Matches(pattern: string.Format("{0}\\w*{0}", delimiter), input: obj.text, options: RegexOptions.IgnoreCase | RegexOptions.Multiline))
			{
				string text = item.Value.Trim('%');
				if (!fieldNames.Contains(text))
				{
					fieldNames.Add(text);
				}
				bool flag = false;
				foreach (ParameterizedTextField parameterizedTextField in parameterizedTextFields)
				{
					if (text == parameterizedTextField.name)
					{
						parameterizedTextField.placeholder = item.Value;
						flag = true;
					}
				}
				if (!flag)
				{
					parameterizedTextFields.Add(new ParameterizedTextField
					{
						name = text,
						placeholder = item.Value,
						value = string.Empty
					});
				}
			}
		}
		parameterizedTextFields.RemoveAll((ParameterizedTextField x) => !fieldNames.Contains(x.name));
	}

	public static void FillParameterizedTextFields(this TextMeshProUGUI[] textFields, ref List<ParameterizedTextField> parameterizedTextFields, string delimiter)
	{
		List<string> fieldNames = new List<string>();
		foreach (TextMeshProUGUI obj in textFields)
		{
			foreach (Match item in Regex.Matches(pattern: string.Format("{0}\\w*{0}", delimiter), input: obj.text, options: RegexOptions.IgnoreCase | RegexOptions.Multiline))
			{
				string text = item.Value.Trim('%');
				if (!fieldNames.Contains(text))
				{
					fieldNames.Add(text);
				}
				bool flag = false;
				foreach (ParameterizedTextField parameterizedTextField in parameterizedTextFields)
				{
					if (text == parameterizedTextField.name)
					{
						parameterizedTextField.placeholder = item.Value;
						flag = true;
					}
				}
				if (!flag)
				{
					parameterizedTextFields.Add(new ParameterizedTextField
					{
						name = text,
						placeholder = item.Value,
						value = string.Empty
					});
				}
			}
		}
		parameterizedTextFields.RemoveAll((ParameterizedTextField x) => !fieldNames.Contains(x.name));
	}

	public static void FillDynamicImageFields(this DynamicImage[] imageFields, ref List<DynamicImageField> dynamicImageFields, string delimiter)
	{
		List<string> fieldNames = new List<string>();
		foreach (DynamicImage obj in imageFields)
		{
			string text = obj.placeholderName.Trim('%');
			if (!fieldNames.Contains(text))
			{
				fieldNames.Add(text);
			}
			Image placeholderImage = obj.PlaceholderImage;
			bool flag = false;
			foreach (DynamicImageField dynamicImageField in dynamicImageFields)
			{
				if (text == dynamicImageField.name)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				dynamicImageFields.Add(new DynamicImageField
				{
					name = text,
					placeholderSprite = placeholderImage.sprite,
					replacementSprite = null
				});
			}
		}
		dynamicImageFields.RemoveAll((DynamicImageField x) => !fieldNames.Contains(x.name));
	}

	public static void FillDynamicSectionFields(this DynamicSection[] sectionFields, ref List<DynamicSectionField> dynamicSectionFields, string delimiter)
	{
		List<string> fieldNames = new List<string>();
		foreach (DynamicSection obj in sectionFields)
		{
			string text = obj.placeholderName.Trim('%');
			if (!fieldNames.Contains(text))
			{
				fieldNames.Add(text);
			}
			GameObject gameObject = obj.gameObject;
			bool flag = false;
			foreach (DynamicSectionField dynamicSectionField in dynamicSectionFields)
			{
				if (text == dynamicSectionField.name)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				dynamicSectionFields.Add(new DynamicSectionField
				{
					name = text,
					isOn = gameObject.activeSelf
				});
			}
		}
		dynamicSectionFields.RemoveAll((DynamicSectionField x) => !fieldNames.Contains(x.name));
	}
}
