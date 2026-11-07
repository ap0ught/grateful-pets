using UnityEngine;

namespace RedShadow.CommonDialogs;

public class ComplexDialogExample : DialogBase
{
	public TitleBar TitleBar;

	public MenuBar MenuBar;

	public ToolBar ToolBar;

	public StatusBar StatusBar;

	public Sprite FileNewIcon;

	public Sprite FileOpenIcon;

	public Sprite FileSaveIcon;

	public Sprite FileSaveAsIcon;

	public Sprite FileExitIcon;

	public Sprite EditUndoIcon;

	public Sprite EditRedoIcon;

	public Sprite EditCopyIcon;

	public Sprite EditCutIcon;

	public Sprite EditPasteIcon;

	public Sprite EditDeleteIcon;

	public Sprite EditSelectAllIcon;

	public Sprite EditSettingsIcon;

	public override void Start()
	{
		setupMenuBar();
		setupToolBar();
		if (TitleBar != null)
		{
			TitleBar.onClose.AddListener(cancel);
			TitleBar.setTitleText("Text Editor");
		}
	}

	public void showDialog()
	{
		base.show();
	}

	private void setupMenuBar()
	{
		Menu menu = DialogManager.createMenu();
		menu.addItem(FileNewIcon, "New").setHotKeyText("Ctrl+N").setCallback(exampleMenuItemHandler);
		menu.addItem(FileOpenIcon, "Open..").setHotKeyText("Ctrl+O").setCallback(exampleMenuItemHandler);
		menu.addItem(FileSaveIcon, "Save").setHotKeyText("Ctrl+S").setCallback(exampleMenuItemHandler);
		menu.addItem(FileSaveAsIcon, "Save As...").setCallback(exampleMenuItemHandler);
		menu.addSeparator();
		menu.addItem(FileExitIcon, "Exit").setHotKeyText("Alt+F4").setCallback(cancel);
		MenuBar.addMenu("File", menu);
		Menu menu2 = DialogManager.createMenu();
		menu2.addItem(EditUndoIcon, "Undo").setHotKeyText("Ctrl+Z").setCallback(exampleMenuItemHandler);
		menu2.addItem(EditRedoIcon, "Redo").setHotKeyText("Ctrl+Y").setCallback(exampleMenuItemHandler);
		menu2.addSeparator();
		menu2.addItem(EditCutIcon, "Cut").setHotKeyText("Ctrl+X").setCallback(exampleMenuItemHandler);
		menu2.addItem(EditCopyIcon, "Copy").setHotKeyText("Ctrl+C").setCallback(exampleMenuItemHandler);
		menu2.addItem(EditPasteIcon, "Paste").setHotKeyText("Ctrl+V").setCallback(exampleMenuItemHandler);
		menu2.addItem(EditDeleteIcon, "Delete").setHotKeyText("Del").setCallback(exampleMenuItemHandler);
		menu2.addSeparator();
		menu2.addItem(EditSelectAllIcon, "Select All").setHotKeyText("Ctrl+A").setCallback(exampleMenuItemHandler);
		menu2.addSeparator();
		menu2.addItem(EditSettingsIcon, "Settings").setCallback(exampleMenuItemHandler);
		MenuBar.addMenu("Edit", menu2);
	}

	private void setupToolBar()
	{
		ToolBar.addButton(FileNewIcon, "New").setCallback(exampleMenuItemHandler);
		ToolBar.addButton(FileOpenIcon, "Open").setCallback(exampleMenuItemHandler);
		ToolBar.addButton(FileSaveIcon, "Save").setCallback(exampleMenuItemHandler);
		ToolBar.addButton(FileSaveAsIcon, "Save As").setCallback(exampleMenuItemHandler);
		ToolBar.addSeparator();
		ToolBar.addButton(EditUndoIcon, "Undo").setCallback(exampleMenuItemHandler);
		ToolBar.addButton(EditRedoIcon, "Redo").setCallback(exampleMenuItemHandler);
		ToolBar.addSeparator();
		ToolBar.addButton(EditCutIcon, "Cut").setCallback(exampleMenuItemHandler);
		ToolBar.addButton(EditCopyIcon, "Copy").setCallback(exampleMenuItemHandler);
		ToolBar.addButton(EditPasteIcon, "Paste").setCallback(exampleMenuItemHandler);
		ToolBar.addButton(EditDeleteIcon, "Delete").setCallback(exampleMenuItemHandler);
		ToolBar.addSeparator();
	}

	private void exampleMenuItemHandler()
	{
	}
}
