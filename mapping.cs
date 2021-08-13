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
			new Tuple<string, Func<WorldTile, Color>>("Country", ColorCountry),
			new Tuple<string, Func<WorldTile, Color>>("Potential ET Ratio", ColorPETRatio),
			new Tuple<string, Func<WorldTile, Color>>("Precipitation", ColorPrecipitation),
			new Tuple<string, Func<WorldTile, Color>>("Resource", ColorResource),
			new Tuple<string, Func<WorldTile, Color>>("River Inflow", ColorRiver),
			new Tuple<string, Func<WorldTile, Color>>("Satellite", ColorSatellite),
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
		/// <summary>
		/// 0 = red (-> orange)
		/// 1 = (magenta ->) red
		/// </summary>
		static Color Rainbow(double x){
			x = Program.Clamp(x, 0, 1);
			// r -> y
			if (x < 1/6.0)
				return new Color(255, (byte)(6*x*255), 0);
			// y -> g
			if (x < 1/3.0)
				return new Color((byte)(6*(0.5-x)*255), 255, 0);
			// g -> c
			if (x < 0.5)
				return new Color(0, 255, (byte)(6*(x-0.5)*255));
			// c -> b
			if (x < 2/3.0)
				return new Color(0, (byte)(6*(2/3.0-x)*255), 255);
			// b -> m
			if (x < 5/6.0)
				return new Color((byte)(6*(x-5/6.0)*255), 0, 255);
			// m -> r
			return new Color(255, 0, (byte)(6*(1-x)*255));
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
		static Color ColorCountry(WorldTile w){
			// outflow direction points back to inflow?
			int countryID = People.Country.CountryAtTile(w);
			if (countryID < 0)
				return Color.Black;
			return Rainbow((double)countryID/People.Country.maxCountries);
			// return !w.isLand ? Color.Blue : Resource.resources.Any(r => r.TileTest(w)) ? Color.Lime : Color.Red;
			// return !w.isLand ? Color.Blue : Resource.silk.TileTest(w) ? Color.Lime : Color.Red;
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
		static Color ColorPETRatio(WorldTile w){
			if (!w.isLand)
				return Color.White;
			return Heat(w.potential_evaporation/w.annual_rainfall);
		}
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
		static readonly Color[] blueMarbleColors = new Color[]{
			new Color(5, 8, 23), // SEA
			new Color(45, 53, 22), // Af
			new Color(29, 42, 13), // Am
			new Color(53, 58, 26), // Aw
			new Color(114, 100, 65), // BWh
			new Color(68, 69, 38), // BWk
			new Color(67, 66, 34), // BSh
			new Color(66, 67, 35), // BSk
			new Color(56, 60, 28), // Csa
			new Color(53, 58, 26), // Csb = Csc = Csd
			new Color(53, 58, 26),
			new Color(53, 58, 26),
			new Color(55, 59, 27), // CWa = Cwb = Cwc
			new Color(55, 59, 27),
			new Color(55, 59, 27),
			new Color(52, 58, 26), // Cfa = Cfb = Cfc
			new Color(52, 58, 26),
			new Color(52, 58, 26),
			new Color(59, 62, 30), // Dsa
			new Color(54, 59, 27), // Dsb
			new Color(52, 58, 26), // Dsc
			new Color(53, 58, 26), // Dsd
			new Color(54, 60, 27), // Dwa
			new Color(55, 61, 28), // Dwb
			new Color(51, 57, 26), // Dwc
			new Color(59, 65, 36), // Dwd
			new Color(55, 61, 28), // Dfa
			new Color(46, 55, 23), // Dfb
			new Color(42, 50, 22), // Dfc
			new Color(47, 54, 25), // Dfd
			new Color(65, 69, 39), // ET
			new Color(164, 167, 155), // EF
		};
		static readonly string[] blueMarbleClimateList = new string[]{
			"Af",
			"Am",
			"Aw",
			"BWh",
			"BWk",
			"BSh",
			"BSk",
			"Csa",
			"Csb",
			"Csc",
			"Csd",
			"Cwa",
			"Cwb",
			"Cwc",
			"Cfa",
			"Cfb",
			"Cfc",
			"Dsa",
			"Dsb",
			"Dsc",
			"Dsd",
			"Dwa",
			"Dwb",
			"Dwc",
			"Dwd",
			"Dfa",
			"Dfb",
			"Dfc",
			"Dfd",
			"ET",
			"EF",
		};
		public static Color ColorSatellite(WorldTile w){
			// todo antialiasing
			// increase this if the sea is too dark
			double sea_brightness_mul = 5;
			if (!w.isLand && -200 <= w.temperature.Max()){
				Color sea = blueMarbleColors[0];
				sea.R = (byte)(sea_brightness_mul*sea.R);
				sea.G = (byte)(sea_brightness_mul*sea.G);
				sea.B = (byte)(sea_brightness_mul*sea.B);
				return sea;
			}
			Color color = blueMarbleColors[blueMarbleClimateList.ToList().IndexOf(w.climate)+1];
			// okay now randomly almost-normalize the value/saturation
			// lower this if the land is too bright
			double max_almost_normalization = 1;
			// max RGB
			byte max = Math.Max(color.R, Math.Max(color.G, color.B));
			// theoretical max increase
			double d_max = (1-max_almost_normalization)+max_almost_normalization*(255.0/max);
			Tuple<double, double, double> xyz = Program.LatLong2Spherical(w.y*Math.PI, w.x*Math.PI);
			// increase this if detail not fine enough
			double scale = 4;
			// actual random increase
			// increase octaves if variation too extreme or not enough detail
			double random = (Noise.Simplex.OctaveNoise(xyz.Item1*scale, xyz.Item2*scale, xyz.Item3*scale, 12, 2)+1)/2;
			double d = 1+random*(d_max-1);
			color.R = (byte)(color.R*d);
			color.G = (byte)(color.G*d);
			color.B = (byte)(color.B*d);
			return color;
		}
		static Color ColorSatelliteAA(WorldTile w){
			// todo antialiasing
			Color[] neighborColors = w.deltaNeighbors.Append(w).Select(t => ColorSatellite(t)).ToArray();
			double r = neighborColors.Average(c => c.R);
			double g = neighborColors.Average(c => c.G);
			double b = neighborColors.Average(c => c.B);
			return new Color((byte)r, (byte)g, (byte)b);
		}
		static Color ColorTest(WorldTile w){
			return ColorSatelliteAA(w);
			// return !w.isLand ? Color.Blue : Color.Lime;
		}
	}
}