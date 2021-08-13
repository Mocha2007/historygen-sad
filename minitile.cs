// color
using Microsoft.Xna.Framework;
namespace Mini {
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
	}
	class Block {
		readonly Color fore, back;
		readonly char c;
	}
	class Floor {
		readonly Color fore, back;
		readonly char c;
	}
	class Building : Block {
		// incl. furniture
		readonly byte size;
	}
}