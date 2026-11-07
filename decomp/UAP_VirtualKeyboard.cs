using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UAP_VirtualKeyboard : MonoBehaviour
{
	[Serializable]
	public class UAPKeyboardLayout
	{
		public SystemLanguage m_SystemLanguage = SystemLanguage.English;

		public string m_Letters = "QWERTYUIOPASDFGHJKLZXCVBNM";

		public string m_BottomKeys = ",.";
	}

	public enum EKeyboardMode
	{
		Default,
		Password
	}

	private enum EKeyboardPage
	{
		Letters,
		Numbers,
		Symbols
	}

	private const string PrefabPath = "UAP Virtual Keyboard";

	[Header("Layouts")]
	public UAPKeyboardLayout m_NumbersLayout = new UAPKeyboardLayout
	{
		m_Letters = "1234567890@#$&*()'\"_ -/:;!?"
	};

	public UAPKeyboardLayout m_SymbolsLayout = new UAPKeyboardLayout
	{
		m_Letters = "1234567890€£¥%^[]{}+=|\\©®™",
		m_BottomKeys = "<>"
	};

	public List<UAPKeyboardLayout> m_SupportedKeyboardLayouts = new List<UAPKeyboardLayout>();

	[Header("References")]
	public Transform m_SecondButtonRow;

	public List<Button> m_LetterButtons = new List<Button>();

	public Text m_PreviewText;

	public UAP_BaseElement m_PreviewTextAccessible;

	public Button m_LanguageButton;

	public Button m_EmailAtButton;

	public Button m_ShiftKey;

	public Image m_ShiftSymbol;

	public Button m_SwitchKey;

	public Button m_Done;

	public Button m_Cancel;

	public Button m_LeftOfSpace;

	public Button m_RightOfSpace;

	public Button m_ReturnKey;

	private string m_OriginalText = "";

	private bool m_AllowMutliLine;

	private EKeyboardMode m_KeyboardMode;

	private bool m_StartCapitalized = true;

	private SystemLanguage m_PreferredLanguage = SystemLanguage.English;

	private EKeyboardPage m_CurrentKeyboardPage;

	private UAPKeyboardLayout m_ActiveLetterLayout;

	private bool m_ShiftModeActive;

	private string m_EditedText = "";

	private string m_PasswordedText = "";

	private SystemLanguage m_CurrentLanguage = SystemLanguage.English;

	private float m_CursorBlinkDuration = 0.8f;

	private float m_CursorBlinkTimer = -1f;

	private static UAP_VirtualKeyboard Instance;

	private static UnityAction<string, bool> m_OnFinishListener;

	private static UnityAction<string> m_OnChangeListener;

	private void Update()
	{
		m_CursorBlinkTimer -= Time.unscaledDeltaTime;
		bool flag = m_CursorBlinkTimer > 0f;
		if (m_CursorBlinkTimer < 0f - m_CursorBlinkDuration)
		{
			m_CursorBlinkTimer = m_CursorBlinkDuration;
		}
		if (m_PasswordedText.Length != m_EditedText.Length)
		{
			m_PasswordedText = "";
			for (int i = 0; i < m_EditedText.Length; i++)
			{
				m_PasswordedText += "•";
			}
		}
		m_PreviewText.text = ((m_KeyboardMode == EKeyboardMode.Password) ? m_PasswordedText : m_EditedText) + (flag ? "<color='#777777'>|</color>" : "");
		m_PreviewTextAccessible.SetCustomText((m_KeyboardMode == EKeyboardMode.Password) ? UAP_AccessibilityManager.Localize_Internal("Keyboard_PasswordHidden") : m_EditedText);
		if (Input.GetKeyDown(KeyCode.Backspace))
		{
			OnBackSpacePressed();
		}
		else if (Input.GetKeyDown(KeyCode.Return))
		{
			if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android || Input.GetKey(KeyCode.LeftShift))
			{
				OnReturnPressed();
			}
		}
		else if (!string.IsNullOrEmpty(Input.inputString))
		{
			AddLetter(Input.inputString);
		}
	}

	private void OnTextUpdated()
	{
		m_CursorBlinkTimer = m_CursorBlinkDuration;
		if (m_ShiftModeActive)
		{
			if (m_CurrentKeyboardPage == EKeyboardPage.Letters)
			{
				OnShiftKeyPressed();
			}
			else
			{
				m_ShiftModeActive = false;
			}
		}
		if (m_OnChangeListener != null)
		{
			m_OnChangeListener(m_EditedText);
		}
		if (m_KeyboardMode == EKeyboardMode.Default)
		{
			UAP_AccessibilityManager.Say(m_EditedText);
		}
	}

	private void SetKeyboardLayoutForLanguage(SystemLanguage language)
	{
		m_CurrentLanguage = language;
		m_ActiveLetterLayout = null;
		for (int i = 0; i < m_SupportedKeyboardLayouts.Count; i++)
		{
			if (m_SupportedKeyboardLayouts[i].m_SystemLanguage == language)
			{
				m_ActiveLetterLayout = m_SupportedKeyboardLayouts[i];
			}
		}
		if (m_ActiveLetterLayout == null)
		{
			m_ActiveLetterLayout = new UAPKeyboardLayout();
		}
		SetLettersLayout();
		if (m_PreferredLanguage == SystemLanguage.English)
		{
			m_LanguageButton.gameObject.SetActive(value: false);
			m_EmailAtButton.gameObject.SetActive(value: true);
			return;
		}
		m_LanguageButton.gameObject.SetActive(value: true);
		m_EmailAtButton.gameObject.SetActive(value: false);
		if (m_CurrentLanguage == m_PreferredLanguage)
		{
			m_LanguageButton.GetComponent<UAP_BaseElement>().SetCustomText("English");
		}
		else
		{
			m_LanguageButton.GetComponent<UAP_BaseElement>().SetCustomText(m_PreferredLanguage.ToString());
		}
	}

	private void SetLetterButtonsFromString(string letters, string bottomKeys)
	{
		bool flag = letters.Length > 26;
		m_LetterButtons[19].gameObject.SetActive(flag);
		if (flag)
		{
			float num = m_LetterButtons[4].transform.position.x - m_LetterButtons[3].transform.position.x;
			m_SecondButtonRow.localPosition = new Vector3(-0.5f * num, 0f, 0f);
		}
		else
		{
			m_SecondButtonRow.localPosition = new Vector3(0f, 0f, 0f);
		}
		int num2 = -1;
		for (int i = 0; i < m_LetterButtons.Count; i++)
		{
			if (!flag && i == 19)
			{
				continue;
			}
			num2++;
			if (num2 < letters.Length)
			{
				if (m_CurrentKeyboardPage == EKeyboardPage.Letters)
				{
					m_LetterButtons[i].GetComponent<UAP_BaseElement>().SetCustomText(m_ShiftModeActive ? UAP_AccessibilityManager.Localize_Internal("Keyboard_CapitalLetter").Replace("X", letters[num2].ToString().ToLower()) : letters[num2].ToString().ToLower());
					m_LetterButtons[i].GetComponentInChildren<Text>().text = (m_ShiftModeActive ? letters[num2].ToString().ToUpper() : letters[num2].ToString().ToLower());
				}
				else
				{
					m_LetterButtons[i].GetComponent<UAP_BaseElement>().SetCustomText(letters[num2].ToString());
					m_LetterButtons[i].GetComponentInChildren<Text>().text = letters[num2].ToString();
				}
			}
			else
			{
				m_LetterButtons[i].gameObject.SetActive(value: false);
			}
		}
		m_LeftOfSpace.GetComponentInChildren<Text>().text = bottomKeys[0].ToString();
		m_RightOfSpace.GetComponentInChildren<Text>().text = bottomKeys[1].ToString();
	}

	public void OnShiftKeyPressed()
	{
		switch (m_CurrentKeyboardPage)
		{
		case EKeyboardPage.Numbers:
			SetSymbolsLayout();
			return;
		case EKeyboardPage.Symbols:
			SetNumbersLayout();
			return;
		}
		m_ShiftModeActive = !m_ShiftModeActive;
		m_ShiftSymbol.color = (m_ShiftModeActive ? Color.white : ((Color)new Color32(50, 50, 50, byte.MaxValue)));
		UAP_AccessibilityManager.Say(UAP_AccessibilityManager.Localize_Internal(m_ShiftModeActive ? "Keyboard_ShiftOn" : "Keyboard_ShiftOff"));
		SetLettersLayout();
	}

	public void OnToggleKeyPressed()
	{
		switch (m_CurrentKeyboardPage)
		{
		case EKeyboardPage.Letters:
			SetNumbersLayout();
			break;
		default:
			SetLettersLayout();
			break;
		}
	}

	public void OnLanguageKeyPressed()
	{
		if (m_CurrentLanguage != m_PreferredLanguage)
		{
			SetKeyboardLayoutForLanguage(m_PreferredLanguage);
		}
		else
		{
			SetKeyboardLayoutForLanguage(SystemLanguage.English);
		}
		UAP_AccessibilityManager.Say(UAP_AccessibilityManager.Localize_Internal("Keyboard_ShowingLanguage") + m_CurrentLanguage);
	}

	private void SetLettersLayout()
	{
		if (m_CurrentKeyboardPage != EKeyboardPage.Letters)
		{
			UAP_AccessibilityManager.Say(UAP_AccessibilityManager.Localize_Internal("Keyboard_ShowingLetters"));
		}
		m_CurrentKeyboardPage = EKeyboardPage.Letters;
		m_ShiftKey.GetComponentInChildren<Text>().text = "";
		m_ShiftSymbol.gameObject.SetActive(value: true);
		m_ShiftKey.GetComponent<UAP_BaseElement>().SetCustomText(UAP_AccessibilityManager.Localize_Internal("Keyboard_ShiftKey"));
		m_SwitchKey.GetComponentInChildren<Text>().text = "123";
		m_SwitchKey.GetComponent<UAP_BaseElement>().SetCustomText(UAP_AccessibilityManager.Localize_Internal("Keyboard_NumbersAndSymbols"));
		SetLetterButtonsFromString(m_ActiveLetterLayout.m_Letters, m_ActiveLetterLayout.m_BottomKeys);
	}

	private void SetNumbersLayout()
	{
		if (m_CurrentKeyboardPage != EKeyboardPage.Numbers)
		{
			UAP_AccessibilityManager.Say(UAP_AccessibilityManager.Localize_Internal("Keyboard_ShowingNumbers"));
		}
		m_ShiftModeActive = false;
		m_CurrentKeyboardPage = EKeyboardPage.Numbers;
		m_ShiftKey.GetComponentInChildren<Text>().text = "%{<";
		m_ShiftSymbol.gameObject.SetActive(value: false);
		m_ShiftKey.GetComponent<UAP_BaseElement>().SetCustomText(UAP_AccessibilityManager.Localize_Internal("Keyboard_Symbols"));
		m_SwitchKey.GetComponentInChildren<Text>().text = "abc";
		m_SwitchKey.GetComponent<UAP_BaseElement>().SetCustomText(UAP_AccessibilityManager.Localize_Internal("Keyboard_Letters"));
		SetLetterButtonsFromString(m_NumbersLayout.m_Letters, m_NumbersLayout.m_BottomKeys);
	}

	private void SetSymbolsLayout()
	{
		if (m_CurrentKeyboardPage != EKeyboardPage.Symbols)
		{
			UAP_AccessibilityManager.Say(UAP_AccessibilityManager.Localize_Internal("Keyboard_ShowingSymbols"));
		}
		m_ShiftModeActive = false;
		m_CurrentKeyboardPage = EKeyboardPage.Symbols;
		m_ShiftKey.GetComponentInChildren<Text>().text = "123";
		m_ShiftSymbol.gameObject.SetActive(value: false);
		m_ShiftKey.GetComponent<UAP_BaseElement>().SetCustomText(UAP_AccessibilityManager.Localize_Internal("Keyboard_Numbers"));
		m_SwitchKey.GetComponentInChildren<Text>().text = "abc";
		m_SwitchKey.GetComponent<UAP_BaseElement>().SetCustomText(UAP_AccessibilityManager.Localize_Internal("Keyboard_Letters"));
		SetLetterButtonsFromString(m_SymbolsLayout.m_Letters, m_SymbolsLayout.m_BottomKeys);
	}

	private void AddLetter(string letter)
	{
		UAP_AccessibilityManager.Say(letter);
		m_EditedText += letter;
		OnTextUpdated();
	}

	public void OnLetterKeyPressed(Button button)
	{
		AddLetter(button.GetComponentInChildren<Text>().text);
	}

	public void OnSpacePressed()
	{
		AddLetter(" ");
		if (m_CurrentKeyboardPage != EKeyboardPage.Letters)
		{
			SetLettersLayout();
		}
	}

	public void OnBackSpacePressed()
	{
		if (m_EditedText.Length != 0)
		{
			m_EditedText = m_EditedText.Substring(0, m_EditedText.Length - 1);
			OnTextUpdated();
			AutoSetShiftMode();
		}
	}

	public void OnReturnPressed()
	{
		if (m_AllowMutliLine)
		{
			AddLetter("\n");
		}
		else
		{
			OnDonePressed();
		}
	}

	public void OnClearTextPressed()
	{
		m_EditedText = "";
		OnTextUpdated();
		AutoSetShiftMode();
	}

	private void AutoSetShiftMode()
	{
		if (m_EditedText.Length == 0 && m_StartCapitalized && !m_ShiftModeActive)
		{
			if (m_CurrentKeyboardPage == EKeyboardPage.Letters)
			{
				OnShiftKeyPressed();
			}
			else
			{
				m_ShiftModeActive = true;
			}
		}
	}

	public void OnDonePressed()
	{
		if (m_OnFinishListener != null)
		{
			m_OnFinishListener(m_EditedText, arg1: true);
		}
		ClearAllListeners();
		CloseKeyboardOverlay();
	}

	public void OnCancelPressed()
	{
		if (m_OnFinishListener != null)
		{
			m_OnFinishListener(m_OriginalText, arg1: false);
		}
		ClearAllListeners();
		CloseKeyboardOverlay();
	}

	private void OnApplicationFocus(bool focus)
	{
		if (!focus)
		{
			OnCancelPressed();
		}
	}

	private void InitializeKeyboard(string prefilledText, EKeyboardMode keyboardMode = EKeyboardMode.Default, bool startCapitalized = true, bool alllowMultiline = false)
	{
		if ((keyboardMode == EKeyboardMode.Password) & startCapitalized)
		{
			Debug.LogWarning("[Accessibility] Password Input fields should not start capitalized. Ignoring parameter.");
			startCapitalized = false;
		}
		m_KeyboardMode = keyboardMode;
		m_StartCapitalized = startCapitalized;
		m_EditedText = prefilledText;
		m_OriginalText = prefilledText;
		m_CursorBlinkTimer = m_CursorBlinkDuration;
		m_AllowMutliLine = alllowMultiline;
		if (SupportsSystemLanguage())
		{
			m_PreferredLanguage = Application.systemLanguage;
		}
		else
		{
			m_PreferredLanguage = SystemLanguage.English;
		}
		SetKeyboardLayoutForLanguage(m_PreferredLanguage);
		m_ReturnKey.GetComponent<UAP_BaseElement>().SetCustomText(alllowMultiline ? UAP_AccessibilityManager.Localize_Internal("Keyboard_Return") : UAP_AccessibilityManager.Localize_Internal("Keyboard_Done"));
		AutoSetShiftMode();
		UAP_AccessibilityManager.Say(UAP_AccessibilityManager.Localize_Internal("Keyboard_Showing"));
		UAP_AccessibilityManager.Say(UAP_AccessibilityManager.Localize_Internal(m_ShiftModeActive ? "Keyboard_ShiftOn" : "Keyboard_ShiftOff"));
	}

	private void CloseKeyboardOverlay()
	{
		Instance = null;
		UnityEngine.Object.DestroyImmediate(gameObject);
		UAP_AccessibilityManager.Say(UAP_AccessibilityManager.Localize_Internal("Keyboard_Hidden"));
	}

	private void OnDestroy()
	{
		Instance = null;
	}

	public static void SetOnFinishListener(UnityAction<string, bool> callback)
	{
		m_OnFinishListener = callback;
	}

	public static void SetOnChangeListener(UnityAction<string> callback)
	{
		m_OnChangeListener = callback;
	}

	public static void CloseKeyboard()
	{
		if (Instance == null)
		{
			ClearAllListeners();
		}
		else
		{
			Instance.OnCancelPressed();
		}
	}

	public static UAP_VirtualKeyboard ShowOnscreenKeyboard(string prefilledText = "", EKeyboardMode keyboardMode = EKeyboardMode.Default, bool startCapitalized = true, bool alllowMultiline = false)
	{
		if (Instance != null)
		{
			UnityEngine.Object.DestroyImmediate(Instance.gameObject);
		}
		ClearAllListeners();
		UAP_VirtualKeyboard component = (UnityEngine.Object.Instantiate(Resources.Load("UAP Virtual Keyboard")) as GameObject).GetComponent<UAP_VirtualKeyboard>();
		component.InitializeKeyboard(prefilledText, keyboardMode, startCapitalized, alllowMultiline);
		Instance = component;
		return component;
	}

	public static void ClearAllListeners()
	{
		m_OnFinishListener = null;
		m_OnChangeListener = null;
	}

	public static bool IsOpen()
	{
		return Instance != null;
	}

	public static bool SupportsSystemLanguage()
	{
		List<UAPKeyboardLayout> supportedKeyboardLayouts = (Resources.Load("UAP Virtual Keyboard") as GameObject).GetComponent<UAP_VirtualKeyboard>().m_SupportedKeyboardLayouts;
		for (int i = 0; i < supportedKeyboardLayouts.Count; i++)
		{
			if (supportedKeyboardLayouts[i].m_SystemLanguage == Application.systemLanguage)
			{
				return true;
			}
		}
		return false;
	}
}
