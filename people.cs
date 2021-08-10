using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Resources;
using Noise;

namespace Person {
	class Person {
		// todo Culture
		readonly bool gender; // 0=F; 1=M
		readonly Personality personality;
		readonly List<PersonalRelation> relations = new List<PersonalRelation>();
		readonly Sexuality sexuality;
		readonly PersonalSkill[] skills = PersonalSkill.BlankSlate();
		// todo: memories???
		readonly string name;
		Person(string n, bool g, Personality p, Sexuality s){
			name = n;
			gender = g;
			personality = p;
			sexuality = s;
		}
		public static Person Random(){
			return new Person(People.NamingSystem.systems[0].Random(),
				MochaRandom.Bool(), Personality.Random(), Sexuality.Random());
		}
	}
	// subtypes
	class Personality {
		// prefs: 128 = neutral; 255 = love; 0 = hate
		// axes: 128 = neutral; 255 = high; 0 = low
		readonly byte[] lifeformPrefs, resourcePrefs, personalityAxisValues;
		Personality(byte[] pav, byte[] lp, byte[] rp){
			personalityAxisValues = pav;
			lifeformPrefs = lp;
			resourcePrefs = rp;
		}
		public static Personality Random(){
			return new Personality(
				Bio.Lifeform.lifeforms.Select(r => MochaRandom.Byte()).ToArray(),
				Resource.resources.Select(r => MochaRandom.Byte()).ToArray(),
				PersonalityAxis.axes.Select(a => MochaRandom.Byte()).ToArray()
			);
		}
	}
	class PersonalityAxis {
		public static readonly List<PersonalityAxis> axes = new List<PersonalityAxis>();
		readonly string name, antonym;
		PersonalityAxis(string n, string ant){
			name = n;
			antonym = ant;
			axes.Add(this);
		}
		// https://en.wikipedia.org/wiki/Big_Five_personality_traits ???
		// https://en.wikipedia.org/wiki/HEXACO_model_of_personality_structure ???
		// https://en.wikipedia.org/wiki/Facet_%28psychology%29 ???
		static readonly PersonalityAxis extraversion = new PersonalityAxis("extraversion", "introversion");
		static readonly PersonalityAxis agreeableness = new PersonalityAxis("agreeableness", "disagreeableness");
		static readonly PersonalityAxis neuroticism = new PersonalityAxis("neuroticism", "stability");
		static readonly PersonalityAxis conscientousness = new PersonalityAxis("conscientousness", "spontaneity"); // imperfect antonym but w/e
		static readonly PersonalityAxis openness = new PersonalityAxis("openness", "closedness");
		// extraversion
		// agreeableness
		// neuroticism
		// conscientousness
		// openness (to experiences)
	}
	class Relation {
		// friends, family, etc...
		readonly bool unbreakable; // eg. parent-child relations cannot be broken, but friend-friend ones can be
	}
	class PersonalRelation {
		readonly Relation type;
		readonly Person other_agent;
	}
	class Sexuality {
		readonly bool rom_f, rom_m, sex_f, sex_m, can_poly;
		Sexuality(bool rf, bool rm, bool sf, bool sm, bool cp){
			rom_f = rf;
			rom_m = rm;
			sex_f = sf;
			sex_m = sm;
			can_poly = cp;
		}
		string Name(bool gender){
			string[] s = new string[2];
			// sexuality
			if (sex_m && sex_f)
				s[0] = "bisexual";
			else if ((sex_m && gender) || (sex_f && !gender))
				s[0] = "homosexual";
			else if ((sex_m && !gender) || (sex_f && gender))
				s[0] = "heterosexual";
			else
				s[0] = "asexual";
			// romance
			if (rom_m && rom_f)
				s[0] = "biromanitc";
			else if ((rom_m && gender) || (rom_f && !gender))
				s[0] = "homoromantic";
			else if ((rom_m && !gender) || (rom_f && gender))
				s[0] = "heteroromantic";
			else
				s[0] = "aromantic";
			return string.Join(' ', s);
		}
		public static Sexuality Random(){
			return new Sexuality(
				MochaRandom.Bool(),
				MochaRandom.Bool(),
				MochaRandom.Bool(),
				MochaRandom.Bool(),
				MochaRandom.Bool()
			);
		}
	}
	class Skill {
		public static readonly List<Skill> skills = new List<Skill>();
		readonly string name;
		Skill(string s){
			name = s;
			skills.Add(this);
		}
		public static readonly Skill woodcutter = new Skill("Woodcutter");
	}
	class PersonalSkill {
		// levels range [0, 20], like in Rimworld, Dwarf Fortress, etc.v
		// to get to the next level, you need another 128(this_level)^2 + 128 xp
		Skill skill;
		byte level;
		ushort progress;
		PersonalSkill(Skill s){
			skill = s;
		}
		int nextLevelXPThreshold {
			get {
				return (level*level << 7) + 128;
			}
		}
		int xpToNextLevel {
			get {
				return nextLevelXPThreshold - progress;
			}
		}
		double SkillCheckSucceedChance(byte challengeRating) {
			return 1 - 1/(level*level + 2 - challengeRating*challengeRating);
		}
		static readonly string[] levelnamescoarse = new string[]{
			"professional",
			"expert",
			"master",
			"legendary",
		};
		static readonly string[] levelnamesfine = new string[]{
			"dabbling",
			"beginner",
			"novice",
			"competent",
			"skilled",
		};
		string skillLevelName {
			get {
				if (level < 5)
					return levelnamesfine[level];
				return String.Format("{0} +{1}", levelnamescoarse[level/5-1], levelnamescoarse[level%5]);
			}
		}
		// static
		public static PersonalSkill[] BlankSlate(){
			return Skill.skills.Select(s => new PersonalSkill(s)).ToArray();
		}
	}
}

namespace People {
	static class Core {
		public static void ParseData(){
			IEnumerable<string> raw = File.ReadAllLines("data/language.dat")
				.Select(line =>
					Regex.Replace(line.ToLower(), @"^\s+|\s+$", "") // set lowercase; remove leading/trailing whitespace
				);
			string type = "";
			// MAKE SURE TO COPY THESE TO RESET
			string name = "";
			string[] given = new string[0];
			string[] family = new string[0];
			Action Reset = () => {
				name = "";
				given = new string[0];
				family = new string[0];
			};
			int namingcount = 0;
			foreach (string line in raw){
				string[] split = line.Split(" ");
				string kw = split[0];
				switch (kw){
					case "naming":
						type = kw;
						continue;
					case "name":
						name = split[1];
						continue;
					case "given":
						given = split.Skip(1).ToArray();
						continue;
					case "family":
						given = split.Skip(1).ToArray();
						continue;
					case "end":
						break; // handled below
					default: // just a comment!
						continue;
				}
				// handle end
				if (type == "naming"){
					new NamingSystem(name, given, family);
					namingcount++;
				}
				else
					throw new NotImplementedException();
				// reset
				Reset();
			}
			Program.Log(String.Format("{0} naming systems loaded", namingcount), 0);
		}
	}
	class Construct {
		string name;
		public Construct(string name){
			this.name = name;
		}
	}
	class Country : Construct {
		public static readonly byte maxCountries = MochaRandom.Byte();
		readonly Culture primaryCulture;
		Country(string name, Culture primary) : base(name){
			primaryCulture = primary;
		}
		// Simplex.Noise(x, y, r.id, 0)
		// Program.LatLongToSpherical(lat, long) => x,y,z
		public static byte CountryAtTile(double lat, double lon){
			List<int> k = Simplex.p.ToList();
			k.SimplexShuffle(lat, lon);
			Tuple<double, double, double> xyz = Program.LatLong2Spherical(lat, lon);
			// v starts in [-1, 1]
			double v = Simplex.Noise(xyz.Item1, xyz.Item2, xyz.Item3, 0);
			v++; // now in [0, 2]
			v /= 2; // now in [0, 1]
			byte temp_id = (byte)(v*maxCountries);
			return (byte)k[temp_id];
		}
	}
	class Culture : Construct {
		readonly Language language;
		readonly Religion religion;
		Culture(string name, Language l, Religion r) : base(name){
			language = l;
			religion = r;
		}
	}
	class Language : Construct {
		Language(string name) : base(name){}	
	}
	class NamingSystem : Construct {
		public static readonly List<NamingSystem> systems = new List<NamingSystem>();
		/* todo list
			-patrynomic/matrynomic
			-marriage name/maiden name
			-family name/given name order
		*/
		readonly string[] familyNameBank, givenNameBank;
		public NamingSystem(string name, string[] given, string[] family) : base(name){
			givenNameBank = given;
			familyNameBank = family;
			systems.Add(this);
		}
		public string Random(){
			int fi = Program.rng.Next(0, familyNameBank.Length);
			int gi = Program.rng.Next(0, givenNameBank.Length);
			return String.Format("{0} {1}", familyNameBank[fi], givenNameBank[gi]);
		}
	}
	class Religion : Construct {
		Religion(string name) : base(name){}
	}
}