using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.UIControls;

public class PopupWindow : MonoBehaviour
{
	[SerializeField]
	private PopupWindow Prefab;

	[SerializeField]
	private Text DefaultBody;

	[SerializeField]
	private Text TxtHeader;

	[SerializeField]
	private Transform Body;

	[SerializeField]
	private Button BtnCancel;

	[SerializeField]
	private Button BtnOk;

	[SerializeField]
	private LayoutElement Panel;

	public PopupWindowEvent OK;

	public PopupWindowEvent Cancel;

	private PopupWindowAction m_okCallback;

	private PopupWindowAction m_cancelCallback;

	private PopupWindow[] m_openedPopupWindows;

	private static PopupWindow m_instance;

	private bool m_isOpened;

	public bool IsOpened => m_isOpened;

	private void Awake()
	{
		if (Prefab != null)
		{
			m_instance = this;
		}
	}

	private void Start()
	{
		if (BtnCancel != null)
		{
			BtnCancel.onClick.AddListener(OnBtnCancel);
		}
		if (BtnOk != null)
		{
			BtnOk.onClick.AddListener(OnBtnOk);
		}
	}

	private void Update()
	{
		if (!(this == m_instance))
		{
			if (Input.GetKeyDown(KeyCode.Return))
			{
				OnBtnOk();
			}
			else if (Input.GetKeyDown(KeyCode.Escape))
			{
				OnBtnCancel();
			}
		}
	}

	private void OnDestroy()
	{
		if (BtnCancel != null)
		{
			BtnCancel.onClick.RemoveListener(OnBtnCancel);
		}
		if (BtnOk != null)
		{
			BtnOk.onClick.RemoveListener(OnBtnOk);
		}
	}

	private void OnBtnOk()
	{
		if (OK != null)
		{
			PopupWindowArgs popupWindowArgs = new PopupWindowArgs();
			OK.Invoke(popupWindowArgs);
			if (popupWindowArgs.Cancel)
			{
				return;
			}
		}
		if (m_okCallback != null)
		{
			PopupWindowArgs popupWindowArgs2 = new PopupWindowArgs();
			m_okCallback(popupWindowArgs2);
			if (popupWindowArgs2.Cancel)
			{
				return;
			}
		}
		HidePopup();
	}

	private void OnBtnCancel()
	{
		if (Cancel != null)
		{
			PopupWindowArgs popupWindowArgs = new PopupWindowArgs();
			Cancel.Invoke(popupWindowArgs);
			if (popupWindowArgs.Cancel)
			{
				return;
			}
		}
		if (m_cancelCallback != null)
		{
			PopupWindowArgs popupWindowArgs2 = new PopupWindowArgs();
			m_cancelCallback(popupWindowArgs2);
			if (popupWindowArgs2.Cancel)
			{
				return;
			}
		}
		HidePopup();
	}

	private void HidePopup()
	{
		if (m_openedPopupWindows != null)
		{
			PopupWindow[] openedPopupWindows = m_openedPopupWindows;
			foreach (PopupWindow popupWindow in openedPopupWindows)
			{
				if (popupWindow != null)
				{
					popupWindow.gameObject.SetActive(value: true);
				}
			}
		}
		m_openedPopupWindows = null;
		gameObject.SetActive(value: false);
		Object.Destroy(gameObject);
		m_okCallback = null;
		m_cancelCallback = null;
		m_isOpened = false;
	}

	private void ShowPopup(string header, Transform body, string ok = null, PopupWindowAction okCallback = null, string cancel = null, PopupWindowAction cancelCallback = null, float width = 500f)
	{
		m_openedPopupWindows = (from wnd in Object.FindObjectsByType<PopupWindow>(FindObjectsSortMode.None)
			where wnd.IsOpened && wnd.isActiveAndEnabled
			select wnd).ToArray();
		PopupWindow[] openedPopupWindows = m_openedPopupWindows;
		for (int num = 0; num < openedPopupWindows.Length; num++)
		{
			openedPopupWindows[num].gameObject.SetActive(value: false);
		}
		gameObject.SetActive(value: true);
		if (TxtHeader != null)
		{
			TxtHeader.text = header;
		}
		if (Body != null)
		{
			body.SetParent(Body, worldPositionStays: false);
		}
		if (BtnOk != null)
		{
			if (string.IsNullOrEmpty(ok))
			{
				BtnOk.gameObject.SetActive(value: false);
			}
			else
			{
				Text componentInChildren = BtnOk.GetComponentInChildren<Text>();
				if (componentInChildren != null)
				{
					componentInChildren.text = ok;
				}
			}
		}
		if (BtnCancel != null)
		{
			if (string.IsNullOrEmpty(cancel))
			{
				BtnCancel.gameObject.SetActive(value: false);
			}
			else
			{
				Text componentInChildren2 = BtnCancel.GetComponentInChildren<Text>();
				if (componentInChildren2 != null)
				{
					componentInChildren2.text = cancel;
				}
			}
		}
		if (Panel != null)
		{
			Panel.preferredWidth = width;
		}
		m_okCallback = okCallback;
		m_cancelCallback = cancelCallback;
		m_isOpened = true;
	}

	public void Close(bool result)
	{
		if (result)
		{
			OnBtnOk();
		}
		else
		{
			OnBtnCancel();
		}
	}

	public static void Show(string header, string body, string ok, PopupWindowAction okCallback = null, string cancel = null, PopupWindowAction cancelCallback = null, float width = 530f)
	{
		if (m_instance == null)
		{
			Debug.LogWarning("PopupWindows.m_instance is null");
			return;
		}
		PopupWindow popupWindow = Object.Instantiate(m_instance.Prefab);
		popupWindow.transform.position = Vector3.zero;
		popupWindow.transform.SetParent(m_instance.transform, worldPositionStays: false);
		popupWindow.DefaultBody.text = body;
		popupWindow.ShowPopup(header, popupWindow.DefaultBody.transform, ok, okCallback, cancel, cancelCallback, width);
	}

	public static void Show(string header, Transform body, string ok, PopupWindowAction okCallback = null, string cancel = null, PopupWindowAction cancelCallback = null, float width = 530f)
	{
		if (m_instance == null)
		{
			Debug.LogWarning("PopupWindows.m_instance is null");
			return;
		}
		PopupWindow popupWindow = Object.Instantiate(m_instance.Prefab);
		popupWindow.transform.position = Vector3.zero;
		popupWindow.transform.SetParent(m_instance.transform, worldPositionStays: false);
		if (popupWindow.DefaultBody != null)
		{
			Object.Destroy(popupWindow.DefaultBody);
		}
		popupWindow.ShowPopup(header, body, ok, okCallback, cancel, cancelCallback, width);
	}
}
