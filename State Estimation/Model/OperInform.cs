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
		/// <summary>
		/// Название ОИ
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// Номер узла
		/// </summary>
		public int NodeNumb { get; set; }
		/// <summary>
		/// Номер узла 2
		/// </summary>
		public int NodeNumb2 { get; set; }
		/// <summary>
		/// Значение измерения
		/// </summary>
		public double Meas { get; set; }
		/// <summary>
		/// Оцененное значение
		/// </summary>
		public double Est { get; set; }
		/// <summary>
		/// Ошибка оценивания
		/// </summary>
		public double Error { get => Meas - Est; }
		/// <summary>
		/// Время замера
		/// </summary>
		public DateTime  TimeMeas { get; set; }

		public enum KeyType
		{ P, Q, U, Delta, Pij, Qij, Iij, Sigma }

	}
}
