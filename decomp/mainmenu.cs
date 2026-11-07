using UnityEngine;
using UnityEngine.UI;

public class mainmenu : MonoBehaviour
{
	public Dropdown m_DifficultyDropdown;

	private void Start()
	{
		gameplay.DifficultyLevel = PlayerPrefs.GetInt("Difficulty", 0);
		m_DifficultyDropdown.value = gameplay.DifficultyLevel;
	}

	public void OnInstructionsButtonPressed()
	{
		if (UAP_AccessibilityManager.IsEnabled())
		{
			Object.Instantiate(Resources.Load("Instructions"));
		}
		else
		{
			Object.Instantiate(Resources.Load("Instructions Sighted"));
		}
		Object.DestroyImmediate(gameObject);
	}

	public void OnQuitButtonPressed()
	{
		UAP_AccessibilityManager.Say("Goodbye");
		Application.Quit();
	}

	public void OnPlayButtonPressed()
	{
		gameplay.DifficultyLevel = m_DifficultyDropdown.value;
		PlayerPrefs.SetInt("Difficulty", gameplay.DifficultyLevel);
		PlayerPrefs.Save();
		Object.Instantiate(Resources.Load("Match3"));
		Object.DestroyImmediate(gameObject);
	}

	public void OnAccessibilityButtonPressed()
	{
		Object.Instantiate(Resources.Load("Accessibility Settings"));
	}
}
