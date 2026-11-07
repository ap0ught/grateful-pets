using UnityEngine;
using UnityEngine.UI;

public class gameover : MonoBehaviour
{
	public GameObject m_GameLostHeading;

	public GameObject m_GameWonHeading;

	public GameObject m_GameLostText;

	public GameObject m_GameWonText;

	public Text m_MovesLabel;

	public Text m_TimeLabel;

	public AudioClip m_GameWon;

	public AudioClip m_GameLost;

	public AudioSource m_AudioPlayer;

	public static int MoveCount;

	public static float GameDuration;

	public static bool GameWon;

	private int m_WaitingForSilence;

	public void OnReturnButtonPressed()
	{
		Object.DestroyImmediate(gameObject);
		Object.Instantiate(Resources.Load("Main Menu"));
	}

	public void OnPlayAnotherMatchButtonPressed()
	{
		Object.DestroyImmediate(gameObject);
		Object.Instantiate(Resources.Load("Match3"));
	}

	private void OnEnable()
	{
		UAP_AccessibilityManager.PauseAccessibility(pause: true);
		m_GameLostHeading.SetActive(!GameWon);
		m_GameWonHeading.SetActive(GameWon);
		m_GameLostText.SetActive(!GameWon);
		m_GameWonText.SetActive(GameWon);
		m_MovesLabel.text = MoveCount.ToString("0");
		m_MovesLabel.GetComponent<UAP_BaseElement>().m_Text = "You made " + MoveCount + " swaps.";
		m_TimeLabel.text = GameDuration.ToString("0") + " s";
		m_TimeLabel.GetComponent<UAP_BaseElement>().m_Text = "Game lasted " + GameDuration.ToString("0") + " seconds.";
		m_WaitingForSilence = 0;
	}

	private void Update()
	{
		if (m_WaitingForSilence == 0)
		{
			if (!UAP_AccessibilityManager.IsSpeaking())
			{
				m_WaitingForSilence = 1;
			}
		}
		else if (m_WaitingForSilence == 1)
		{
			m_WaitingForSilence = 2;
			if (GameWon)
			{
				m_AudioPlayer.PlayOneShot(m_GameWon);
			}
			else
			{
				m_AudioPlayer.PlayOneShot(m_GameLost);
			}
			UAP_AccessibilityManager.Say(GameWon ? "Game Won!" : "Game Over!", canBeInterrupted: false, allowVoiceOver: true, UAP_AudioQueue.EInterrupt.All);
			UAP_AccessibilityManager.BlockInput(block: false);
			UAP_AccessibilityManager.PauseAccessibility(pause: false);
		}
	}
}
