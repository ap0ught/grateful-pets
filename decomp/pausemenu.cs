using UnityEngine;
using UnityEngine.UI;

public class pausemenu : MonoBehaviour
{
	public Image m_SoundToggle;

	public Sprite m_SoundOn;

	public Sprite m_SoundOff;

	public UAP_BaseElement m_SoundToggleButton_Access;

	private void OnEnable()
	{
		EnableMusic(PlayerPrefs.GetInt("Music_Enabled", 1) == 1);
		UAP_AccessibilityManager.RegisterOnPauseToggledCallback(OnUserPause);
	}

	private void OnDisable()
	{
		UAP_AccessibilityManager.UnregisterOnPauseToggledCallback(OnUserPause);
	}

	public void OnUserPause()
	{
		OnResumeButtonPressed();
	}

	public void OnResumeButtonPressed()
	{
		gameplay.Instance.ResumeGame();
		Object.DestroyImmediate(gameObject);
	}

	public void OnAbortGameButtonPressed()
	{
		Object.DestroyImmediate(gameObject);
		gameplay.Instance.AbortGame();
	}

	public void OnSoundToggle()
	{
		EnableMusic(PlayerPrefs.GetInt("Music_Enabled", 1) != 1);
	}

	private void EnableMusic(bool enable)
	{
		m_SoundToggleButton_Access.m_Text = (enable ? "Turn Music Off" : "Turn Music On");
		m_SoundToggle.sprite = (enable ? m_SoundOn : m_SoundOff);
		PlayerPrefs.SetInt("Music_Enabled", enable ? 1 : 0);
		PlayerPrefs.Save();
	}
}
