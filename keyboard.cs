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
			World.MoveCursor(-1, 0);
		else if (info.IsKeyPressed(Keys.Right))
			World.MoveCursor(1, 0);
		if (info.IsKeyPressed(Keys.Up))
			World.MoveCursor(0, -1);
		else if (info.IsKeyPressed(Keys.Down))
			World.MoveCursor(0, 1);
		// ditto for zoom keys
		if (info.IsKeyPressed(Keys.OemPlus))
			World.Zoom(1);
		else if (info.IsKeyPressed(Keys.OemMinus))
			World.Zoom(-1);
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