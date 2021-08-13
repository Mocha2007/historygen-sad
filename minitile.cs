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
			Action Reset = () => {
				name = "";
				color = Color.Black;
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
						color = new Color(uint.Parse(split[1]));
						continue;
					case "end":
						break; // handled below
					default: // just a comment!
						continue;
				}
				// handle end
				if (type == "material"){
					new Material(name, color);
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
	}
	class Material {
		// each material has a block and floor form. each block/floor has a material.
		static readonly List<Material> materials = new List<Material>();
		readonly Block block;
		readonly Floor floor;
		readonly string name;
		public readonly Color color;
		public Material (string name, Color color){
			this.name = name;
			this.color = color;
			// create block and floor
			block = new Block(this);
			floor = new Floor(this);
			// push to list
			materials.Add(this);
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