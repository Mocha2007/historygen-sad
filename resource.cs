using System;
using System.Collections.Generic; // lists
using System.Linq; // enumerable operations
// color
using Microsoft.Xna.Framework;

namespace Resources {
	static class ResourceRNG {
		static int iteration = 0;
		public static void SimplexShuffle<T>(this IList<T> list, double x, double y){
			// https://stackoverflow.com/a/1262619/2579798
			int n = list.Count;  
			while (n > 1) {  
				n--;  
				int k = Next(n + 1, x, y);  
				T value = list[k];  
				list[k] = list[n];  
				list[n] = value;  
			}
		}
		static double Next(double x, double y){
			Tuple<double, double, double> xyz = Program.LatLong2Spherical(y*Math.PI, x*Math.PI);
			iteration++;
			return Noise.Simplex.Noise(xyz.Item1, xyz.Item2, xyz.Item3, iteration);
		}
		static int Next(int max, double x, double y){
			return (int)(Next(x, y) * max);
		}
		public static void Reset(){
			iteration = 0;
		}

	}
	class Resource {
		public static readonly List<Resource> resources = new List<Resource>();
		public readonly string name;
		/// <summary>
		/// Used to determine whether a province is capable of having this resource.
		/// </summary>
		public readonly Func<WorldTile, bool> TileTest;
		/// <summary>
		/// The weight the RNG will use for this tile is 2^abundance_exponent
		/// </summary>
		readonly byte abundance_exponent;
		public readonly Color color;
		Resource(string name, Func<WorldTile, bool> TileTest, byte abundance_exponent, Color c){
			this.name = name;
			this.TileTest = TileTest;
			this.abundance_exponent = abundance_exponent;
			color = c;
			if (c == Color.Black) // used for sea
				throw new ArgumentOutOfRangeException("color of resource cannot be black, as that is used for the sea");
			resources.Add(this);
		}
		public double worldgen_weight {
			get { return Math.Pow(2, abundance_exponent); }
		}
		public int id {
			get { return resources.IndexOf(this); }
		}
		// STATIC METHODS
		public static string PrettyList(IEnumerable<Resource> rs){
			return String.Join(" ", rs.Select(r => "" + r.name[0] + r.name[r.name.Length-1]));
		}
		// NON-STATIC METHODS
		// list of resources
		static readonly Resource cinnamon = new Resource("cinnamon",
			w => {
				Tuple<int, int> h = w.holdridgeCoords;
				return h.Item1 == -1 && -2 < h.Item1+h.Item2;
			} // based on modern cultivation range
			, 7, new Color(210, 105, 30)); // guess
		static readonly Resource coal = new Resource("coal",
			w => w.HasDeposit(4, 0.5) // was this a wet forest 300 million years ago?
			, 7, new Color(32, 32, 32)); // 179 prov in vicky 2
		static readonly Resource cocoa = new Resource("cocoa",
			w => {
				Tuple<int, int> h = w.holdridgeCoords;
				return h.Item1 < 0 && -1 < h.Item1+h.Item2;
			} // based on native range
			, 7, new Color(114, 53, 22)); // guess
		static readonly Resource coffee = new Resource("coffee",
			w => w.holdridge == "dry forest" // based on native range
			, 7, new Color(56, 38, 22)); // 63 prov in vicky 2
		static readonly Resource copper = new Resource("copper",
			w => w.HasDeposit(3, 0.5)
			, 7, new Color(216, 117, 38)); // guess
		static readonly Resource cotton = new Resource("cotton",
			w => {
				Tuple<int, int> hc = w.holdridgeCoords;
				return hc.Item2 < 1 && -2 < hc.Item1+hc.Item2;
			} // "The plant is a shrub native to tropical and subtropical regions around the world"
			, 7, new Color(132, 173, 153)); // 25 Mt/yr in 2011; 106 prov in vicky 2
		static readonly Resource fish = new Resource("fish",
			w => w.elevation < 200 // 150 is slightly too low
			, 8, new Color(140, 209, 247)); // 235 prov in vicky 2
		static readonly Resource furs = new Resource("furs",
			w => {
				Tuple<int, int> hc = w.holdridgeCoords;
				int a = hc.Item1; int b = hc.Item2;
				return (a == -1 || a == -2) && (a+b == -2 || a+b == -3);
			} // appx natural range of beavers
			, 7, new Color(137, 102, 79)); // guess
		static readonly Resource gem = new Resource("gem",
			w => w.HasDeposit(7, 0.5)
			, 3, new Color(244, 198, 198));
		static readonly Resource gold = new Resource("gold",
			w => w.HasDeposit(1, 0.5)
			, 2, new Color(255, 214, 48)); // 3503 t/yr in 2018; 1 prov in vicky 2 = precious metal
		static readonly Resource hemp = new Resource("hemp",
			w => {
				if ( w.temperature.Min() < -500 || 3100 < w.temperature.Max() ) // lost source, but somewhere on wikipedia
					return false;
				Tuple<int, int> hc = w.holdridgeCoords;
				return -1 < hc.Item1 && -2 < hc.Item1+hc.Item2;
			} // guess
			, 6, Color.DarkGreen); // guess
		static readonly Resource incense = new Resource("incense",
			w => {
				Tuple<int, int> h = w.holdridgeCoords;
				return 0 < h.Item1 && -2 < h.Item1+h.Item2;
			} // based on eu4 range
			, 6, new Color(226, 201, 119)); // guess
		public static readonly Resource iron = new Resource("iron",
			w => w.HasDeposit(2, 0.5)
			, 7, new Color(51, 51, 51)); // 1.595 Gt/yr in 2006; 116 prov in vicky 2 = iron
		static readonly Resource ivory = new Resource("ivory",
			w => {
				Tuple<int, int> hc = w.holdridgeCoords;
				return (-2 < hc.Item1 || hc.Item1 < 3) && -1 < hc.Item1+hc.Item2;
			} // appx natural range
			, 6, new Color(191, 178, 160)); // guess
		static readonly Resource marble = new Resource("marble",
			w => w.HasDeposit(8, 0.5)
			, 3, Color.White);
		static readonly Resource opium = new Resource("opium",
			w => {
				Tuple<int, int> hc = w.holdridgeCoords;
				return hc.Item1 + hc.Item2 == -1 && -3 < hc.Item1 && hc.Item1 < 2;
			} // appx native range
			, 6, Color.Green); // guess
		static readonly Resource pepper = new Resource("pepper", // black pepper
			w => {
				Tuple<int, int> h = w.holdridgeCoords;
				return h.Item1 < 2 && h.Item2 < 2 && -2 < h.Item1+h.Item2;
			} // based on native range
			, 6, Color.DarkGray); // guess
		static readonly Resource rubber = new Resource("rubber",
			w => {
				double r = w.annual_rainfall;
				short t_min = w.temperature.Min();
				short t_max = w.temperature.Max();
				return 2000 < r && r < 3000
					&& 2500 <= t_min && t_max <= 2800;
			} // https://en.wikipedia.org/wiki/Natural_rubber#Cultivation
			, 7, Color.DarkBlue); // guess
		static readonly Resource salt = new Resource("salt",
			w => { // desert OR near water
				if (w.elevation < 200)
					return true;
				Tuple<int, int> hc = w.holdridgeCoords;
				return -4 < hc.Item1 + hc.Item2 && hc.Item2 < -3;
			} // appx natural range
			, 7, new Color(254, 254, 254)); // guess
		static readonly Resource silk = new Resource("silk",
			w => {
				Tuple<int, int> hc = w.holdridgeCoords;
				return hc.Item1 + hc.Item2 == -1 && hc.Item1 < 1 && -3 < hc.Item1;
			} // appx natural range
			, 6, new Color(183, 25, 25)); // guess
		static readonly Resource silver = new Resource("silver",
			w => w.HasDeposit(6, 0.5)
			, 3, Color.Gray); // 26.8 kt/yr in 2014; guess
		static readonly Resource sugar = new Resource("sugar",
			w => {
				if (w.annual_rainfall < 600) // https://en.wikipedia.org/wiki/Sugarcane#Cultivation
					return false;
				Tuple<int, int> h = w.holdridgeCoords;
				return -2 < h.Item1+h.Item2;
			} // based on native range
			, 6, new Color(188, 242, 173)); // guess
		static readonly Resource sulfur = new Resource("sulfur",
			w => w.HasDeposit(5, 0.5)
			, 5, Color.Yellow); // 36 prov in vicky 2
		static readonly Resource tea = new Resource("tea",
			w => {
				Tuple<int, int> hc = w.holdridgeCoords;
				return hc.Item1 == -1 && hc.Item2 == 0;
			} // appx natural range
			, 6, new Color(17, 84, 22)); // guess
		static readonly Resource tobacco = new Resource("tobacco",
			w => {
				Tuple<int, int> hc = w.holdridgeCoords;
				return -3 < hc.Item1 + hc.Item2 && (hc.Item2 == -1 || hc.Item2 == 0);
			} // appx cultivation range
			, 6, new Color(84, 142, 96)); // guess
		static readonly Resource truffle = new Resource("truffle",
			w => {
				Tuple<int, int> h = w.holdridgeCoords;
				return h.Item1 == -1 && (h.Item2 == -1 || h.Item2 == 0);
			} // france...?
			, 5, Color.DarkCyan); // guess
		static readonly Resource tropical_wood = new Resource("tropical_wood",
			w => w.holdridgeCoords.Item1 < 0 && w.climate[0] == 'A' // wet && tropical
			, 6, new Color(114, 119, 22)); // guess
		static readonly Resource wine = new Resource("wine",
			w => {
				Tuple<int, int> hc = w.holdridgeCoords;
				return (hc.Item2 == -2 || hc.Item2 == -1) && (hc.Item1+hc.Item2 == -2 || hc.Item1+hc.Item2 == -1);
			} // appx natural range
			, 6, new Color(91, 33, 71)); // guess
		static readonly Resource woad = new Resource("woad",
			w => {
				Tuple<int, int> hc = w.holdridgeCoords;
				return hc.Item2 < 0 && hc.Item1+hc.Item2 == -2;
			} // "Woad is native to the steppe and desert zones of the Caucasus, Central Asia to Eastern Siberia and Western Asia"
			, 5, Color.Blue); // 20 prov in vicky 2 == dye
		static readonly Resource wheat = new Resource("wheat",
			w => {
				Tuple<int, int> hc = w.holdridgeCoords;
				return (hc.Item2 == -2 || hc.Item2 == -1) && (hc.Item1+hc.Item2 == -2 || hc.Item1+hc.Item2 == -1);
			} // appx modern cultivation area
			, 8, new Color(244, 237, 147)); // guess
		// todo: livestock wool
		// Program.Log(String.Format("{0} && {1} == {2}", hc.Item2 < 1, -2 < hc.Item1+hc.Item2, hc.Item2 < 1 && -2 < hc.Item1+hc.Item2));
	}
}
/* COLORS
Black		FORBIDDEN

17	84	22	tea
32	32	32	coal
51	51	51	iron
56	38	22	coffee
84	142	96	tobacco
91	33	71	wine
114	53	22	cocoa
114	119	22	tropical_wood
132	173	153	cotton
137	102	79	furs
140	209	247	fish
183	25	25	silk
188	242	173	sugar
191	178	160	ivory
210	105	30	cinnamon
216	117	38	copper
226	201	119	incense
244	198	198	gems
244	237	147	wheat
254	254	254	salt
255	214	48	gold

DarkCyan	truffle
DarkBlue	rubber
DarkGray	pepper
DarkGreen	hemp
Blue		woad
Gray		silver
Green		opium
Yellow		sulfur
White		marble

ORE IDS
0	FORBIDDEN
1	gold
2	iron
3	copper
4	coal
5	sulfur
6	silver
7	gems
8 	marble
*/