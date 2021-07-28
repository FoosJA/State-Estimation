using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace State_Estimation.Model
{
	class OperInform
	{
		public int Id { get; set; }
		public string Type { get; set; }
		public string Name { get; set; }
		public int NodeNumb { get; set; }
		public double Meas { get; set; }
		public double Est { get; set; }
		public double Error { get => Meas - Est; }
		public DateTime  TimeMeas { get; set; }

		public Dictionary<int, string> keyType = new Dictionary<int, string>
		{
			[0] = "P",
			[1] = "Q",
			[2] = "U",
			[3] = "Delta",
			[4] = "Pij",
			[5] = "Qij",
			[6] = "Iij",
			[7] = "Sigma"
		};

	}
}
