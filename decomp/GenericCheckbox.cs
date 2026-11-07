using UnityEngine;
using UnityEngine.UI;
using XRL.UI;

[RequireComponent(typeof(Toggle))]
public class GenericCheckbox : MonoBehaviour
{
	public Toggle toggle;

	public UITextSkin checkboxText;

	protected const string EMPTY_CHECK = "[ ] ";

	protected const string CHECKED = "[■] ";

	protected const string CHECKED_ACTIVE = "[{{W|■}}] ";

	private void Start()
	{
		toggle = GetComponent<Toggle>();
		OnToggleValueChanged(toggle.isOn);
	}

	public void OnToggleValueChanged(bool value)
	{
		checkboxText.SetText(value ? "[■] " : "[ ] ");
	}

	private void Update()
	{
	}
}
