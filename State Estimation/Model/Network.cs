using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using TypeOi = State_Estimation.Model.OperInform.KeyType;

namespace State_Estimation.Model
{
	public class Node
	{
		/// <summary>
		/// Состояние узла
		/// </summary>
		public bool Sta { get; set; }
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
		public string TypeStr { get { return Type.ToDescriptionString(); } }
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

		public OperInform U { get; private set; }
		public OperInform Delta { get; private set; }
		public OperInform P { get; private set; }
		public OperInform Q { get; private set; }

		public Node()
		{
			U = new OperInform { Type = TypeOi.U };
			Delta = new OperInform { Type = TypeOi.Delta };
			P = new OperInform { Type = TypeOi.P };
			Q = new OperInform { Type = TypeOi.Q };
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
					Pest += branchNode.Pi.Est;
					Qest += branchNode.Qi.Est;
				}
				else
				{
					Pest += branchNode.Pj.Est;
					Qest += branchNode.Qj.Est;
				}
			}
			P.Est = Pest;
			Q.Est = Qest;
		}

	}
	class State
	{
		public Node Node;
		public double U;
		public double Delta;
	}
	public class Branch
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

		public OperInform Pi { get; private set; }
		public OperInform Qi { get; private set; }
		public OperInform Ii { get; private set; }
		public OperInform Sigmai { get; private set; }
		public OperInform Pj { get; private set; }
		public OperInform Qj { get; private set; }
		public OperInform Ij { get; private set; }
		public OperInform Sigmaj { get; private set; }
		public Branch()
		{
			Pi = new OperInform { Type = TypeOi.Pij };
			Qi = new OperInform { Type = TypeOi.Qij };
			Ii = new OperInform { Type = TypeOi.Iij };
			Sigmai = new OperInform { Type = TypeOi.Sigma };
			Pj = new OperInform { Type = TypeOi.Pij };
			Qj = new OperInform { Type = TypeOi.Qij };
			Ij = new OperInform { Type = TypeOi.Iij };
			Sigmaj = new OperInform { Type = TypeOi.Sigma };
		}
		/// <summary>
		/// Расчет параметров режима ветви
		/// </summary>
		/// <param name="node_i"></param>
		/// <param name="node_j"></param>
		public void GetBranchOi(Node node_i, Node node_j)
		{
			var tuple = GetBranchParam(node_i, node_j);
			double gij = tuple.gij;
			double bij = tuple.bij;
			double gii = tuple.gii;
			double bii = tuple.bii;

			var Vj = node_j.U.Est;
			var delta_j = node_j.Delta.Est * Math.PI / 180;
			var Vi = node_i.U.Est;
			var delta_i = node_i.Delta.Est * Math.PI / 180;
			var Cij = (Vi * (gii) - Vj * (gij * Math.Cos(delta_i - delta_j) - bij * Math.Sin(delta_i - delta_j))) / Math.Sqrt(3);
			var Dij = (Vi * (bii) - Vj * (gij * Math.Sin(delta_i - delta_j) + bij * Math.Cos(delta_i - delta_j))) / Math.Sqrt(3);

			Pi.Est = Cij * Vi * Math.Sqrt(3);
			Qi.Est = Dij * Math.Sqrt(3) * Vi;
			Ii.Est = Math.Sqrt(Cij * Cij + Dij * Dij);
			Sigmai.Est = Math.Atan(Dij / Cij) / (Math.PI / 180);

			tuple = GetBranchParam(node_j, node_i);
			gij = tuple.gij;
			bij = tuple.bij;
			gii = tuple.gii;
			bii = tuple.bii;

			Vj = node_i.U.Est;
			delta_j = node_i.Delta.Est * Math.PI / 180;
			Vi = node_j.U.Est;
			delta_i = node_j.Delta.Est * Math.PI / 180;

			Cij = (Vi * (gii) - Vj * (gij * Math.Cos(delta_i - delta_j) - bij * Math.Sin(delta_i - delta_j))) / Math.Sqrt(3);
			Dij = (Vi * (bii) - Vj * (gij * Math.Sin(delta_i - delta_j) + bij * Math.Cos(delta_i - delta_j))) / Math.Sqrt(3);

			Pj.Est = Cij * Vi * Math.Sqrt(3);
			Qj.Est = Dij * Math.Sqrt(3) * Vi;
			Ij.Est = Math.Sqrt(Cij * Cij + Dij * Dij);
			Sigmaj.Est = Math.Atan(Dij / Cij) / (Math.PI / 180);
		}

		/// <summary>
		/// Расчёт параметров схемы замещения ветви
		/// </summary>
		/// <param name="node_i"></param>
		/// <param name="node_j"></param>
		/// <returns></returns>		
		public (double gij, double bij, double gii, double bii) GetBranchParam(Node node_i, Node node_j)
		{
			double gij; double bij; double gii; double bii;
			if (Kt == 1)
			{
				gij = R / (R * R + X * X);
				bij = X / (R * R + X * X);
				gii = gij + G;
				bii = bij + B / 2 * 0.000001 + node_i.B;
			}
			else
			{
				gij = (R / (R * R + X * X)) / Kt;
				bij = (X / (R * R + X * X)) / Kt;
				if (node_i.Unom > node_j.Unom)
				{
					gii = gij * Kt;
					bii = bij * Kt;
				}
				else
				{
					gii = gij / Kt;
					bii = bij / Kt;
				}
			}
			return (gij, bij, gii, bii);
		}
	}
	public enum TypeBranch
	{
		[Description("ЛЭП")] Line = 0,
		[Description("Тр-р")] Trans = 1,
		[Description("Выкл")] Breaker = 2
	}
	public enum TypeNode
	{
		[Description("База")] Base = 0,
		[Description("Нагр")] Load = 1,
		[Description("Ген")] Gen = 2,
		[Description("Ген+")] GenP = 3,
		[Description("Ген-")] GenN = 4,
		[Description("Сет")] Net = 5
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
