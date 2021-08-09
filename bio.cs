using System.Collections.Generic;
using System.Linq;

namespace Bio {
	class Lifeform {
		static readonly List<Lifeform> lifeforms = new List<Lifeform>();
		readonly string name;
		readonly int mass; // in grams
		public Lifeform(string s, int m){
			name = s;
			mass = m;
			lifeforms.Append(this);
		}
	}
	class Animal : Lifeform {
		static readonly List<Animal> animals = new List<Animal>();
		readonly BodyPart[] parts;
		readonly bool carnivorous, herbivorous;
		readonly AnimalName detailedName;
		readonly string[] tags;
		Animal(string s, int m, AnimalName n, BodyPart[] p, bool c, bool h, string[] t) : base(s, m){
			parts = p;
			carnivorous = c;
			herbivorous = h;
			detailedName = n;
			tags = t;
			animals.Append(this);
		}
		static readonly Animal cat = new Animal(
			"cat",
			4000,
			new AnimalName(
				"cat",
				"cats",
				"kitten",
				"kittens"
			),
			BodyPart.generic_mammal,
			true, true,
			new string[]{"intelligent"}
		);
	}
	class AnimalName {
		readonly string generic, generic_pl, male_, male_pl_, female_, female_pl_,
			young_, young_pl_, male_young_, male_young_pl_, female_young_, female_young_pl_;
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
		static readonly BodyPart fur = new BodyPart("fur");
		static readonly BodyPart head = new BodyPart("head");
		// useful templates
		public static readonly BodyPart[] quadruped = new BodyPart[]{
			head
		};
		public static readonly BodyPart[] generic_mammal = new BodyPart[]{}.Append(
			fur
		).ToArray();
	}
	class Plant : Lifeform{
		static readonly List<Plant> plants = new List<Plant>();
		readonly bool deciduous;
		// 0 = moss/lichen/grass; 1 = small plant-like (think weed); 2 = bushy; 3 = tree
		readonly byte type;
		readonly int maturity_time; // in days???
		Plant(string s, int m, bool d, byte t, int mt) : base(s, m){
			deciduous = d;
			type = t;
			maturity_time = mt;
			plants.Append(this);
		}
		static readonly Plant pine = new Plant("pine", 2500000, false, 3, 25*365);
	}
}