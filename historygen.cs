using System;
using System.Collections.Generic; // lists
using System.IO; // file reading
using System.Linq; // enumerable operations
using System.Threading.Tasks; // Parallel.ForEach
// other cs files
using Resources;
using Mappings;
using Noise;
// :( console
using SadConsole;
using Microsoft.Xna.Framework;
using Game = SadConsole.Game;
using Console = SadConsole.Console;
// program
class Program {
	public static Random rng;
	public static int seed;
	public static double precomputed_altitude_cutoff;
	public static readonly byte tooltip_width = 32;
	public static Console console;
	public static World world;
	static readonly Tuple<string, byte>[] log_history = new Tuple<string, byte>[20];
	static readonly int window_height = World.size + log_history.Length;
	static readonly int window_width = World.size*2 + tooltip_width;
	static void Main(string[] args){
		People.Person p = People.Person.Random();
		// clear log
		File.WriteAllText("log.txt", "");
		// sadconsole stuff
		Game.Create("fonts/moki_square.font", window_width, window_height);
		Game.Instance.Window.Title = "Mocha's History Generator";
		Game.OnInitialize = InitializeConsole;
		Global.KeyboardState.InitialRepeatDelay /= 2;
		// mapcolor args
		if (args.Contains("-d")) // debug mode
			Mapping.Debug();
		else {
			if (args.Contains("-char"))
				Mapping.CycleChar(int.Parse(args[args.ToList().IndexOf("-char")+1]));
			if (args.Contains("-color"))
				Mapping.CycleColor(int.Parse(args[args.ToList().IndexOf("-color")+1]));
		}
		// seed arg
		if (args.Contains("-s")){
			seed = int.Parse(args[args.ToList().IndexOf("-s")+1]);
			NewSeed(false);
		}
		// Test();
		// Histogram(pac_data);
		// Console.ReadKey();
		Game.Instance.Run();
		Game.Instance.Dispose();
	}
	static void InitializeConsole(){
		console = new Console(window_width, window_height);
		console.IsFocused = true;
		console.Components.Add(new Mokey());
		Log("Awakening the dusklings...");
		Global.CurrentScreen = console;
		new Task(() => { // do async so the window immediately shows up
			// compute altitude cutoff
			seed = (int)DateTime.Now.Ticks;
			rng = new Random(seed);
			List<double> pac_data = Simplex.Test().Select(x => Math.Pow(x, WorldTile.altitude_exponent)).ToList();
			precomputed_altitude_cutoff = Percentile(pac_data, WorldTile.desiredSeaFraction);
			// gen world
			long t_start = DateTime.Now.Ticks;
			world = World.Random();
			long t_end = DateTime.Now.Ticks;
			Log(String.Format("worldgen took {0} ms", (t_end - t_start)/10000));
			world.Print();
		}).Start();
	}
	public static void NewSeed(bool change_seed){
		// Log(String.Format("seed {0} failed", seed), 1);
		if (change_seed)
			seed = rng.Next();
		rng = new Random(seed);
		Simplex.Initialize();
	}
	public static void NewSeed(){
		NewSeed(true);
	}
	public static void Log(object message, byte level){
		// log in file
		File.AppendAllText("log.txt", String.Format("{0} {1} ms - {2}: {3}\r\n", DateTime.Now, DateTime.Now.Millisecond, level, message));
		// clear log
		console.Fill(new Rectangle(0, window_height - log_history.Length, window_width, log_history.Length), Color.Silver, Color.Black, 0, 0);
		for (int i = log_history.Length-1; 0 <= i; i--){
			// select next log message
			Tuple<string, byte> t = i == 0 ? new Tuple<string, byte>(message.ToString(), level) : log_history[i-1];
			if (t != null)
				LogMessage(t.Item1, t.Item2, i);
			// move logs up / add message to log history
			log_history[i] = t;
		}
	}
	static void LogMessage(string message, byte level, int i){
		int y = window_height - i - 1; // messages push up
		switch (level){
			case 1:
				console.Print(0, y, "[warn] ", Color.Yellow, Color.Black);
				break;
			case 2:
				console.Print(0, y, "[err] ", Color.Red, Color.Black);
				break;
			default:
				console.Print(0, y, "[info] ", Color.Cyan, Color.Black);
				break;
		}
		console.Print(7, y, message, Color.Silver, Color.Black);
	}
	public static void Log(object message){
		Log(message, 0);
	}
	public static void Exit(){
		Environment.Exit(1);
	}
	static void Test(){
		File.WriteAllLines("output.txt", Simplex.Test().Select(x => x.ToString()));
		Exit();
	}
	// utilities
	/*
	public static void DrawCharLine(char c, int from_x, int from_y, int to_x, int to_y){
		int chars = Math.Abs(from_y - to_y);
		double x_rate = (double)Math.Abs(from_x - to_x) / chars;
		for (int i = 0; i < chars; i++){
			Console.CursorTop = from_y + i;
			Console.CursorLeft = from_x + (int)(x_rate * i);
			Console.Write(c);
		}
	}
	static void Histogram(IEnumerable<double> data){
		double size = data.ToArray().Length;
		byte bins = 20;
		byte max_height = 75;
		double interval = 1.0 / bins;
		double[] bar_heights = new double[bins];
		for (byte i = 0; i < bins; i++){
			double minimum = interval*i;
			double maximum = interval*(i+1);
			bar_heights[i] = data.Where(x => minimum <= x && x < maximum).ToArray().Length / size;
		}
		double bar_max = bar_heights.Max();
		foreach (double p in bar_heights)
			Console.WriteLine(String.Format("|{0}", new String('█', (int)(p/bar_max*max_height))));
	}
	*/
	// utilities
	public static double Clamp(double x, double min, double max){
		return Math.Min(max, Math.Max(min, x));
	}
	public static int Clamp(int x, int min, int max){
		return Math.Min(max, Math.Max(min, x));
	}
	// theta is lat; phi is lon
	public static Tuple<double, double, double> LatLong2Spherical(double latitude, double longitude){
		double x = Math.Sin(latitude) * Math.Cos(longitude);
		double y = Math.Sin(latitude) * Math.Sin(longitude);
		double z = Math.Cos(latitude);
		return new Tuple<double, double, double>(x, y, z);
	}
	public static int Mod(int x, int m){
		return (x%m + m)%m;
	}
	public static double Percentile(IEnumerable<double> data, double percentile){
		double[] sorted = data.ToArray();
		Array.Sort(sorted);
		int i = (int)(sorted.Length*percentile);
		return sorted[i];
	}
	public static double Remap(double v, double min1, double max1, double min2, double max2){
		return (v-min1)/(max1-min1) * (max2-min2) + min2;
	}
	/*public static Tuple<double, double> Spherical2LatLong(double x, double y, double z){
		double longitude = Math.Atan2(y, x);
		double latitude = Math.Acos(z);
		return new Tuple<double, double>(latitude, longitude);
	}*/
	public static Tuple<double, double> Time2TimeXY(double t){
		return new Tuple<double, double>(Math.Cos(t), Math.Sin(t));
	}
}
class World {
	public static readonly byte size = 64;
	static readonly float seaFraction_tolerance = 0.1F;
	static readonly short min_highest_peak_altitude = 5000;
	readonly WorldTile[,] tiles;
	/// <summary>
	/// a sorted, flattened array of provinces sorted by elevation, from highest to lowest
	/// </summary>
	WorldTile[] elevation_cache;
	World(WorldTile[,] tiles){
		this.tiles = tiles;
		new Task(() => {
			// todo create elevation cache
			// https://stackoverflow.com/a/641565/2579798
			List<WorldTile> temp = tiles.Cast<WorldTile>().ToList();
			temp.Sort((a, b) => a.elevation - b.elevation);
			elevation_cache = temp.ToArray();
			Program.Log("Created elevation cache");
			// todo generate rivers
			foreach (WorldTile t in elevation_cache){
				double outflow = t.river_outflow;
				if (outflow <= 0)
					continue; // it all evaporates/freezes/whatever
				// else, compute outflow direction and push downstream
				t.downstream.river_inflow += (int)outflow;
			}
			Program.Log("Computed rivers");
		}).Start();
	}
	static int tileCount {
		get { return size * size * 2; }
	}
	int cursor_x = size;
	int cursor_y = size/2;
	// static methods
	public static World Random(){
		WorldTile[,] w = new WorldTile[size, size*2];
		bool acceptable = false;
		int failures = 0;
		while (!acceptable){
			Program.Log("Generating new world from seed " + Program.seed);
			// generate new world
			// long t_start = DateTime.Now.Ticks;
			/*
				for, for					=> 279 ms
				Parallel.For, for			=> 100 ms
				Parallel.For, Parallel.For	=> 77 ms
			*/
			Parallel.For(0, size, y => {
				Parallel.For(0, size*2, x => {
					w[y, x] = WorldTile.Random((double)x/size, (double)y/(size-1));
			});});
			// long t_end = DateTime.Now.Ticks;
			// Program.Log(String.Format("took {0} ticks", t_end - t_start));
			// check if valid
			if (Valid(ref w))
				break;
			// otherwise, regen.
			Program.NewSeed();
			failures++;
			if (1000 <= failures){
				Program.Log("No good world in 1,000 tries", 2);
				throw new ArgumentException();
			}
		}
		Program.Log(failures + " failures");
		// create a new thread to continue resource computation in background
		new Task(() => {
			foreach (WorldTile t in w)
				if (t.resource == null)
					Program.Log("tile has no resource (if this message appears, mocha broke the code!)", 1);
			Program.Log("resource generation complete");
		}).Start();
		// return while thread is running
		return new World(w);
	}
	static void ClearTooltip(){
		Program.console.Fill(new Rectangle(size*2, 0, Program.tooltip_width, size), Color.Silver, Color.Black, 0, 0);
	}
	static bool Valid(ref WorldTile[,] w){
		// return true;
		// check sea tile fraction
		int seaTileCount = 0;
		foreach (WorldTile wt in w)
			if (!wt.isLand)
				seaTileCount++;
		double seaTileFraction = (double)seaTileCount/tileCount;
		if (seaTileFraction < WorldTile.desiredSeaFraction - seaFraction_tolerance){
			Program.Log(String.Format("too few sea tiles: {0} not in [{1}, {2}]", seaTileFraction,
				WorldTile.desiredSeaFraction-seaFraction_tolerance, WorldTile.desiredSeaFraction+seaFraction_tolerance));
			Program.precomputed_altitude_cutoff += 0.01; // attempt corrective measures
			return false;
		}
		if (WorldTile.desiredSeaFraction + seaFraction_tolerance < seaTileFraction){
			Program.Log(String.Format("too many sea tiles: {0} not in [{1}, {2}]", seaTileFraction,
				WorldTile.desiredSeaFraction-seaFraction_tolerance, WorldTile.desiredSeaFraction+seaFraction_tolerance));
			Program.precomputed_altitude_cutoff -= 0.01; // attempt corrective measures
			return false;
		}
		// check highest peak
		short highest_peak_altitude = 0;
		foreach (WorldTile wt in w){
			highest_peak_altitude = highest_peak_altitude < wt.elevation ?
				wt.elevation : highest_peak_altitude;
			if (min_highest_peak_altitude < wt.elevation)
				break;
		}
		if (highest_peak_altitude < min_highest_peak_altitude){
			Program.Log(String.Format("bad peak altitude: {0} < {1}", highest_peak_altitude, min_highest_peak_altitude));
			return false;
		}
		// must have one of every resource
		Dictionary<Resource, bool> resource_count = new Dictionary<Resource, bool>();
		foreach (WorldTile wt in w){
			if (!wt.isLand || wt.resource == null)
				continue;
			if (!resource_count.ContainsKey(wt.resource))
				resource_count[wt.resource] = true;
			if (resource_count.Keys.Count == Resource.resources.Count)
				break;
		}
		if (resource_count.Keys.Count < Resource.resources.Count){
			int missing = Resource.resources.Count-resource_count.Keys.Count;
			Program.Log(String.Format("missing {0} resource{1}: {2}", missing, missing == 1 ? "" : "s",
				String.Join(", ", Resource.resources.Where(r => !resource_count.Keys.Contains(r)).Select(r => r.name))
			));
			return false;
		}
		// otherwise, fine
		return true;
	}
	// non-static methods
	public WorldTile GetTileAt(int x, int y){
		if (y < 0){
			y = 0;
			x += size;
		}
		else if (size <= y){
			y = size - 1;
			x += size;
		}
		return tiles[y, Program.Mod(x, size*2)];
	}
	public void MoveCursor(int x, int y){
		// replace old location
		tiles[cursor_y, cursor_x].Print(Mapping.color_mode, Mapping.char_mode, cursor_x, cursor_y);
		// move cursor
		cursor_x = Program.Mod(cursor_x+x, size*2);
		cursor_y = Program.Clamp(cursor_y+y, 0, size-1);
		// draw cursor
		Program.console.SetGlyph(cursor_x, cursor_y, 'X', Color.Magenta);
	}
	public void Print(){
		// big map
		Program.console.Fill(new Rectangle(0, 0, size*2, size), Color.Silver, Color.Black, 0, 0);
		for (byte y = 0; y < size; y++)
			for (short x = 0; x < size*2; x++)
				tiles[y, x].Print(Mapping.color_mode, Mapping.char_mode, x, y);
		// highlight selection
		MoveCursor(0, 0);
		// display tooltip for thing
		RedrawTooltip();
	}
	public void RedrawTooltip(){
		WorldTile selection = tiles[cursor_y, cursor_x];
		ClearTooltip();
		selection.Tooltip();
	}
	public void Zoom(int z){
		if (0 < z){
			if (WorldTile.minimap_scale < 32){
				WorldTile.octaves++;
				WorldTile.minimap_scale *= 2;
			}
			Zoom(z-1);
		}
		else if (z < 0){
			if (1 < WorldTile.minimap_scale){
				WorldTile.octaves--;
				WorldTile.minimap_scale /= 2;
			}
			Zoom(z+1);
		}
		// else pass
		RedrawTooltip();
	}
}

class WorldTile {
	// todo: rivers, rain shadow
	// todo: perlin offset
	public static byte octaves = 4; // lowest to average less than 1 failure, due to getting closer to being a normal distribution
	public static byte minimap_scale = 4;
	static readonly double circumference = 40075.017;
	// elevation - m; rainfall - mm; temperature - degrees celsius * 100
	public readonly short elevation;
	readonly double x, y;
	Resource resource_cache;
	public int river_inflow; // how many mm anually get converted into streams and rivers
	readonly short[] rainfall;
	public readonly short[] temperature;
	public static readonly float desiredSeaFraction = 0.45F; // 0.4 is about perfect; must be in (0, 0.92]
	public static readonly short mountain_altitude = 3000; // for rain shadows
	// alt
	public static readonly double altitude_exponent = 1.2; // in my experience, has the lowest failure rate
	public static readonly short altitude_min = -11000;
	public static readonly short altitude_max = 9000;
	static readonly byte altitude_scale = 1;
	// rf
	// 5 generates a realistic amount of magenta-level regions
	// 9 is too large, and worlds don't gen with all the climates needed.
	static readonly double rainfall_exponent = 5;
	// world avg. is 990 mm annually, thus the max must maintain that average
	public static readonly short rainfall_max = 2200;
	static readonly byte rainfall_scale = 2;
	// temp
	public static readonly short temperature_min = -2400;
	public static readonly short temperature_max = 2600;
	static readonly byte temperature_scale_geographic = 5;
	static readonly byte temperature_scale_seasonal = 5;
	public static readonly short temperature_anomaly = 4000;
	WorldTile(double x, double y, short elevation, short[] rainfall){
		double raw_heat = Math.Sin(Math.PI * y); // heat, [0, 1]
		temperature = new short[12];
		double seasonal_anomaly_factor = 1 - raw_heat; // how much time affects temperature
		double geographic_anomaly_factor = Math.Pow(1 - raw_heat, 0.3); // how much distance affects temperature
		for (byte i = 0; i < 12; i++)
			temperature[i] = (short)(raw_heat * (temperature_max - temperature_min) + temperature_min
				+ geographic_anomaly_factor * RandomTemperatureAnomaly(x, y, seasonal_anomaly_factor*i/12));
		// Program.Log(String.Format("{0} => [{1}, {2}]", raw_heat, temperature.Min()/100.0, temperature.Max()/100.0));
		// Program.Log(String.Format("\trange = {0}°C", (temperature.Max() - temperature.Min())/100.0));
		this.elevation = elevation;
		this.rainfall = rainfall;
		// debug
		this.x = x;
		this.y = y;
	}
	public double annual_rainfall {
		get { return rainfall.Select(x => (double)x).Sum(); }
	}
	public double average_temperature {
		get { return temperature.Select(x => (double)x).Sum()/12; }
	}
	int biotemperature {
		get {
			double t_mean = temperature.Select(t => Program.Clamp(t/100.0, 0, 30)).Sum()/12;
			return (int)(Math.Log(t_mean/48)/Math.Log(2)); // this represents the sum of a+b on the sides of the triangle
		}
	}
	public string climate {
		get {
			// assume northern hemisphere for this calculation
			double summer_rainfall = rainfall.Take(6).Select(x => (double)x).Sum();
			int bonus = 0.7 <= summer_rainfall/annual_rainfall ? 280
				: 0.3 <= summer_rainfall/annual_rainfall ? 140 : 0;
			double threshold = average_temperature / 100 * 20 + bonus; // in mm
			short coldest_month = temperature.Min();
			if (annual_rainfall < threshold / 2){
				if (0 <= coldest_month)
					return "BWh";
				return "BWk";
			}
			if (annual_rainfall < threshold){
				if (0 <= coldest_month)
					return "BSh";
				return "BSk";
			}
			if (1800 <= coldest_month){
				short driest = rainfall.Min();
				if (60 <= driest)
					return "Af";
				threshold = 100 - annual_rainfall/25;
				if (threshold <= driest)
					return "Am";
				return "Aw";
			}
			short hottest_month = temperature.Max();
			if (hottest_month < 0)
				return "EF";
			if (hottest_month < 1000)
				return "ET";
			string letter_1 = coldest_month < 0 ? "D" : "C";
			char letter_2 = rainfall.Skip(6).Min() * 10 <= rainfall.Take(6).Max() ? 'w'
				: rainfall.Take(6).Min() * 10 <= rainfall.Skip(6).Max() ? 's' : 'f';
			char letter_3 = 4 <= temperature.Where(x => 1000 < x).ToArray().Length ?
				(2200 <= hottest_month ? 'a' : 'b') : coldest_month < -3800 ? 'd' : 'c';
			return letter_1 + letter_2 + letter_3;
		}
	}
	public WorldTile downstream {
		get {
			switch ((int)slope.Item2){
				case 24: // N
					return RelativeTile(0,-1);
				case 25: // S
					return RelativeTile(0, 1);
				case 26: // E
					return RelativeTile(1, 0);
				default: // 27 W
					return RelativeTile(-1, 0);
			}
		}
	}
	public string holdridge {
		get {
			Tuple<int, int> hc = holdridgeCoords;
			int a = hc.Item1;
			int b = hc.Item2;
			if (a == 0  && b == -4)
				return "dry tundra";
			if (a+b < -4 || b < -3 || a < -10)
				return "desert";
			if (a == -1 && b == -3)
				return "moist tundra";
			if (a == -2 && b == -2)
				return "wet tundra";
			if (a+b == -4)
				return "rain tundra";
			if (a < -2)
				return "rainforest";
			if (a == -2)
				return "wet forest";
			if (a == -1)
				return "moist forest";
			if (a+b == -3)
				return "dry scrub";
			if (b == -3)
				return "desert scrub";
			if (a+b == -2)
				return "steppe";
			if (a == 0)
				return "dry forest";
			if (a+b == -1)
				return "thorn steppe";
			if (b == -2)
				return "thorn woodland";
			return "very dry forest";
		}
	}
	public Tuple<int, int> holdridgeCoords {
		get {
			double ap = annual_rainfall;
			double per = potential_evaporation / ap;
			int b = Math.Max(-69, (int)Math.Floor(Math.Log(ap/1000)/Math.Log(2)));
			return new Tuple<int, int>(
				biotemperature - b,
				b);
			// these represent coordinates on the grid
		}
	}
	public bool isLand {
		get { return 0 < this.elevation; }
	}
	double potential_evaporation { // mm/yr
		get {
			if (temperature.Max() <= 0)
				return 0;
			double I = temperature.Select(Tmi => Math.Pow(Math.Max(0, Tmi/500.0), 1.514)).Sum();
			double alpha = 6.75e-7*Math.Pow(I, 3)
				- 7.71e-5*I*I
				+ 1.792e-2*I
				+ 0.49239;
			return temperature.Select((Td, i) => 
				16 * DayLength(i/12.0)/2 * Math.Pow(10 * Math.Max(0, Td/100.0) / I, alpha)
			).Sum();
		}
	}
	public IEnumerable<Resource> potential_resources {
		get { return Resource.resources.Where(r => r.TileTest(this)); }
	}
	public string oceanicZone {
		get {
			if (-200 < elevation)
				return "Epipelagic";
			if (-1000 < elevation)
				return "Mesopelagic";
			if (-4000 < elevation)
				return "Bathypelagic";
			if (-6000 < elevation)
				return "Abyssopelagic";
			return "Hadopelagic";
		}
	}
	public Resource resource {
		get {
			if (resource_cache != null)
				return resource_cache;
			// generate resource
			List<Resource> rs = new List<Resource>(potential_resources);
			if (rs.Count == 0){
				resource_cache = Resource.iron;
				return Resource.iron;
			}
			ResourceRNG.Reset();
			rs.SimplexShuffle(x, y);
			while (0 < rs.Count){
				double s = rs.Select(res => res.worldgen_weight).Sum();
				Resource r = rs[0];
				rs.RemoveAt(0);
				if (Simplex.Noise(x, y, r.id, 0) < r.worldgen_weight/s){
					resource_cache = r;
					return r;
				}
			}
			resource_cache = Resource.iron;
			return Resource.iron;
		}
	}
	public double river_outflow {
		get {
			double a = rainfall.Select((r, i) => 0 < temperature[i] ? r : 0).Sum(); // account for frost
			return isLand ? river_inflow + a - potential_evaporation : 0;
		}
	}
	public Tuple<double, char> slope {
		get {
			// get fine-tuned elevation
			short a = RandomAltitude(x, y);
			// hypothetical tiles 200 km away, smaller than most tiles under 60 latitude or so
			double away = 200;
			// 2/circ because 2 worldsizes = 1 circumference
			short b1 = RandomAltitude(x + away * 2/circumference, y); // E
			short b2 = RandomAltitude(x - away * 2/circumference, y); // W
			short b3 = RandomAltitude(x, y + away * 2/circumference); // S
			short b4 = RandomAltitude(x, y - away * 2/circumference); // N
			List<int> ds = new List<int>{a-b1, a-b2, a-b3, a-b4}.ToList();
			int d = ds.Max();
			// points downhill
			char c = (char)new int[]{26, 27, 25, 24}[ds.IndexOf(d)]; // todo →←↓↑
			return new Tuple<double, char>(d / 1000.0 / away, c);
		}
	}
	Tuple<int, int> tile_index {
		get {
			return new Tuple<int, int>((int)(x*World.size), (int)(y*World.size));
		}
	}
	// static methods
	static double RandomUnadjustedAltitude(double x, double y){
		double lat = y*Math.PI;
		double lon = x*Math.PI;
		Tuple<double, double, double> xyz = Program.LatLong2Spherical(lat, lon);
		x = xyz.Item1*altitude_scale;
		y = xyz.Item2*altitude_scale;
		double z = xyz.Item3*altitude_scale;
		return Math.Pow(Simplex.OctaveNoise(x, y, z, 0, octaves), altitude_exponent);
	}
	static short[] RandomRainfall(double x, double y){
		double lat = y*Math.PI;
		double lon = x*Math.PI;
		// beyond +/- 60 degrees latitude, the level of rainfall rapidly diminishes
		double l4p = y*180;
		double lat_penalty = Math.Pow(l4p < 30 ? l4p/30 : 150 < l4p ? (180-l4p)/30 : 1, 2);
		// use 3d coords for wrapping
		Tuple<double, double, double> xyz = Program.LatLong2Spherical(lat, lon);
		short[] o = new short[12];
		for (byte month = 0; month < 12; month++){
			Tuple<double, double> txty = Program.Time2TimeXY(month/12.0);
			double tx = txty.Item1+1;
			double ty = txty.Item2+1;
			o[month] = (short)(Math.Pow(Simplex.OctaveNoise(
							xyz.Item1*rainfall_scale,
							xyz.Item2*rainfall_scale,
							xyz.Item3*rainfall_scale,
							tx*ty,
						octaves), rainfall_exponent)*rainfall_max*lat_penalty);
		}
		return o;
	}
	static short RandomTemperatureAnomaly(double x, double y, double t){
		double lat = y*Math.PI;
		double lon = x*Math.PI;
		Tuple<double, double, double> xyz = Program.LatLong2Spherical(lat, lon);
		Tuple<double, double> txty = Program.Time2TimeXY(t);
		double tx = txty.Item1+1;
		double ty = txty.Item2+1;
		// output is in range [-anomaly, anomaly]
		return (short)(-temperature_anomaly+2*temperature_anomaly*Simplex.OctaveNoise(
				xyz.Item1*temperature_scale_geographic,
				xyz.Item2*temperature_scale_geographic,
				xyz.Item3*temperature_scale_geographic,
				tx*ty*temperature_scale_seasonal,
				octaves));
	}
	static short RandomAltitude(double x, double y){
		// altitude
		double unadjusted_altitude = RandomUnadjustedAltitude(x, y);
		if (unadjusted_altitude < 0 || 1 < unadjusted_altitude)
			Program.Log(unadjusted_altitude, 2);
		// this SHOULD be Math.Pow(desiredSeaFraction, altitude_exponent) if Perlin were evenly distributed, but distribution is actually appx. normal mean = 0.5, std = 0.146
		double seaCutoff = Program.precomputed_altitude_cutoff;
		return (short)(unadjusted_altitude < seaCutoff ?
			(1-unadjusted_altitude/seaCutoff) * altitude_min :
			(unadjusted_altitude-seaCutoff)/(1-seaCutoff)*altitude_max);
	}
	public static WorldTile Random(double x, double y){
		return new WorldTile(x, y, RandomAltitude(x, y), RandomRainfall(x, y));
	}
	// non-static methods
	double DayLength(double yearFraction){ // yearFraction in [0, 1); day length in days ie [0, 1]
		// http://www.jgiesen.de/astro/suncalc/calculations.htm
		double y = 2*Math.PI*yearFraction;
		double declin = 0.006918
			-0.399912*Math.Cos(y)
			+0.070257*Math.Sin(y)
			-0.006758*Math.Cos(2*y)
			+0.000907*Math.Sin(2*y)
			-0.002697*Math.Cos(3*y)
			+0.00148*Math.Sin(3*y);
		double lat = Program.Remap(this.y, 0, 1, Math.PI/2, -Math.PI/2);
		double q = Math.Cos(90.833 / 180 * Math.PI);
		// ha_inner used to determine eternal night/day
		double ha_inner = q/(Math.Cos(lat)*Math.Cos(declin)) - Math.Tan(lat)*Math.Tan(declin);
		if (ha_inner <= -1 || 1 <= ha_inner){ // eternal night/day
			if (0 < ha_inner) // NORTHERN SUMMER
				return 0 < lat ? 1 : 0;
			return 0 < lat ? 0 : 1;
		}
		double ha = Math.Acos(ha_inner);
		return ha / Math.PI;
	}
	/// <summary>
	///  used for generating veins of resources
	/// </summary>
	/// <param name="offset">use to create unique deposit zones</param>
	/// <param name="p">use to tweak how much is generated</param>
	public bool HasDeposit(double offset, double p){
		Tuple<double, double, double> xyz = Program.LatLong2Spherical(y*Math.PI, x*Math.PI);
		return p < Simplex.OctaveNoise(xyz.Item1 + offset, xyz.Item2 + offset, xyz.Item3 + offset, 0, octaves);
	}
	public void Print(Func<WorldTile, Color> Coloration, Func<WorldTile, int> CharSelector, int x, int y){
		Program.console.SetGlyph(x, y, CharSelector(this), Coloration(this));
	}
	WorldTile RelativeTile(int rel_x, int rel_y){
		Tuple<int, int> t = tile_index;
		return Program.world.GetTileAt(t.Item1 + rel_x, t.Item2 + rel_y);
	}
	// SELECTION TOOLTIP
	public void Tooltip(){
		int left = World.size*2;
		int top = 0;
		Action<string> Print = s => {
			Program.console.Print(left, top, s, Color.Silver);
			top++;
		};
		Print(isLand ? "Land" : "Sea");
		// climate
		Print(isLand ? String.Format("Climate: {0}", climate) : String.Format("Zone: {0}", oceanicZone));
		// biome
		char infinity = (char)236; // ∞
		char deg = (char)248; // °
		if (isLand){
			Tuple<int, int> hc = holdridgeCoords;
			string holdridge_string = String.Format("Biome: {0} ({1}, {2})", holdridge,
				hc.Item1 <= -69 ? "-"+infinity : hc.Item1.ToString(),
				hc.Item2 <= -69 ? "-"+infinity : hc.Item2.ToString());
			Print(holdridge_string);
		}
		// elevation
		Print(String.Format("Elevation: {0} m", elevation));
		// slope
		Tuple<double, char> s = slope;
		Print(String.Format("Slope: {1} {0}%", Math.Round(s.Item1*100, 1), s.Item2));
		// temperature
		Print(String.Format("Temperature: avg. {0:.##}{1}C", average_temperature/100.0, deg));
		Print(String.Format("  in [{0:.##}{2}C, {1:.##}{2}C]", temperature.Min()/100.0, temperature.Max()/100.0, deg));
		// rainfall
		Print(String.Format("Rainfall: tot. {0} mm", annual_rainfall));
		Print(String.Format("  in [{0} mm, {1} mm]", rainfall.Min(), rainfall.Max()));
		Print(String.Format("River Inflow: tot. {0} mm", river_inflow));
		double pe = Math.Round(potential_evaporation);
		Print(String.Format("PE: {0} mm ({1:.###})", pe, pe/annual_rainfall));
		// Resource(s)
		if (isLand)
			Print(String.Format("Resource: {0}", resource == null ? "null" : resource.name));
		// Print(String.Format("PRs: {0}", Resource.PrettyList(potential_resources)));
		// minimap
		DrawMinimap();
	}
	WorldTile[,] Minimap(int width, int height){
		WorldTile[,] o = new WorldTile[height, width];
		for (int i = 0; i < height; i++){
			for (int j = 0; j < width; j++){
				double rx = x + (double)j/World.size / minimap_scale;
				double ry = y + (double)i/World.size / minimap_scale;
				o[i, j] = WorldTile.Random(rx, ry);
			}
		}
		return o;
	}
	void DrawMinimap(){
		int size = Program.tooltip_width - 2;
		int left = World.size*2 + 1;
		int top = World.size - size - 5;
		Action<string> Print = s => {
			Program.console.Print(left, top, s, Color.Silver);
			top++;
		};
		// show minimap info
		octaves = 10;
		Tuple<int, int> t = tile_index;
		Print(String.Format("Minimap ({0}, {1})", t.Item1, t.Item2));
		Print(String.Format("Scale: {0}; {1}x{1}", ScaleString(), 32/minimap_scale));
		Print(String.Format("Chars: {0}", Mapping.char_mode_name));
		Print(String.Format("Color: {0}", Mapping.color_mode_name));
		// show seed
		Program.console.Print(left, World.size-1, Program.seed.ToString(), Color.White);
		// actual map part
		WorldTile[,] w = Minimap(size, size);
		for (int i = 0; i < size; i++)
			for (int j = 0; j < size; j++)
				w[i, j].Print(Mapping.color_mode, Mapping.char_mode, left + j, top + i);
	}
	string ScaleString(){
		double true_scale = circumference / World.size / minimap_scale * 16; // unadjusted
		true_scale *= Math.Cos(Program.Remap(y, 0, 1, Math.PI/2, -Math.PI/2)); // adjusts for latitude
		return String.Format("{0} ({1})", 
			1 <= true_scale ? Math.Round(true_scale) + "km" : Math.Round(true_scale*1000) + "m",
			1.609344 <= true_scale ? Math.Round(true_scale/1.609344) + "mi" : Math.Round(1000/0.3048*true_scale) + "ft"
		);
	}
}
/* todo list
- 5D simplex https://github.com/SquidPony/SquidLib/blob/master/squidlib-util/src/main/java/squidpony/squidmath/SeededNoise.java#L416-L570
	- 5D perlin...? https://github.com/SquidPony/SquidLib/blob/master/squidlib-util/src/main/java/squidpony/squidmath/ClassicNoise.java#L174-L267
	- apparently has more meaningful values in higher dimensions in contrast to simplex
	- https://github.com/SquidPony/SquidLib/blob/master/squidlib-util/src/main/java/squidpony/squidmath/FoamNoise.java#L224-L289
	- see comments in https://discord.com/channels/501465397518925843/509394241819115522/767232917213937685
- change map cursor to this:
	https://sadconsole.com/articles/tutorials/get-started/part-4-movable-characters.html#add-a-movable-glyph
- maybe use
	https://stackoverflow.com/a/4190969/2579798
		for worldgen...?
- PAC tweaks in worldgen can create non-deterministic worlds. This should be fixed.
- no lakes form from local minima, gotta add that
- actually work on the history generator part of the history generator
*/