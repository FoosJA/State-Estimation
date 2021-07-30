using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace State_Estimation.Model
{
	class Branch
	{
		public bool Sta { get; set; }
		public int Numb { get; set; }
		public string TypeStr { get { return Type.ToDescriptionString(); } }
		public TypeBranch Type { get; set; }
		public int Ni { get; set; }
		public int Nj { get; set; }
		public int Paral { get; set; }
		public string Name { get; set; }
		public double R { get; set; }
		public double X { get; set; }
		public double B { get; set; }
		public double G { get; set; }
		public double Kt { get; set; }
		public double Pi { get; set; }
		public double Qi { get; set; }
		public double Ii { get; set; }
		public double Di { get; set; }
		public double Pj { get; set; }
		public double Qj { get; set; }
		public double Ij { get; set; }
		public double Dj { get; set; }

		
		
	}
	public enum TypeBranch
	{
		[Description("ЛЭП")] Line = 0,
		[Description("Тр-р")] Trans = 1,
		[Description("Выкл")] Breaker = 2
	}

}
