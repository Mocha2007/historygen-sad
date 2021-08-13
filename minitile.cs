using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
// color
using Microsoft.Xna.Framework;
namespace Mini {
	static class Core {
		public static readonly char blockChar = (char)0xdb;
		public static void ParseData(){
			IEnumerable<string> raw = File.ReadAllLines("data/material.dat")
				.Select(line =>
					Regex.Replace(line.ToLower(), @"^\s+|\s+$", "") // set lowercase; remove leading/trailing whitespace
				);
			string type = "";
			// MAKE SURE TO COPY THESE TO RESET
			string name = "";
			Color color = Color.Black;
			ushort density = 1000;
			Action Reset = () => {
				name = "";
				color = Color.Black;
				density = 1000;
			};
			int materials = 0;
			foreach (string line in raw){
				string[] split = line.Split(" ");
				string kw = split[0];
				switch (kw){
					case "material":
						type = kw;
						continue;
					case "name":
						name = split[1];
						continue;
					case "color":
						color = Program.ColorFromHex(split[1]);
						continue;
					case "density":
						density = ushort.Parse(split[1]);
						continue;
					case "end":
						break; // handled below
					default: // just a comment!
						continue;
				}
				// handle end
				if (type == "material"){
					new Material(name, color, density);
					materials++;
				}
				else
					throw new NotImplementedException();
				// reset
				Reset();
			}
			Program.Log(String.Format("{0} materials loaded", materials));
		}
	}
	
	class Map {
		/*
			This will be where the game is played out.
			Todo: elevation, rivers, flora, resource placement.
		*/
		static readonly int mapsize = 100; // tiles; each is 1m^2
		readonly Tile[,] tiles = new Tile[mapsize, mapsize];
		readonly WorldTile worldtile; // used to determine climate
		public Map(WorldTile w){
			worldtile = w;
			Generate();
			// todo: change view to Mini.Map!
		}
		void Generate(){
			// generate "hilly" parts...
			// generate APPROPRIATE flora
			// for now just generate a dirt floor and nothing else...
			Material dirt = Material.FromString("dirt");
			int percent = 1;
			for (int x = 0; x < mapsize; x++){
				for (int y = 0; y < mapsize; y++)
					tiles[x, y] = new Tile(null, dirt.floor);
				if (percent*100.0 <= (double)x/mapsize){
					Program.LogProgress(percent, 100);
					percent++;
				}
			}
			Program.Log("Generated MiniMap");
		}
	}
	class Tile {
		/*
			This is a 1m^2 "block"
			floor, wall, furniture
		*/
		Block block;
		Floor floor;
		Tuple<char, Color, Color> charForeBack {
			get { return block == null ? floor.charForeBack : block.charForeBack; }
		}
		public Tile(Block b, Floor f){
			this.block = b;
			this.floor = f;
		}
	}
	class Material {
		// each material has a block and floor form. each block/floor has a material.
		static readonly List<Material> materials = new List<Material>();
		readonly Block block;
		public readonly Floor floor;
		readonly string name;
		public readonly Color color;
		readonly ushort density; // kg/m^3
		// readonly short melt, boil; // "degrees centicelsius", ie. C*100
		// todo: standardize temperature (make a class for it?) across ALL files
		public Material (string name, Color color, ushort density){
			this.name = name;
			this.color = color;
			this.density = density;
			// create block and floor
			block = new Block(this);
			floor = new Floor(this);
			// push to list
			materials.Add(this);
		}
		public static Material FromString(string s){
			return materials.Find(m => m.name == s);
		}
	}
	abstract class MadeFromMaterial {
		readonly Material material;
		public MadeFromMaterial(Material m){
			this.material = m;
		}
		public Tuple<char, Color, Color> charForeBack {
			get { return new Tuple<char, Color, Color>(Core.blockChar, material.color, material.color); }
		}
	}
	class Block : MadeFromMaterial {
		public Block(Material m) : base(m){}
	}
	class Floor : MadeFromMaterial {
		public Floor(Material m) : base(m){}
	}
	class Building : Block {
		// incl. furniture
		readonly byte size;
		Building(Material m, byte size) : base(m){
			this.size = size;
		}
	}
}