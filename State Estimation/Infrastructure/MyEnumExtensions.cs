using System.ComponentModel;
using State_Estimation.Model;

namespace State_Estimation.Infrastructure
{
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
