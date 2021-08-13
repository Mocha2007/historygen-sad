using System.Drawing;
namespace Export {
	static class Celestia {
		// export a celestia-readable mod, including maps!
		static readonly byte resolution = 8; // 2^11 by 2^10 px
		static readonly int width = 2 << resolution;
		static readonly int height = 2 << (resolution - 1);
		public static void ExportBitmap(){
			Bitmap b = new Bitmap(width, height);
			int percentnotification = 1;
			Program.Log("Exporting...");
			for (int x = 0; x < width; x++){
				for (int y = 0; y < height; y++){
					WorldTile w = WorldTile.Random(2.0*x/width, (double)y/height);
					Microsoft.Xna.Framework.Color c = Mappings.Mapping.ColorSatellite(w);
					b.SetPixel(x, y, Color.FromArgb(c.R, c.G, c.B));
				}
				if (percentnotification/100.0 <= (double)x/width){
					Program.LogProgress(percentnotification, 100);
					percentnotification++;
				}
			}
			b.Save("export/satellite.png");
			Program.Log("Exported satellite view to export/satellite.png");
		}
	}
}