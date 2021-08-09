// various rng tools
static class MochaRandom {
	static readonly System.Random r = new System.Random();
	public static bool Bool(){
		return r.Next(0, 2) == 0;
	}
	public static byte Byte(){
		return (byte)r.Next();
	}
}