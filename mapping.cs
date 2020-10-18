using System;
using System.Collections.Generic; // dicts
using System.Linq; // enumerable operations
using Resources;
// color
using Microsoft.Xna.Framework;

namespace Mappings {
	static class Mapping {
		static readonly Tuple<string, Func<WorldTile, int>>[] char_modes = new Tuple<string, Func<WorldTile, int>>[]{
			new Tuple<string, Func<WorldTile, int>>("Altitude", CharAltitude),
			new Tuple<string, Func<WorldTile, int>>("Block", CharBlock),
			new Tuple<string, Func<WorldTile, int>>("River", CharRiver),
			new Tuple<string, Func<WorldTile, int>>("Dwarf Fortress", CharDF),
		};
		static readonly Tuple<string, Func<WorldTile, Color>>[] color_modes = new Tuple<string, Func<WorldTile, Color>>[]{
			new Tuple<string, Func<WorldTile, Color>>("Default", ColorDefault),
			new Tuple<string, Func<WorldTile, Color>>("Altitude", ColorAltitude),
			new Tuple<string, Func<WorldTile, Color>>("Biome", ColorHoldridge),
			new Tuple<string, Func<WorldTile, Color>>("Climate", ColorKoppen),
			// new Tuple<string, Func<WorldTile, Color>>("Potential ET Ratio", ColorPETRatio),
			new Tuple<string, Func<WorldTile, Color>>("Precipitation", ColorPrecipitation),
			new Tuple<string, Func<WorldTile, Color>>("Resource", ColorResource),
			new Tuple<string, Func<WorldTile, Color>>("River Inflow", ColorRiver),
			new Tuple<string, Func<WorldTile, Color>>("Temperature", ColorTemperature),
			new Tuple<string, Func<WorldTile, Color>>("Temperature Variation", ColorTemperatureVariation),
			new Tuple<string, Func<WorldTile, Color>>("Dwarf Fortress", ColorDF),
			new Tuple<string, Func<WorldTile, Color>>("MOCHA_DEBUG", ColorTest),
		};
		static int selected_char_mode = 0;
		static int selected_color_mode = 0;
		public static Func<WorldTile, int> char_mode {
			get { return Mapping.char_modes[selected_char_mode].Item2; }
		}
		public static Func<WorldTile, Color> color_mode {
			get { return Mapping.color_modes[selected_color_mode].Item2; }
		}
		public static string char_mode_name {
			get { return Mapping.char_modes[selected_char_mode].Item1; }
		}
		public static string color_mode_name {
			get { return Mapping.color_modes[selected_color_mode].Item1; }
		}
		public static void CycleChar(int n){
			selected_char_mode = Program.Mod(selected_char_mode+n, char_modes.Length);
			Program.Log("Mapmode char changed to " + char_mode_name);
		}
		public static void CycleChar(){
			CycleChar(1);
		}
		public static void CycleColor(int n){
			selected_color_mode = Program.Mod(selected_color_mode+n, color_modes.Length);
			Program.Log("Mapmode color changed to " + color_mode_name);
		}
		public static void CycleColor(){
			CycleColor(1);
		}
		public static void Debug(){
			selected_char_mode = 1;
			selected_color_mode = color_modes.Length-1;
		}
		/// <summary>
		/// x < 0 => sea; [-1, 0) [dark blue, blue, cyan)
		/// else land; d. green -> green -> yellow -> red -> magenta [0, 1]
		/// the discontinuity at 0 is intentional and is for clarity
		/// </summary>
		static Color Heat(double x){
			x = Program.Clamp(x, -1, 1);
			if (x < -0.5) // dark blue -> blue
				return new Color(0, 0, (int)Program.Remap(x, -1, -0.5, 128, 255));
			if (x < 0) // blue -> cyan
				return new Color(0, (int)Program.Remap(x, -0.5, 0, 0, 255), 255);
			if (x < 0.25) // dark green -> green
				return new Color(0, (int)Program.Remap(x, 0, 0.25, 128, 255), 0);
			if (x < 0.5) // green -> yellow
				return new Color((int)Program.Remap(x, 0.25, 0.5, 0, 255), 255, 0);
			if (x < 0.75) // yellow -> red
				return new Color(255, (int)Program.Remap(x, 0.5, 0.75, 255, 0), 0);
			// red -> magenta
			return new Color(255, 0, (int)Program.Remap(x, 0.75, 1, 0, 255));
		}
		// char selectors
		static int CharAltitude(WorldTile w){
			// uses df-inspired legend
			if (w.isLand){
				// write symbol
				if (4000 < w.elevation)
					return 30;
				if (WorldTile.mountain_altitude < w.elevation)
					return 127;
				if (2000 < w.elevation)
					return 239;
				if (1000 < w.elevation)
					return 'n';
				if (500 < w.elevation)
					return  252;
				return 7; // •
			}
			// sea tiles
			if (w.temperature.Max() < -200) // freezing point of seawater
				return 178;
			switch (w.oceanicZone[0]){
				case 'A':
					return 'v';
				case 'E':
					return '~';
				case 'H':
					return 'V';
				default:
					return 247;
			}
		}
		static int CharBlock(WorldTile w){
			return 219;
		}
		static int CharDF(WorldTile w){
			// uses df legend
			// freezing point of seawater
			if (!w.isLand)
				return w.temperature.Max() < -200 ? 177 : 247;
			// land
			// mountains
			if (4000 <= w.elevation)
				return 30;
			if (WorldTile.mountain_altitude <= w.elevation)
				return 127;
			// non-mountains
			Tuple<int, int> h = w.holdridgeCoords;
			if (h.Item1 + h.Item2 < -4 && w.average_temperature < 1000) // glacier
				return 178;
			if (h.Item1 + h.Item2 == -4) // tundra
				return '.';
			if (h.Item1 < 0){ // forests
				switch (h.Item1+h.Item2){
					case -3:
						return h.Item1 < -2 ? 23 : 24;
					case -2:
						return 5;
					case -1:
						return 6;
					default:
						return 226;
				}
			}
			if (-4 < h.Item2){ // shrubland grassland savanna
				switch (h.Item2){
					case -3:
						return '.';
					case -2:
						return 252;
					case -1:
						return '"';
					default:
						return 231;
				}
			}
			// deserts
			return w.elevation < 1000 ? '~' : 247;
		}
		static int CharRiver(WorldTile w){
			return w.isLand ? w.slope.Item2 : (char)247;
		}
		// coloration
		// todo: pure df legend; works exactly like DF
		static Color ColorAltitude(WorldTile w){
			if (!w.isLand)
				return Heat(-2000 < w.elevation ? Program.Remap(w.elevation, -2000, 0, -0.5, 0)
					: Program.Remap(w.elevation, WorldTile.altitude_min, -2000, -1, -0.5));
			return Heat(Program.Remap(w.elevation, 0, WorldTile.altitude_max, 0, 1));
		}
		static Color ColorDefault(WorldTile w){
			// uses df-inspired legend
			if (w.isLand){
				// get color
				if (4000 <= w.elevation && w.elevation < 5000)
					return Color.Gray;
				if (WorldTile.mountain_altitude < w.elevation)
					return Color.Silver;
				if (w.climate[0] == 'A')
					return Color.Green;
				if (w.climate == "BWh")
					return Color.Red;
				if (w.climate == "BWk")
					return Color.Maroon;
				if (w.climate == "BSh")
					return Color.Yellow;
				if (w.climate == "BSk")
					return Color.Olive;
				if (w.climate[0] == 'D' || w.climate[0] == 'E')
					return Color.Cyan;
				return Color.Lime;
			}
			// sea
			if (w.temperature.Max() < -200) // freezing point of seawater
				return Color.Teal;
			switch (w.oceanicZone[0]){
				case 'E':
				case 'M':
					return Color.Blue;
				default:
					return Color.Navy;
			}
		}
		static Color ColorDF(WorldTile w){
			// uses df legend
			// freezing point of seawater
			if (!w.isLand)
				return w.temperature.Max() < -200 ? Color.Cyan : Color.DarkBlue;
			// land
			// mountains
			if (4000 <= w.elevation && w.elevation < 5000)
				return Color.Gray;
			if (WorldTile.mountain_altitude <= w.elevation)
				return Color.Silver;
			// non-mountains
			Tuple<int, int> h = w.holdridgeCoords;
			if (h.Item1 + h.Item2 < -3 && w.average_temperature < 1000) // glacier + tundra
				return Color.Cyan;
			if (h.Item1 < 0) // forests
				return Color.Green;
			if (-4 < h.Item2) // shrubland grassland savanna
				return h.Item1 == 0 ? Color.Lime : Color.Yellow;
			// deserts
			switch (h.Item1){
				case 1:
					return Color.Yellow;
				case 2:
					return Color.Red;
				case 3:
					return Color.Gray;
				default:
					return Color.White;
			}
		}
		static readonly Dictionary<string, Color> koppen = new Dictionary<string, Color>(){
			{"Af", Color.Blue},
			{"Am", new Color(0, 120, 255)},
			{"Aw", new Color(70, 170, 250)},
			{"BWh", Color.Red},
			{"BWk", new Color(255, 150, 150)},
			{"BSh", Color.Orange},
			{"BSk", new Color(255, 220, 100)},
			{"Csa", Color.Yellow},
			{"Csb", new Color(192, 192, 0)},
			{"Csc", Color.Olive},
			{"Cwa", new Color(150, 255, 150)},
			{"Cwb", new Color(99, 199, 100)},
			{"Cwc", new Color(50, 150, 51)},
			{"Cfa", new Color(198, 255, 78)},
			{"Cfb", Color.Lime},
			{"Cfc", new Color(51, 199, 1)},
			{"Dsa", Color.Magenta},
			{"Dsb", new Color(198, 0, 199)},
			{"Dsc", new Color(150, 50, 149)},
			{"Dsd", new Color(150, 100, 149)},
			{"Dwa", new Color(171, 177, 255)},
			{"Dwb", new Color(90, 119, 219)},
			{"Dwc", new Color(76, 81, 181)},
			{"Dwd", new Color(50, 0, 135)},
			{"Dfa", Color.Cyan},
			{"Dfb", new Color(56, 199, 255)},
			{"Dfc", Color.Teal},
			{"Dfd", new Color(0, 69, 94)},
			{"EF", Color.Gray},
			{"ET", Color.Silver},
		};
		static Color ColorKoppen(WorldTile w){
			return w.isLand ? koppen[w.climate] : Color.White;
		}
		static Color ColorHoldridge(WorldTile w){
			if (!w.isLand)
				return Color.Black;
			Tuple<int, int> h = w.holdridgeCoords;
			int a = h.Item1; int b = h.Item2;
			if (a + b < -4)
				return Color.White;
			// conform to triangle at sides
			a = Program.Clamp(a, -3, 4);
			b = Program.Clamp(b, -4, 3);
			// conform to triangle at bottom
			int c = a + b;
			if (0 < c){
				a -= c/2;
				b -= c/2 + (c % 2 == 0 ? 0 : 1);
				c = 0;
			}
			// get index
			int red = 32*a + 127;
			int green = 32*c + 255;
			int blue = a == -2 ? 144 : a == -3 ? 192 : 128;
			// Program.Log(String.Format("{0}+{1}={2} -> ({3}, {4}, {5})", a, b, c, red, green, blue));
			return new Color(red, green, blue);
		}
		static Color ColorPrecipitation(WorldTile w){
			if (!w.isLand)
				return Color.White;
			return Heat(Program.Remap(Math.Sqrt(w.annual_rainfall), 0, Math.Sqrt(WorldTile.rainfall_max), 0, 1));
		}
		/*
		static Color ColorPETRatio(WorldTile w){
			int etc = w.holdridgeCoords.Item1;
			if (!w.isLand)
				return Color.White;
			switch (etc){
				case 3:
					return heat[1];
				case 2:
					return heat[2];
				case 1:
					return heat[3];
				case 0:
					return heat[4];
				case -1:
					return heat[5];
				case -2:
					return heat[6];
				default:
					return heat[etc < 0 ? 7 : 0];
			}
				
		}*/
		static Color ColorResource(WorldTile w){
			return !w.isLand ? Color.Black : w.resource == null ? Color.White : w.resource.color;
		}
		static Color ColorRiver(WorldTile w){
			return !w.isLand ? Color.Black : Heat((double)w.river_inflow/WorldTile.rainfall_max);
		}
		static Color ColorTemperature(WorldTile w){
			if (!w.isLand)
				return Color.White;
			double t = w.average_temperature/100; // in celsius, no *100 modifier anymore
			return Heat(Program.Remap(w.average_temperature, WorldTile.temperature_min, WorldTile.temperature_max, 0, 1));
		}
		static Color ColorTemperatureVariation(WorldTile w){
			if (!w.isLand)
				return Color.White;
			int t = w.temperature.Max() - w.temperature.Min();
			return Heat(Program.Remap(t, 0, WorldTile.temperature_anomaly, 0, 1));
		}
		static Color ColorTest(WorldTile w){
			// outflow direction points back to inflow?
			return !w.isLand ? Color.Blue : w.downstream.downstream == w ? Color.Red : Color.Lime;
			// return !w.isLand ? Color.Blue : Resource.resources.Any(r => r.TileTest(w)) ? Color.Lime : Color.Red;
			// return !w.isLand ? Color.Blue : Resource.silk.TileTest(w) ? Color.Lime : Color.Red;
		}
	}
}