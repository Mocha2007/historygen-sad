using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Bio {
	class Lifeform {
		public static readonly List<Lifeform> lifeforms = new List<Lifeform>();
		readonly AnimalName name;
		readonly int mass; // in grams
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
			string[] raw = File.ReadAllLines("data/bio.dat").Select(line =>
				Regex.Replace(line.ToLower(), @"^\s+|\s+$", "") // set lowercase; remove leading/trailing whitespace
			).ToArray();
			string type = "";
			AnimalName name = AnimalName.none;
			int mass = 0;
			int maturity_time = 0;
			BodyPlan body_plan = BodyPlan.none;
			string[] tags = new string[0];
			int animals = 0;
			int plants = 0;
			foreach (string line in raw){
				string[] split = line.Split(" ");
				string kw = split[0];
				switch (kw){
					case "animal":
					case "plant":
						type = kw;
						continue;
					case "name":
						name = new AnimalName(split[1]); // todo
						continue;
					case "mass":
						mass = int.Parse(split[1]);
						continue;
					case "maturity_time":
						maturity_time = int.Parse(split[1]);
						continue;
					case "bodyplan":
						body_plan = BodyPlan.FromName(split[1]);
						continue;
					case "tags":
						tags = split.Skip(1).ToArray();
						continue;
					case "end":
						break; // handled below
					default:
						throw new ArgumentOutOfRangeException("invalid datafile keyword");
				}
				// handle end
				if (type == "animal"){
					new Animal(name, mass, maturity_time, tags, body_plan);
					animals++;
				}
				else if (type == "plant"){
					new Plant(name, mass, maturity_time, tags);
					plants++;
				}
				else
					throw new NotImplementedException();
				// reset
				name = AnimalName.none;
				mass = 0;
				maturity_time = 0;
				body_plan = BodyPlan.none;
				tags = new string[0];
			}
			Program.Log(String.Format("{0} plants loaded", plants), 0);
			Program.Log(String.Format("{0} animals loaded", animals), 0);
		}
	}
	class Animal : Lifeform {
		static readonly List<Animal> animals = new List<Animal>();
		readonly BodyPlan bodyplan;
		public Animal(AnimalName n, int mas, int mat, string[] t, BodyPlan p) : base(n, mas, mat, t){
			bodyplan = p;
			animals.Append(this);
		}
		/*
		static readonly Animal cat = new Animal(
			new AnimalName(
				"cat",
				"cats",
				"kitten",
				"kittens"
			),
			4000,
			365,
			new string[]{"intelligent", "omnivore"},
			BodyPlan.generic_mammal
		);
		*/
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
		readonly string name;
		readonly BodyPart parent;
		readonly string[] tags;
		BodyPart(string n, BodyPart p, string[] t){
			name = n;
			parent = p;
			tags = t;
		}
		BodyPart(string n, BodyPart p){
			name = n;
			parent = p;
			tags = new string[0];
		}
		BodyPart(string n){
			name = n;
			parent = root;
			tags = new string[0];
		}
		static readonly BodyPart root = new BodyPart("root");
		public static readonly BodyPart fur = new BodyPart("fur");
		public static readonly BodyPart head = new BodyPart("head");
	}
	class BodyPlan {
		public static readonly List<BodyPlan> bodyPlans = new List<BodyPlan>();
		string name;
		BodyPart[] parts;
		BodyPlan(string n, BodyPart[] p){
			name = n;
			parts = p;
			bodyPlans.Append(this);
		}
		BodyPlan(string n, BodyPlan b, BodyPart[] p){
			name = n;
			parts = b.parts.Concat(p).ToArray();
			bodyPlans.Append(this);
		}
		public static BodyPlan FromName(string s){
			return bodyPlans.Find(bp => bp.name == s);
		}
		// useful templates
		public static readonly BodyPlan none = new BodyPlan("none", new BodyPart[0]);
		public static readonly BodyPlan quadruped = new BodyPlan("quadruped", new BodyPart[]{
			BodyPart.head
		});
		public static readonly BodyPlan generic_mammal = new BodyPlan("generic_mammal", quadruped, new BodyPart[]{
			BodyPart.fur
		});
	}
	class Plant : Lifeform{
		static readonly List<Plant> plants = new List<Plant>();
		// 0 = moss/lichen/grass; 1 = small plant-like (think weed); 2 = bushy; 3 = tree
		public Plant(AnimalName n, int m, int mt, string[] t) : base(n, m, mt, t){
			plants.Append(this);
		}
		// static readonly Plant pine = new Plant(new AnimalName("pine", "pines"), 2500000, 25*365, new string[]{"tree"});
	}
}