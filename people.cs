using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Resources;

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
			bool gender = MochaRandom.Bool();
			return new Person(People.NamingSystem.systems[0].RandomFromGender(gender),
				gender, Personality.Random(), Sexuality.Random());
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
		Relation(bool ub){
			unbreakable = ub;
		}
	}
	class PersonalRelation {
		readonly Relation type;
		readonly Person other_agent;
		PersonalRelation(Relation r, Person other){
			type = r;
			other_agent = other;
		}
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
			// turn raw bytes into string to trick c# into not decoding it
			string unbroken = new String(File.ReadAllBytes("data/language.dat").Select(b => (char)b).ToArray());
			IEnumerable<string> raw = unbroken.Split('\n') // should be saved as \r\n but won't matter because we remove whitespace
				.Select(line =>
					Regex.Replace(line.ToLower(), @"^\s+|\s+$", "") // set lowercase; remove leading/trailing whitespace
				);
			string type = "";
			// MAKE SURE TO COPY THESE TO RESET
			string name = "";
			string[] male = new string[0];
			string[] female = new string[0];
			string[] neuter = new string[0];
			string[] family = new string[0];
			bool usesTwoGivenNames = false;
			bool givenNamesFirst = true;
			bool nofemalesurname = false;
			string patronymic = "";
			Action Reset = () => {
				name = "";
				male = new string[0];
				female = new string[0];
				neuter = new string[0];
				family = new string[0];
				usesTwoGivenNames = false;
				givenNamesFirst = true;
				nofemalesurname = false;
				patronymic = "";
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
					case "male":
						male = split.Skip(1).ToArray();
						continue;
					case "female":
						female = split.Skip(1).ToArray();
						continue;
					case "neuter":
						neuter = split.Skip(1).ToArray();
						continue;
					case "family":
						family = split.Skip(1).ToArray();
						continue;
					case "twogivennames":
						usesTwoGivenNames = bool.Parse(split[1]);
						continue;
					case "givennamesfirst":
						givenNamesFirst = bool.Parse(split[1]);
						continue;
					case "nofemalesurname":
						nofemalesurname = bool.Parse(split[1]);
						continue;
					case "patronymic":
						patronymic = String.Join(' ', split.Skip(1));
						continue;
					case "end":
						break; // handled below
					default: // just a comment!
						continue;
				}
				// handle end
				if (type == "naming"){
					new NamingSystem(name, male, female, neuter, family,
						usesTwoGivenNames, givenNamesFirst, nofemalesurname, patronymic);
					namingcount++;
				}
				else
					throw new NotImplementedException();
				// reset
				Reset();
			}
			Program.Log(String.Format("{0} naming systems loaded", namingcount));
		}
	}
	class Construct {
		string name;
		public Construct(string name){
			this.name = name;
		}
	}
	class Country : Construct {
		public static readonly int maxCountries = 200;
		static readonly WorldTile[] capitals = new WorldTile[maxCountries];
		public static void Initialize(){
			// choose 200 random tiles and put countries there...
			double meany = 0.5*World.size;
			double stdy = 0.2*World.size; // experimentation has shown this to look nicest
			for (int i = 0; i < maxCountries; i++){
				WorldTile attempt;
				while (0 <= (attempt = Program.world.tiles // reassign every test
					[Program.Mod((int)MochaRandom.Normal(meany, stdy), World.size), Program.rng.Next(0, 2*World.size)] // longitude random, latitude normally distributed
					).owner){} // redo if there's already an owner
				capitals[i] = attempt;
			}
			Program.Log(String.Format("{0} countries placed", maxCountries));
		}
		readonly Culture primaryCulture;
		Country(string name, Culture primary) : base(name){
			primaryCulture = primary;
		}
		// Simplex.Noise(x, y, r.id, 0)
		// Program.LatLongToSpherical(lat, long) => x,y,z
		/*
			NEW PLAN
			generate 200 random points on sphere
			see which is closest using simple 3d distance calculation
			create voronoi diagram
		*/
		public static int CountryAtTile(WorldTile w){
			if (!w.isLand)
				return -1;
			if (0 <= w.owner)
				return w.owner;
			if (capitals[maxCountries-1] == null) // in case cursor gets placed on land BEFORE countries generate
				return -1;
			// find min dist
			List<double> distances = capitals.Select(c => w.Distance(c)).ToList();
			return distances.IndexOf(distances.Min());
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
			-patronymic/matrynomic
			-marriage name/maiden name
			-family name/given name order
		*/
		readonly string[] familyNameBank, maleGivenNameBank, femaleGivenNameBank, neuterGivenNameBank;
		readonly bool usesTwoGivenNames, givenNamesFirst, nofemalesurname;
		readonly string patronymic; // suffix; blank = no patronymics
		// eg. Mac{0}, or {0}son
		bool usesPatronymic {
			get { return 0 < patronymic.Length; }
		}
		public NamingSystem(string name, string[] given_m, string[] given_f, string[] given_n, string[] family,
				bool usesTwoGivenNames, bool givenNamesFirst, bool nofemalesurname, string patronymic) : base(name){
			maleGivenNameBank = given_m;
			femaleGivenNameBank = given_f;
			neuterGivenNameBank = given_n;
			familyNameBank = family;
			this.usesTwoGivenNames = usesTwoGivenNames;
			this.givenNamesFirst = givenNamesFirst;
			this.nofemalesurname = nofemalesurname;
			this.patronymic = patronymic;
			systems.Add(this);
		}
		public string RandomFromGender(bool is_male){
			string family;
			if (usesPatronymic)
				family = String.Format(patronymic, RandomGivenFromGender(true));
			else {
				int fi = Program.rng.Next(0, familyNameBank.Length);
				family = familyNameBank[fi];
			}
			string givens = RandomGivenFromGender(is_male);
			if (usesTwoGivenNames)
				givens += " " + RandomGivenFromGender(is_male);
			if (!is_male && nofemalesurname)
				return givens;
			return String.Format(givenNamesFirst ? "{0} {1}" : "{1} {0}", givens, family);
		}
		string RandomGivenFromGender(bool is_male){
			string[] names = neuterGivenNameBank
				.Concat(is_male ? maleGivenNameBank : femaleGivenNameBank).ToArray();
			int gi = Program.rng.Next(0, names.Length);
			return names[gi];
		}
	}
	class Religion : Construct {
		Religion(string name) : base(name){}
	}
}