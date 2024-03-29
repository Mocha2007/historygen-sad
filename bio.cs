using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
// color
using Microsoft.Xna.Framework;

namespace Bio {
	class Lifeform {
		public static readonly List<Lifeform> lifeforms = new List<Lifeform>();
		readonly AnimalName name;
		public readonly int mass; // in grams
		readonly int maturity_time; // in days???
		readonly string[] tags;
		public Lifeform(AnimalName s, int mass, int mat, string[] t){
			name = s;
			this.mass = mass;
			maturity_time = mat;
			tags = t;
			lifeforms.Append(this);
		}
		public static void ParseData(){
			IEnumerable<string> raw = File.ReadAllLines("data/bio_part.dat")
				.Concat(File.ReadAllLines("data/bio_plan.dat"))
				.Concat(File.ReadAllLines("data/bio.dat"))
				.Select(line =>
					Regex.Replace(line.ToLower(), @"^\s+|\s+$", "") // set lowercase; remove leading/trailing whitespace
				);
			string type = "";
			// MAKE SURE TO COPY THESE TO RESET
			AnimalName name = AnimalName.none;
			int mass = 0;
			int maturity_time = 0;
			ushort relsize = 0;
			BodyPart parent = BodyPart.root;
			BodyPart[] parts = new BodyPart[0];
			BodyPlan body_plan = BodyPlan.none;
			string[] tags = new string[0];
			char c = '\0';
			Color color = Color.Gray;
			double dimorphism = 1;
			Action Reset = () => {
				name = AnimalName.none;
				mass = 0;
				maturity_time = 0;
				relsize = 0;
				parent = BodyPart.root;
				parts = new BodyPart[0];
				body_plan = BodyPlan.none;
				tags = new string[0];
				c = '\0';
				color = Color.Gray;
				dimorphism = 1;
			};
			int partcount = 0;
			int plans = 0;
			int animals = 0;
			int plants = 0;
			foreach (string line in raw){
				string[] split = line.Split(" ");
				string kw = split[0];
				switch (kw){
					case "part":
					case "plan":
					case "animal":
					case "plant":
						type = kw;
						continue;
					case "name":
						name = new AnimalName(split[1]);
						continue;
					case "tags":
						tags = split.Skip(1).ToArray();
						continue;
					case "parent":
						parent = BodyPart.FromName(split[1]);
						continue;
					case "parts":
						parts = split.Skip(1).Select(s => BodyPart.FromName(s)).ToArray();
						continue;
					case "icon":
						color = Program.ColorFromHex(split[1]);
						c = split[2][0];
						continue;
					case "mass":
						mass = int.Parse(split[1]);
						continue;
					case "maturity_time":
						maturity_time = int.Parse(split[1]);
						continue;
					case "relsize":
						relsize = ushort.Parse(split[1]);
						continue;
					case "dimorphism":
						dimorphism = double.Parse(split[1]);
						continue;
					case "template":
					case "bodyplan":
						body_plan = BodyPlan.FromName(split[1]);
						continue;
					case "end":
						break; // handled below
					default: // just a comment!
						continue;
				}
				// handle end
				if (type == "part"){
					new BodyPart(name, parent, tags, relsize);
					partcount++;
				}
				else if (type == "plan"){
					new BodyPlan(name, body_plan, parts, tags);
					plans++;
				}
				else if (type == "animal"){
					new Animal(name, mass, maturity_time, tags, body_plan, c, color, dimorphism);
					animals++;
				}
				else if (type == "plant"){
					new Plant(name, mass, maturity_time, tags);
					plants++;
				}
				else
					throw new NotImplementedException();
				// reset
				Reset();
			}
			Program.Log(String.Format("{0} bodyparts loaded", partcount), 0);
			Program.Log(String.Format("{0} bodyplans loaded", plans), 0);
			Program.Log(String.Format("{0} plants loaded", plants), 0);
			Program.Log(String.Format("{0} animals loaded", animals), 0);
		}
	}
	class Animal : Lifeform {
		static readonly List<Animal> animals = new List<Animal>();
		static readonly double standardDeviationAsAFactorOfMass = 0.16; // sigma = 0.16*mean
		readonly BodyPlan bodyplan;
		char c;
		Color color;
		double dimorphismConstant;
		public Animal(AnimalName n, int mas, int mat, string[] t, BodyPlan p, char c, Color col, double d) : base(n, mas, mat, t){
			bodyplan = p;
			this.c = c;
			color = col;
			dimorphismConstant = d;
			animals.Append(this);
		}
		int RandomMass(){
			// normally distributed.
			return (int)MochaRandom.Normal(mass, standardDeviationAsAFactorOfMass*mass);
		}
	}
	class AnimalName {
		public readonly string generic, generic_pl;
		readonly string male_, male_pl_, female_, female_pl_,
			young_, young_pl_, male_young_, male_young_pl_, female_young_, female_young_pl_;
		public static readonly AnimalName none = new AnimalName("unnamed");
		// HAS ALL NAMES
		public AnimalName(string g, string gp, string m, string mp, string f, string fp,
				string y, string yp, string my, string myp, string fy, string fyp){
			// adult
			generic = g;
			generic_pl = gp;
			male_ = m;
			male_pl_ = mp;
			female_ = f;
			female_pl_ = fp;
			// young
			young_ = y;
			young_pl_ = yp;
			male_young_ = my;
			male_young_pl_ = myp;
			female_young_ = fy;
			female_young_pl_ = fyp;
		}
		// ALL NAMES BUT GENDERED YOUNG
		public AnimalName(string g, string gp, string m, string mp, string f, string fp, string y, string yp){
			// adult
			generic = g;
			generic_pl = gp;
			male_ = m;
			male_pl_ = mp;
			female_ = f;
			female_pl_ = fp;
			// young
			young_ = male_young_ = female_young_ = y;
			young_pl_ = male_young_pl_ = female_young_pl_ = yp;
		}
		// ALL NAMES BUT YOUNG
		public AnimalName(string g, string gp, string m, string mp, string f, string fp){
			// adult
			generic = young_ = g;
			generic_pl = young_pl_ = gp;
			male_ = male_young_ = m;
			male_pl_ = male_young_pl_ = mp;
			female_ = female_young_ = f;
			female_pl_ = female_young_pl_ = fp;
		}
		// ADULT/YOUNG DISTINCTION ONLY
		public AnimalName(string g, string gp, string y, string yp){
			// adult
			generic = male_ = female_ = g;
			generic_pl = male_pl_ = female_pl_ = gp;
			// young
			young_ = male_young_ = female_young_ = y;
			young_pl_ = male_young_pl_ = female_young_pl_ = yp;
		}
		// SINGULAR/PLURAL ONLY
		public AnimalName(string g, string gp){
			generic = male_ = female_ = young_ = male_young_ = female_young_ = g;
			generic_pl = male_pl_ = female_pl_ = young_pl_ = male_young_pl_ = female_young_pl_ = gp;
		}
		// NO DISTINCTIONS OF ANY KIND
		public AnimalName(string s){
			generic = male_ = female_ = young_ = male_young_ = female_young_ =
			generic_pl = male_pl_ = female_pl_ = young_pl_ = male_young_pl_ = female_young_pl_ = s;
		}
		
	}
	class BodyPart {
		public static readonly List<BodyPart> bodyParts = new List<BodyPart>();
		readonly AnimalName name;
		readonly BodyPart parent;
		readonly string[] tags;
		readonly ushort relsize; // this is primarily based on average human mass in grams
		/* Sources:
			https://exrx.net/Kinesiology/Segments
			https://www.hindawi.com/journals/ari/2018/4687538/
		*/
		public BodyPart(AnimalName n, BodyPart p, string[] t, ushort r){
			name = n;
			parent = p;
			tags = t;
			relsize = r;
			bodyParts.Add(this);
		}
		BodyPart(string n, ushort r){
			name = new AnimalName(n);
			parent = root;
			tags = new string[0];
			relsize = r;
			bodyParts.Add(this);
		}
		double massFraction {
			get { return (double)relsize/root.relsize; }
		}
		public static BodyPart FromName(string s){
			return bodyParts.Find(bp => bp.name.generic == s);
		}
		public static readonly BodyPart root = new BodyPart("root", 62000);
	}
	class BodyPlan {
		public static readonly List<BodyPlan> bodyPlans = new List<BodyPlan>();
		AnimalName name;
		BodyPart[] parts;
		string[] tags;
		BodyPlan(string n){
			name = new AnimalName(n);
			tags = new string[0];
			parts = new BodyPart[0];
			bodyPlans.Append(this);
		}
		public BodyPlan(AnimalName n, BodyPlan b, BodyPart[] p, string[] t){
			name = n;
			if (b == null){
				parts = p;
				tags = t;
			}
			else {
				parts = b.parts.Concat(p).ToArray();
				tags = t.Concat(b.tags).ToArray();
			}
			bodyPlans.Append(this);
		}
		public static BodyPlan FromName(string s){
			return bodyPlans.Find(bp => bp.name.generic == s);
		}
		// useful templates
		public static readonly BodyPlan none = new BodyPlan("none");
	}
	class Plant : Lifeform{
		static readonly List<Plant> plants = new List<Plant>();
		public Plant(AnimalName n, int m, int mt, string[] t) : base(n, m, mt, t){
			plants.Append(this);
		}
	}
}