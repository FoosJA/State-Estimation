using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace State_Estimation.Model
{
	class OperInform
	{
		/// <summary>
		/// Идентификатор ТИ
		/// </summary>
		public int Id { get; set; }
		/// <summary>
		/// Тип ОИ 
		/// </summary>
		public KeyType Type { get; set; }
		public string Name { get; set; }
		public int NodeNumb { get; set; }
		public double Meas { get; set; }
		public double Est { get; set; }
		public double Error { get => Meas - Est; }
		public DateTime  TimeMeas { get; set; }
		public enum KeyType
		{ P, Q, U, Delta, Pij, Qij, Iij, Sigma }

	}
}
