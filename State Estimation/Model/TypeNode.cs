using System.ComponentModel;

namespace State_Estimation.Model
{
	public enum TypeNode
	{
		[Description("База")] Base = 0,
		[Description("Нагр")] Load = 1,
		[Description("Ген")] Gen = 2,
		[Description("Ген+")] GenP = 3,
		[Description("Ген-")] GenN = 4,
		[Description("Сет")] Net = 5
	}
}
