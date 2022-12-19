using System.ComponentModel;

namespace State_Estimation.Model
{
	public enum TypeBranch
	{
		[Description("ЛЭП")] Line = 0,
		[Description("Тр-р")] Trans = 1,
		[Description("Выкл")] Breaker = 2
	}
}
