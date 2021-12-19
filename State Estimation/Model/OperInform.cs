using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace State_Estimation.Model
{
	class OperInform : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		protected void NotifyPropertyChanged(params string[] propertyNames)
		{
			if (PropertyChanged != null)
			{
				foreach (string propertyName in propertyNames)
				{
					PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
				}
			}
		}
		private int _id;
		/// <summary>
		/// Идентификатор ТИ
		/// </summary>
		public int Id
		{
			get { return _id; }
			set
			{
				if (value <= 0)
					throw new ArgumentException("Некорректный ID измерения", value.ToString());
				_id = value;
			}
		}
		/// <summary>
		/// Тип ОИ 
		/// </summary>
		public KeyType Type { get; set; }
		/// <summary>
		/// Название ОИ
		/// </summary>
		public string Name { get; set; }

		private int _nodeNumb1;
		/// <summary>
		/// Номер узла
		/// </summary>
		public int NodeNumb
		{
			get { return _nodeNumb1; }
			set
			{
				if (value <= 0)
					throw new ArgumentException("Некорректный номер узла", value.ToString());
				_nodeNumb1 = value;
			}
		}
		private int _nodeNumb2;
		/// <summary>
		/// Номер узла 2
		/// </summary>
		public int NodeNumb2
		{
			get { return _nodeNumb2; }
			set
			{
				if (value < 0)
					throw new ArgumentException("Некорректный номер узла", value.ToString());
				_nodeNumb2 = value;
			}
		}

		private double _Meas;
		/// <summary>
		/// Значение измерения
		/// </summary>
		public double Meas
		{
			get { return _Meas; }
			set { _Meas = value; NotifyPropertyChanged("Meas", "Error"); }
		}

		private double _Est;
		/// <summary>
		/// Оцененное значение
		/// </summary>
		public double Est
		{
			get { return _Est; }
			set { _Est = value; NotifyPropertyChanged("Est", "Error"); }
		}
		/// <summary>
		/// Ошибка оценивания
		/// </summary>
		public double Error
		{
			get { return Meas - Est; }
		}

		/// <summary>
		/// Время замера
		/// </summary>
		public DateTime TimeMeas { get; set; }

		public enum KeyType
		{ P, Q, U, Delta, Pij, Qij, Iij, Sigma }

	}
}
