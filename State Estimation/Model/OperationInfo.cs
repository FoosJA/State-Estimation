using System;
using System.ComponentModel;

namespace State_Estimation.Model
{
	public class OperationInfo : INotifyPropertyChanged
	{
		public OperationInfo() { }
		public OperationInfo(KeyType type)
		{
			Type = type;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void NotifyPropertyChanged(params string[] propertyNames)
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
			get => _id;
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
			get => _nodeNumb1;
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
			get => _nodeNumb2;
			set
			{
				if (value < 0)
					throw new ArgumentException("Некорректный номер узла", value.ToString());
				_nodeNumb2 = value;
			}
		}

		private double _measurement;

		/// <summary>
		/// Значение измерения
		/// </summary>
		public double Measurement
		{
			get => _measurement;
			set { _measurement = value; NotifyPropertyChanged("Measurement", "Error"); }
		}

		private double _estimation;
		/// <summary>
		/// Оцененное значение
		/// </summary>
		public double Estimation
		{
			get => _estimation;
			set { _estimation = value; NotifyPropertyChanged("Estimation", "Error"); }
		}

		/// <summary>
		/// Ошибка оценивания
		/// </summary>
		public double Error => Measurement - Estimation;

		/// <summary>
		/// Время замера
		/// </summary>
		public DateTime TimeMeas { get; set; }

		public enum KeyType
		{ P = 0, Q = 1, U = 2, Delta = 3, Pij = 4, Qij = 5, Iij = 6, Sigma = 7 }

	}
}
