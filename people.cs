using System;
using System.Collections.Generic;
using System.Linq;
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
		Person(bool g, Personality p, Sexuality s){
			gender = g;
			personality = p;
			sexuality = s;
		}
		public static Person Random(){
			return new Person(MochaRandom.Bool(), Personality.Random(), Sexuality.Random());
		}
	}
	// subtypes
	class MBTI {
		static readonly string key = "eisntfjp";
		readonly bool axis1, axis2, axis3, axis4;
		MBTI(bool a1, bool a2, bool a3, bool a4){
			axis1 = a1;
			axis2 = a2;
			axis3 = a3;
			axis4 = a4;
		}
		string Name(){
			return string.Join("", new char[]{
				key[axis1 ? 1 : 0],
				key[axis2 ? 3 : 2],
				key[axis3 ? 5 : 4],
				key[axis4 ? 7 : 6],
			});
		}
		public static MBTI Random(){
			return new MBTI(MochaRandom.Bool(), MochaRandom.Bool(), MochaRandom.Bool(), MochaRandom.Bool());
		}
	}
	class Personality {
		// 128 = neutral; 255 = love; 0 = hate
		readonly byte[] lifeformPrefs, resourcePrefs;
		readonly MBTI mbti;
		Personality(byte[] lp, byte[] rp, MBTI m){
			lifeformPrefs = lp;
			resourcePrefs = rp;
			mbti = m;
		}
		public static Personality Random(){
			return new Personality(
				Bio.Lifeform.lifeforms.Select(r => MochaRandom.Byte()).ToArray(),
				Resource.resources.Select(r => MochaRandom.Byte()).ToArray(),
				MBTI.Random()
			);
		}
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
	class Construct {
		string name;
	}
	class Country : Construct {
		readonly Culture primaryCulture;
	}
	class Culture : Construct {
		readonly Language language;
		readonly Religion religion;
	}
	class Language : Construct {

	}
	class Religion : Construct {

	}
}