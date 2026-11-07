using UnityEngine;

namespace RedShadow.CommonDialogs;

public class DialogTest : MonoBehaviour
{
	public Sprite Sprite;

	public ComplexDialogExample ComplexDialog;

	private int _counter;

	public void Update()
	{
		ProgressDialog progressDialog = DialogBase.getTopmost() as ProgressDialog;
		if ((bool)progressDialog && Input.GetKeyDown(KeyCode.Escape))
		{
			progressDialog.close();
		}
	}

	public void testSave()
	{
		DialogManager.getSaveFile("Save File Test", "*", (string s) =>
		{
			DialogManager.notify("Save : " + s, Sprite);
		});
	}

	public void testLoad()
	{
		DialogManager.getLoadFile("Load File Test", "*", (string s) =>
		{
			DialogManager.notify("Load : " + s, Sprite);
		});
	}

	public void testStringInput()
	{
		DialogManager.getString("String input:", "test string", (string s) =>
		{
			DialogManager.notify("String Input : " + s, Sprite);
		});
	}

	public void testLogin()
	{
		DialogManager.getLogin("Login Test", "username", "password", (string u, string p) =>
		{
			DialogManager.notify("UserName : " + u, Sprite);
		});
	}

	public void testNotification()
	{
		_counter++;
		DialogManager.notify("Hello\n   World!!!!!\nCounter: " + _counter, Sprite);
	}

	public void testProgress()
	{
		DialogManager.createProgressDialog().show("Testing progress dialog...\nPress Escape key to exit.", indeterminate: true);
	}

	public void testComplexDialog()
	{
		ComplexDialog.showDialog();
	}
}
