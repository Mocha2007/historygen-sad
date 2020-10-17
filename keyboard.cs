using SadConsole.Components;
// using SadConsole.Input;
using Microsoft.Xna.Framework.Input;
using Keyboard = SadConsole.Input.Keyboard;
using Mappings;
class Mokey : KeyboardConsoleComponent
{
	public override void ProcessKeyboard(SadConsole.Console console, Keyboard info, out bool handled)
	{
		handled = true;
		// movement keys can use else ifs since they are contradictory
		if (info.IsKeyPressed(Keys.Left))
			Program.world.MoveCursor(-1, 0);
		else if (info.IsKeyPressed(Keys.Right))
			Program.world.MoveCursor(1, 0);
		if (info.IsKeyPressed(Keys.Up))
			Program.world.MoveCursor(0, -1);
		else if (info.IsKeyPressed(Keys.Down))
			Program.world.MoveCursor(0, 1);
		// ditto for zoom keys
		if (info.IsKeyPressed(Keys.OemPlus))
			Program.world.Zoom(1);
		else if (info.IsKeyPressed(Keys.OemMinus))
			Program.world.Zoom(-1);
		// other keys can be pressed simultaneously
		if (info.IsKeyPressed(Keys.X)){
			Mapping.CycleChar();
			Program.world.Print();
		}
		if (info.IsKeyPressed(Keys.Z)){
			Mapping.CycleColor();
			Program.world.Print();
		}
		if (info.IsKeyPressed(Keys.Escape)){
			Program.Exit();
		}
	}
}