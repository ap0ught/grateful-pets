using System;
using System.Collections.Generic;
using UnityEngine;

namespace RedShadow.CommonDialogs;

public class DialogManager : MonoBehaviour
{
	public static DialogManager Instance;

	public FileDialog FileDialogPrefab;

	public MessageDialog MessageDialogPrefab;

	public NotificationMessage NotificationMessagePrefab;

	public ProgressDialog ProgressDialogPrefab;

	public InputDialog InputDialogPrefab;

	public LoginDialog LoginDialogPrefab;

	public LoginDialog PairDialogPrefab;

	public Menu MenuPrefab;

	public MenuBar MenuBarPrefab;

	public ToolBar ToolBarPrefab;

	protected void Awake()
	{
		Instance = this;
	}

	public static FileDialog createFileDialog()
	{
		FileDialog fileDialog = UnityEngine.Object.Instantiate(Instance.FileDialogPrefab);
		fileDialog.transform.SetParent(Instance.gameObject.transform, worldPositionStays: false);
		return fileDialog;
	}

	public static MessageDialog createMessageDialog()
	{
		MessageDialog messageDialog = UnityEngine.Object.Instantiate(Instance.MessageDialogPrefab);
		messageDialog.transform.SetParent(Instance.gameObject.transform, worldPositionStays: false);
		return messageDialog;
	}

	public static NotificationMessage createNotificationMessage()
	{
		NotificationMessage notificationMessage = UnityEngine.Object.Instantiate(Instance.NotificationMessagePrefab);
		notificationMessage.transform.SetParent(Instance.gameObject.transform, worldPositionStays: false);
		return notificationMessage;
	}

	public static ProgressDialog createProgressDialog()
	{
		ProgressDialog progressDialog = UnityEngine.Object.Instantiate(Instance.ProgressDialogPrefab);
		progressDialog.transform.SetParent(Instance.gameObject.transform, worldPositionStays: false);
		return progressDialog;
	}

	public static InputDialog createInputDialog()
	{
		InputDialog inputDialog = UnityEngine.Object.Instantiate(Instance.InputDialogPrefab);
		inputDialog.transform.SetParent(Instance.gameObject.transform, worldPositionStays: false);
		return inputDialog;
	}

	public static LoginDialog createLoginDialog()
	{
		LoginDialog loginDialog = UnityEngine.Object.Instantiate(Instance.LoginDialogPrefab);
		loginDialog.transform.SetParent(Instance.gameObject.transform, worldPositionStays: false);
		return loginDialog;
	}

	public static LoginDialog createPairDialog()
	{
		LoginDialog loginDialog = UnityEngine.Object.Instantiate(Instance.PairDialogPrefab);
		loginDialog.transform.SetParent(Instance.gameObject.transform, worldPositionStays: false);
		return loginDialog;
	}

	public static Menu createMenu()
	{
		Menu menu = UnityEngine.Object.Instantiate(Instance.MenuPrefab);
		menu.transform.SetParent(Instance.gameObject.transform, worldPositionStays: false);
		return menu;
	}

	public static MenuBar createMenuBar()
	{
		MenuBar menuBar = UnityEngine.Object.Instantiate(Instance.MenuBarPrefab);
		menuBar.transform.SetParent(Instance.gameObject.transform, worldPositionStays: false);
		return menuBar;
	}

	public static ToolBar createToolBar()
	{
		ToolBar toolBar = UnityEngine.Object.Instantiate(Instance.ToolBarPrefab);
		toolBar.transform.SetParent(Instance.gameObject.transform, worldPositionStays: false);
		return toolBar;
	}

	public static void getSaveFile(string text, string extensions, Action<string> callback)
	{
		createFileDialog().show(FileDialogMode.Save, text, extensions, callback);
	}

	public static void getLoadFile(string text, string extensions, Action<string> callback)
	{
		createFileDialog().show(FileDialogMode.Load, text, extensions, callback);
	}

	public static void selectPath(string text, string extensions, Action<string> callback)
	{
		createFileDialog().show(FileDialogMode.Path, text, extensions, callback);
	}

	public static void error(string text)
	{
		MessageDialog messageDialog = createMessageDialog();
		messageDialog.show(text, (Buttons r) =>
		{
		}, messageDialog.ErrorSprite);
	}

	public static void warning(string text)
	{
		MessageDialog messageDialog = createMessageDialog();
		messageDialog.show(text, (Buttons r) =>
		{
		}, messageDialog.WarningSprite);
	}

	public static void info(string text)
	{
		MessageDialog messageDialog = createMessageDialog();
		messageDialog.show(text, (Buttons r) =>
		{
		}, messageDialog.InfoSprite);
	}

	public static void confirm(string text, Action<Buttons> callback)
	{
		MessageDialog messageDialog = createMessageDialog();
		messageDialog.show(text, callback, messageDialog.QuestionSprite, Buttons.Yes | Buttons.No);
	}

	public static void notify(string text, Sprite icon = null)
	{
		createNotificationMessage().show(text, icon);
	}

	public static void getString(string text, string defaultText, Action<string> callback, InputType type = InputType.String)
	{
		createInputDialog().getString(text, defaultText, callback, type);
	}

	public static void getFloat(string text, float defaultValue, float minValue, float maxValue, Action<float> callback)
	{
		createInputDialog().getFloat(text, defaultValue, minValue, maxValue, callback);
	}

	public static void getInt(string text, int defaultValue, int minValue, int maxValue, Action<int> callback)
	{
		createInputDialog().getInt(text, defaultValue, minValue, maxValue, callback);
	}

	public static void getChoice(string text, int defaultChoice, List<string> choices, Action<int> callback)
	{
		createInputDialog().getChoice(text, defaultChoice, choices, callback);
	}

	public static void getLogin(string text, string defaultUserName, string defaultPassword, Action<string, string> callback)
	{
		createLoginDialog().getLogin(text, defaultUserName, defaultPassword, callback);
	}

	public static void getPair(string text, string defaultName, string defaultValue, Action<string, string> callback)
	{
		createPairDialog().getLogin(text, defaultName, defaultValue, callback);
	}

	public static void clear()
	{
		foreach (Transform item in Instance.transform)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
	}
}
