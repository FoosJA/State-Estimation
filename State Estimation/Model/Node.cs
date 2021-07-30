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
		public double U { get; set; }
		public double Delta { get; set; }
		public double P { get; set; }
		public double Q { get; set; }		
		
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
