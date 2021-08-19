using System.Collections.Generic;
using System.ComponentModel;

namespace State_Estimation.Model
{
	class Node
	{
		public bool Sta { get; set; }
		public int Numb { get; set; }
		public TypeNode Type { get; set; }
		public string TypeStr { get { return Type.ToDescriptionString(); } }
		public int TypeIndex { get; set; }
		public string Name { get; set; }
		public double Unom { get; set; }
		public double B { get; set; }
		public OperInform U { get; set; }
		public OperInform Delta { get; set; }
		public OperInform P { get; set; }
		public OperInform Q { get; set; }				
	}
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
		public OperInform Pi { get; set; }
		public OperInform Qi { get; set; }
		public OperInform Ii { get; set; }
		public OperInform Sigmai { get; set; }
		public OperInform Pj { get; set; }
		public OperInform Qj { get; set; }
		public OperInform Ij { get; set; }
		public OperInform Sigmaj { get; set; }
	}
	public enum TypeBranch
	{
		[Description("ЛЭП")] Line = 0,
		[Description("Тр-р")] Trans = 1,
		[Description("Выкл")] Breaker = 2
	}
	public enum TypeNode
	{
		[Description("База")] Base=0,
		[Description("Нагр")] Load=1,
		[Description("Ген")] Gen=2,
		[Description("Ген+")] GenP=3,
		[Description("Ген-")] GenN=4,
		[Description("Сет")] Net=5
	}

	/// <summary>
	/// Для отображения наименования перечислений в DataGrid
	/// </summary>
	public static class MyEnumExtensions
	{
		public static string ToDescriptionString(this TypeNode val)
		{
			DescriptionAttribute[] attributes = (DescriptionAttribute[])val
			   .GetType()
			   .GetField(val.ToString())
			   .GetCustomAttributes(typeof(DescriptionAttribute), false);
			return attributes.Length > 0 ? attributes[0].Description : string.Empty;
		}
		public static string ToDescriptionString(this TypeBranch val)
		{
			DescriptionAttribute[] attributes = (DescriptionAttribute[])val
			   .GetType()
			   .GetField(val.ToString())
			   .GetCustomAttributes(typeof(DescriptionAttribute), false);
			return attributes.Length > 0 ? attributes[0].Description : string.Empty;
		}
	}
}
