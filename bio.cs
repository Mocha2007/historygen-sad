namespace Bio {
	class Animal {
		readonly BodyPart[] parts;
		readonly string name;
		readonly int mass; // in grams
		readonly bool carnivorous, herbivorous;
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
			parent = head;
			tags = new string[0];
		}
		static readonly BodyPart head = new BodyPart("head");
	}
	class Plant {
		readonly bool deciduous;
		// 0 = moss/lichen/grass; 1 = small plant-like (think weed); 2 = bushy; 3 = tree
		readonly byte type;
		readonly int maturity_time; // in days???
		readonly int max_mass; // in g
	}
}