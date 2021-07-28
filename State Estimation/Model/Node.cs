using System.Collections.Generic;

namespace State_Estimation.Model
{
	class Node
	{
		public bool Sta { get; set; }
		public int Numb { get; set; }
		public string Type { get; set; }
		public int TypeIndex { get; set; }
		public string Name { get; set; }
		public double Unom { get; set; }
		public double B { get; set; }
		public double U { get; set; }
		public double Delta { get; set; }
		public double P { get; set; }
		public double Q { get; set; }

		public Dictionary<int, string> keyType = new Dictionary<int, string>
		{
			[0] = "База",
			[1] = "Нагр",
			[2] = "Ген",
			[3] = "Ген+",
			[4] = "Ген-",
			[5] = "Сет"
		};
	}
}
