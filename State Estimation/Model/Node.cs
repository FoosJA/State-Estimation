using System.Collections.Generic;
using State_Estimation.Infrastructure;

namespace State_Estimation.Model
{
	public class Node
	{
		/// <summary>
		/// Состояние узла
		/// </summary>
		public bool State { get; set; }

		/// <summary>
		/// Номер узла
		/// </summary>
		public int Numb { get; set; }

		/// <summary>
		/// Тип узла
		/// </summary>
		public TypeNode Type { get; set; }

		/// <summary>
		/// Строковый тип узла
		/// </summary>
		public string TypeStr => Type.ToDescriptionString();

		/// <summary>
		/// Название узла
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Номинальное напряжение, кВ
		/// </summary>
		public double Unom { get; set; }

		/// <summary>
		/// Проводимость узла, мкСм
		/// </summary>
		public double B { get; set; }

		public OperationInfo U { get; }
		public OperationInfo Delta { get; }
		public OperationInfo P { get; }
		public OperationInfo Q { get; }

		public Node()
		{
			U = new OperationInfo(OperationInfo.KeyType.U);
			Delta = new OperationInfo(OperationInfo.KeyType.Delta);
			P = new OperationInfo(OperationInfo.KeyType.P);
			Q = new OperationInfo(OperationInfo.KeyType.Q);
		}

		/// <summary>
		/// Расчёт параметров узлов на основе ветвей
		/// </summary>
		/// <param name="branchesNode"></param>
		public void GetNodeOi(IEnumerable<Branch> branchesNode)
		{
			double Pest = 0;
			double Qest = 0;
			foreach (var branchNode in branchesNode)
			{
				if (Numb == branchNode.Ni)
				{
					Pest += branchNode.Pi.Estimation;
					Qest += branchNode.Qi.Estimation;
				}
				else
				{
					Pest += branchNode.Pj.Estimation;
					Qest += branchNode.Qj.Estimation;
				}
			}
			P.Estimation = Pest;
			Q.Estimation = Qest;
		}

	}
}
