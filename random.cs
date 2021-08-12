// various rng tools
static class MochaRandom {
	public static bool Bool(){
		return Program.rng.Next(0, 2) == 0;
	}
	public static byte Byte(){
		return (byte)Program.rng.Next();
	}
	public static double Normal(double mean, double standard_deviation){
		// we convert uniform random to a gaussion distribution via the quantile function.
		return Program.NormalQuantile(Program.rng.NextDouble())*standard_deviation + mean;
	}
}