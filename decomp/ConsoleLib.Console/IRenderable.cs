namespace ConsoleLib.Console;

public interface IRenderable
{
	string getTile();

	string getRenderString();

	string getColorString();

	string getTileColor();

	char getDetailColor();

	ColorChars getColorChars();

	bool getHFlip();

	bool getVFlip();
}
