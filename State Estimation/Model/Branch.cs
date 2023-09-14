using System;
using State_Estimation.Infrastructure;

namespace State_Estimation.Model
{
	public class Branch
	{
		private const double _nomKt = 1;
		public bool Sta { get; set; }
		public int Numb { get; set; }
		public string TypeStr => Type.ToDescriptionString();
		public TypeBranch Type { get; set; }
		public int Ni { get; set; }
		public int Nj { get; set; }
		public int ParallelNumb { get; set; }
		public string Name { get; set; }
		public double R { get; set; }
		public double X { get; set; }
		public double B { get; set; }
		public double G { get; set; }
		public double Kt { get; set; }

		public OperationInfo Pi { get; }
		public OperationInfo Qi { get; }
		public OperationInfo Ii { get; }
		public OperationInfo Sigmai { get; }
		public OperationInfo Pj { get; }
		public OperationInfo Qj { get; }
		public OperationInfo Ij { get; }
		public OperationInfo Sigmaj { get; }

		public Branch()
		{
			Pi = new OperationInfo(OperationInfo.KeyType.Pij);
			Qi = new OperationInfo(OperationInfo.KeyType.Qij);
			Ii = new OperationInfo(OperationInfo.KeyType.Iij);
			Sigmai = new OperationInfo(OperationInfo.KeyType.Sigma);
			Pj = new OperationInfo(OperationInfo.KeyType.Pij);
			Qj = new OperationInfo(OperationInfo.KeyType.Qij);
			Ij = new OperationInfo(OperationInfo.KeyType.Iij);
			Sigmaj = new OperationInfo(OperationInfo.KeyType.Sigma);
		}

		public Branch(bool sta, int numb, TypeBranch type, int ni, int nj, int parallelNumb, string name, double r, double x, double b, double g, double kt):this()
		{
			Sta = sta;
			Numb = numb;
			Type = type;
			Ni = ni;
			Nj = nj;
			ParallelNumb = parallelNumb;
			Name = name;
			R = r;
			X = x;
			B = b;
			G = g;
			Kt = kt;
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

			var Vj = node_j.U.Estimation;
			var delta_j = node_j.Delta.Estimation * Math.PI / 180;
			var Vi = node_i.U.Estimation;
			var delta_i = node_i.Delta.Estimation * Math.PI / 180;
			var Cij = (Vi * (gii) - Vj * (gij * Math.Cos(delta_i - delta_j) - bij * Math.Sin(delta_i - delta_j))) / Math.Sqrt(3);
			var Dij = (Vi * (bii) - Vj * (gij * Math.Sin(delta_i - delta_j) + bij * Math.Cos(delta_i - delta_j))) / Math.Sqrt(3);

			Pi.Estimation = Cij * Vi * Math.Sqrt(3);
			Qi.Estimation = Dij * Math.Sqrt(3) * Vi;
			Ii.Estimation = Math.Sqrt(Cij * Cij + Dij * Dij);
			Sigmai.Estimation = Math.Atan(Dij / Cij) / (Math.PI / 180);

			tuple = GetBranchParam(node_j, node_i);
			gij = tuple.gij;
			bij = tuple.bij;
			gii = tuple.gii;
			bii = tuple.bii;

			Vj = node_i.U.Estimation;
			delta_j = node_i.Delta.Estimation * Math.PI / 180;
			Vi = node_j.U.Estimation;
			delta_i = node_j.Delta.Estimation * Math.PI / 180;

			Cij = (Vi * (gii) - Vj * (gij * Math.Cos(delta_i - delta_j) - bij * Math.Sin(delta_i - delta_j))) / Math.Sqrt(3);
			Dij = (Vi * (bii) - Vj * (gij * Math.Sin(delta_i - delta_j) + bij * Math.Cos(delta_i - delta_j))) / Math.Sqrt(3);

			Pj.Estimation = Cij * Vi * Math.Sqrt(3);
			Qj.Estimation = Dij * Math.Sqrt(3) * Vi;
			Ij.Estimation = Math.Sqrt(Cij * Cij + Dij * Dij);
			Sigmaj.Estimation = Math.Atan(Dij / Cij) / (Math.PI / 180);
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
			if (Kt == _nomKt)
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

		public override string ToString()
		{
			return $"{Sta};{Numb};{Type};{Ni};{Nj};{ParallelNumb};{Name};{R};{X};{B};{G};{Kt}";
		}
	}
}
