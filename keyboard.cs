using SadConsole.Components;
// using SadConsole.Input;
using Microsoft.Xna.Framework.Input;
using Keyboard = SadConsole.Input.Keyboard;
class Mokey : KeyboardConsoleComponent
{
	public override void ProcessKeyboard(SadConsole.Console console, Keyboard info, out bool handled)
	{
		handled = true;
		if (info.IsKeyPressed(Keys.Left))
			World.MoveCursor(-1, 0);
	}
}
/*
			switch (Console.ReadKey().Key){
				case ConsoleKey.LeftArrow:
					CoverHighlight();
					cursor_x = Program.Mod(cursor_x-1, size*2);
					break;
				case ConsoleKey.RightArrow:
					CoverHighlight();
					cursor_x = Program.Mod(cursor_x+1, size*2);
					break;
				case ConsoleKey.UpArrow:
					CoverHighlight();
					if (0 < cursor_y)
						cursor_y--;
					break;
				case ConsoleKey.DownArrow:
					CoverHighlight();
					if (cursor_y < size - 1)
						cursor_y++;
					break;
				case ConsoleKey.OemPlus:
					if (WorldTile.minimap_scale < 32){
						WorldTile.octaves++;
						WorldTile.minimap_scale *= 2;
					}
					break;
				case ConsoleKey.OemMinus:
					if (1 < WorldTile.minimap_scale){
						WorldTile.octaves--;
						WorldTile.minimap_scale /= 2;
					}
					break;
				case ConsoleKey.X:
					Mapping.CycleChar();
					Draw();
					break;
				case ConsoleKey.Z:
					Mapping.CycleColor();
					Draw();
					break;
				case ConsoleKey.Escape:
					Program.Exit();
					break;
			}
*/