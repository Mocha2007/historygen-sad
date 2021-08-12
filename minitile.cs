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
		}
	}
	class Tile {
		/*
			This is a 1m^2 "block"
			floor, wall, furniture
		*/
	}
}