using System;
using System.Collections.Generic;
using System.Linq;
using State_Estimation.Foundation;

namespace State_Estimation.Model
{
	public static class Estimation
	{
		/// <summary>
		/// Создать лист состояния. Создается в начале расчёта
		/// </summary>
		/// <returns>Лист узлов со значениями U и дельта</returns>
		public static List<State> CreateStateList(IEnumerable<Node> nodeList, IList<OperationInfo> oiList)
		{
			var stateList = new List<State>();
			foreach (var node in nodeList)
			{
				var st = new State { Node = node };

				var u = oiList.SingleOrDefault(x => x.Type == OperationInfo.KeyType.U && x.NodeNumb == node.Numb);
				st.U = u?.Measurement ?? node.Unom;

				var delta = oiList.SingleOrDefault(x => x.Type == OperationInfo.KeyType.Delta && x.NodeNumb == node.Numb);
				st.Delta = delta?.Measurement ?? 0.0001;

				stateList.Add(st);
			}
			return stateList;
		}

		/// <summary>
		/// Обновить лист состояния. Обновляется на каждой итерации
		/// </summary>
		/// <param name="U"></param>
		/// <param name="stateList"></param>
		public static List<State> UpdateState(Matrix U, List<State> stateList)
		{
			var i = 0;
			for (var j = 0; j < stateList.Count(); j++)
			{
				stateList[j].U = U[i, 0];
				i++;
				if (stateList[j].Node.Type == TypeNode.Base) continue;

				stateList[j].Delta = U[i, 0];
				i++;
			}
			return stateList;
		}

		public static Matrix GetStateVector(IEnumerable<State> stateList, IList<Node> nodeList)
		{
			var k = 2 * nodeList.Count - 1;
			var u = new Matrix(k, 1);
			foreach (var st in stateList)
			{
				var i = nodeList.IndexOf(st.Node) * 2;
				u[i, 0] = st.U;
				if (st.Node.Type != TypeNode.Base)
				{
					u[i + 1, 0] = st.Delta;
				}
			}
			return u;
		}
		public static Matrix GetMeasVector(IList<OperationInfo> oiList)
		{
			var f = new Matrix(oiList.Count, 1);
			for (var i = 0; i < oiList.Count; i++)
			{
				f[i, 0] = oiList[i].Measurement;
			}
			return f;
		}
		public static Matrix GetCalcVector(IList<State> stateList, IList<Node> nodeList, IList<Branch> branchList, IList<OperationInfo> oiList)
		{
			GetAllOi(stateList, nodeList, branchList);
			var fErr = new Matrix(oiList.Count, 1);
			var m = 0;
			foreach (var meas in oiList)
			{
				var node_i = nodeList.First(x => x.Numb == meas.NodeNumb);
				var node_j = nodeList.FirstOrDefault(x => x.Numb == meas.NodeNumb2);
				var type = meas.Type;
				if (node_j == null)
				{
					switch (type)
					{
						case OperationInfo.KeyType.U:
							fErr[m, 0] = node_i.U.Estimation;
							m++;
							break;
						case OperationInfo.KeyType.Delta:
							//if (node_i.Type != TypeNode.Base)
							//{
							fErr[m, 0] = node_i.Delta.Estimation;
							m++;
							//}
							break;
						case OperationInfo.KeyType.P:
							fErr[m, 0] = node_i.P.Estimation;
							m++;
							break;
						case OperationInfo.KeyType.Q:
							fErr[m, 0] = node_i.Q.Estimation;
							m++;
							break;
					};
				}
				else
				{
					var branch = branchList.First(x => (x.Ni == node_i.Numb && x.Nj == node_j.Numb) ||
														  (x.Ni == node_j.Numb && x.Nj == node_i.Numb));
					switch (type)
					{
						case OperationInfo.KeyType.Pij:
							if (branch.Ni == node_i.Numb) { fErr[m, 0] = branch.Pi.Estimation; }
							else { fErr[m, 0] = branch.Pj.Estimation; }
							m++;
							break;
						case OperationInfo.KeyType.Qij:
							if (branch.Ni == node_i.Numb) { fErr[m, 0] = branch.Qi.Estimation; }
							else { fErr[m, 0] = branch.Qj.Estimation; }
							m++;
							break;
						case OperationInfo.KeyType.Iij:
							if (branch.Ni == node_i.Numb) { fErr[m, 0] = branch.Ii.Estimation; }
							else { fErr[m, 0] = branch.Ij.Estimation; }
							m++;
							break;
						case OperationInfo.KeyType.Sigma:
							if (branch.Ni == node_i.Numb) { fErr[m, 0] = branch.Sigmai.Estimation; }
							else { fErr[m, 0] = branch.Sigmaj.Estimation; }
							m++;
							break;
					}
				}
			}
			return fErr;
		}

		public static void GetAllOi(IList<State> stateList, IList<Node> nodeList, IList<Branch> branchList)
		{
			foreach (var node in nodeList)
			{
				var stateNode = stateList.First(x => x.Node == node);

				//var i = nodeList.IndexOf(node) * 2;
				node.U.Estimation = stateNode.U;
				if (node.Type != TypeNode.Base)
					node.Delta.Estimation = stateNode.Delta;
				else
					node.Delta.Estimation = 0;
			}
			foreach (var branch in branchList)
			{
				var node_i = nodeList.First(x => x.Numb == branch.Ni);
				var node_j = nodeList.First(x => x.Numb == branch.Nj);
				branch.GetBranchOi(node_i, node_j);
			}
			foreach (var node in nodeList)
			{
				//var Vi = node.U.Estimation;
				//var delta_i = node.Delta.Estimation * Math.PI / 180;
				var branchesNode = branchList.Where(x => x.Ni == node.Numb || x.Nj == node.Numb);
				node.GetNodeOi(branchesNode);
			}
		}

		public static Matrix GetJacobian(IList<State> stateList, IList<Node> nodeList, IList<Branch> branchList, IList<OperationInfo> oiList)
		{
			var J = new Matrix(oiList.Count, 2 * nodeList.Count - 1);
			var m = 0;
			foreach (var meas in oiList)
			{
				var k = 0;
				double J_Vi = 0;
				double J_Deltai = 0;
				var node_i = nodeList.First(x => x.Numb == meas.NodeNumb);
				var branchesNode = branchList.Where(x => x.Ni == node_i.Numb || x.Nj == node_i.Numb);
				var node_j = nodeList.FirstOrDefault(x => x.Numb == meas.NodeNumb2);
				var type = meas.Type;
				if (node_j == null)
				{
					switch (type)
					{
						case OperationInfo.KeyType.U:
							k = 2 * nodeList.IndexOf(node_i);
							J[m, k] = 1;
							m++;
							break;

						case OperationInfo.KeyType.Delta:
							if (node_i.Type != TypeNode.Base)
							{
								k = 2 * nodeList.IndexOf(node_i);
								J[m, k + 1] = 1;
								m++;
							}
							break;

						case OperationInfo.KeyType.P:
							foreach (var branchNode in branchesNode)
							{
								if (branchNode.Ni != node_i.Numb)
									node_j = nodeList.First(x => x.Numb == branchNode.Ni);
								else
									node_j = nodeList.First(x => x.Numb == branchNode.Nj);

								var (gij, bij, gii, bii) = branchNode.GetBranchParam(node_i, node_j);

								var Vi = stateList.FirstOrDefault(x => x.Node == node_i).U;
								var delta_i = stateList.FirstOrDefault(x => x.Node == node_i).Delta * Math.PI / 180;
								var Vj = stateList.FirstOrDefault(x => x.Node == node_j).U;
								var delta_j = stateList.FirstOrDefault(x => x.Node == node_j).Delta * Math.PI / 180;

								var Cij = (Vi * (gii) - Vj * (gij * Math.Cos(delta_i - delta_j) - bij * Math.Sin(delta_i - delta_j))) / Math.Sqrt(3);
								var Aij = bij * Math.Sin(delta_i - delta_j) - gij * Math.Cos(delta_i - delta_j);
								var Bij = -bij * Math.Cos(delta_i - delta_j) - gij * Math.Sin(delta_i - delta_j);
								J_Vi += 2 * Vi * (gii) + Vj * Aij;
								J_Deltai += (-Vi) * Vj * Bij * Math.PI / 180;
								k = 2 * nodeList.IndexOf(node_j);
								J[m, k] = Vi * Aij;
								if (node_j.Type != TypeNode.Base)
								{
									J[m, k + 1] = Vi * Vj * Bij * Math.PI / 180;
								}
							}
							k = 2 * nodeList.IndexOf(node_i);
							J[m, k] = J_Vi;
							if (node_i.Type != TypeNode.Base)
							{
								J[m, k + 1] = J_Deltai;
							}
							m++;
							break;

						case OperationInfo.KeyType.Q:
							foreach (var branchNode in branchesNode)
							{
								if (branchNode.Ni != node_i.Numb)
									node_j = nodeList.First(x => x.Numb == branchNode.Ni);
								else
									node_j = nodeList.First(x => x.Numb == branchNode.Nj);

								var (gij, bij, gii, bii) = branchNode.GetBranchParam(node_i, node_j);

								var Vi = stateList.FirstOrDefault(x => x.Node == node_i).U;
								var delta_i = stateList.FirstOrDefault(x => x.Node == node_i).Delta * Math.PI / 180;
								var Vj = stateList.FirstOrDefault(x => x.Node == node_j).U;
								var delta_j = stateList.FirstOrDefault(x => x.Node == node_j).Delta * Math.PI / 180;

								var Dij = (Vi * (bii) - Vj * (gij * Math.Sin(delta_i - delta_j) + bij * Math.Cos(delta_i - delta_j))) / Math.Sqrt(3);
								var Aij = bij * Math.Sin(delta_i - delta_j) - gij * Math.Cos(delta_i - delta_j);
								var Bij = -bij * Math.Cos(delta_i - delta_j) - gij * Math.Sin(delta_i - delta_j);

								J_Vi += 2 * Vi * (bii) + Vj * Bij;
								J_Deltai += Vi * Vj * Aij * Math.PI / 180;
								k = 2 * nodeList.IndexOf(node_j);
								J[m, k] = Vi * Bij;
								if (node_j.Type != TypeNode.Base)
								{
									J[m, k + 1] = (-Vi) * Vj * Aij * Math.PI / 180;
								}
							}
							k = 2 * nodeList.IndexOf(node_i);
							J[m, k] = J_Vi;
							if (node_i.Type != TypeNode.Base)
							{
								J[m, k + 1] = J_Deltai;
							}
							m++;
							break;
					};
				}
				else if (node_j != null)
				{
					var Vi = stateList.FirstOrDefault(x => x.Node == node_i).U;
					var delta_i = stateList.FirstOrDefault(x => x.Node == node_i).Delta * Math.PI / 180;
					var Vj = stateList.FirstOrDefault(x => x.Node == node_j).U;
					var delta_j = stateList.FirstOrDefault(x => x.Node == node_j).Delta * Math.PI / 180;

					var branch = branchList.First(x => (x.Ni == node_i.Numb && x.Nj == node_j.Numb) ||
					  (x.Ni == node_j.Numb && x.Nj == node_i.Numb));

					var (gij, bij, gii, bii) = branch.GetBranchParam(node_i, node_j);

					var _Cij = (Vi * (gii) - Vj * (gij * Math.Cos(delta_i - delta_j) - bij * Math.Sin(delta_i - delta_j))) / Math.Sqrt(3);
					var _Dij = (Vi * (bii) - Vj * (gij * Math.Sin(delta_i - delta_j) + bij * Math.Cos(delta_i - delta_j))) / Math.Sqrt(3);
					if (_Cij == 0 && _Dij == 0)
					{
						_Cij = 0.0001;//TODO: чтобы учесть корректно токи
						_Dij = 0.0001;
					}
					var _Aij = bij * Math.Sin(delta_i - delta_j) - gij * Math.Cos(delta_i - delta_j);
					var _Bij = -bij * Math.Cos(delta_i - delta_j) - gij * Math.Sin(delta_i - delta_j);

					switch (type)
					{
						case OperationInfo.KeyType.Pij:
							k = 2 * nodeList.IndexOf(node_i);
							J[m, k] = 2 * Vi * (gii) + Vj * _Aij;
							if (node_i.Type != TypeNode.Base)
							{
								J[m, k + 1] = (-Vi) * Vj * _Bij * Math.PI / 180;
							}
							k = 2 * nodeList.IndexOf(node_j);
							J[m, k] = Vi * _Aij;
							if (node_j.Type != TypeNode.Base)
							{
								J[m, k + 1] = Vi * Vj * _Bij * Math.PI / 180;
							}
							m++;
							break;
						case OperationInfo.KeyType.Qij:
							k = 2 * nodeList.IndexOf(node_i);
							J[m, k] = 2 * Vi * (bii) + Vj * _Bij;
							if (node_i.Type != TypeNode.Base)
							{
								J[m, k + 1] = Vi * Vj * _Aij * Math.PI / 180;
							}
							k = 2 * nodeList.IndexOf(node_j);
							J[m, k] = Vi * _Bij;
							if (node_j.Type != TypeNode.Base)
							{
								J[m, k + 1] = (-Vi) * Vj * _Aij * Math.PI / 180;
							}
							m++;
							break;
						case OperationInfo.KeyType.Iij:
							k = 2 * nodeList.IndexOf(node_i);
							var test1 = (Math.Pow(_Cij * _Cij + _Dij * _Dij, (-0.5)) / Math.Sqrt(3)) * (_Cij * (gij + gii) + _Dij * (bij + bii));
							J[m, k] = (Math.Pow(_Cij * _Cij + _Dij * _Dij, (-0.5)) / Math.Sqrt(3)) * (_Cij * (gij + gii) + _Dij * (bij + bii));
							if (node_i.Type != TypeNode.Base)
							{
								J[m, k + 1] = ((Vj * Math.Pow(_Cij * _Cij + _Dij * _Dij, (-0.5))) / Math.Sqrt(3)) *
									(Math.PI / 180) * (_Dij * _Aij - _Cij * _Bij);
							}
							k = 2 * nodeList.IndexOf(node_j);
							J[m, k] = (Math.Pow(_Cij * _Cij + _Dij * _Dij, (-0.5)) / Math.Sqrt(3)) * (_Cij * _Aij + _Dij * _Bij);
							if (node_j.Type != TypeNode.Base)
							{
								J[m, k + 1] = ((Vj * Math.Pow(_Cij * _Cij + _Dij * _Dij, (-0.5))) / Math.Sqrt(3)) *
									(Math.PI / 180) * (_Cij * _Bij - _Dij * _Aij);
							}
							m++;
							break;
						case OperationInfo.KeyType.Sigma:
							k = 2 * nodeList.IndexOf(node_i);
							J[m, k] = ((bij + bii) * _Cij - (gij + gii) * _Dij) / (Math.Sqrt(3) * (_Cij * _Cij + _Dij * _Dij) * (Math.PI / 180));
							if (node_i.Type != TypeNode.Base)
							{
								J[m, k + 1] = (Vj * (_Aij * _Cij + _Bij * _Dij) / (Math.Sqrt(3) * (_Cij * _Cij + _Dij * _Dij)));

							}
							k = 2 * nodeList.IndexOf(node_j);
							J[m, k] = (_Bij * _Cij - _Aij * _Dij) / (Math.Sqrt(3) * (_Cij * _Cij + _Dij * _Dij) * (Math.PI / 180));

							if (node_j.Type != TypeNode.Base)
							{
								J[m, k + 1] = (-Vj * (_Aij * _Cij + _Bij * _Dij) / (Math.Sqrt(3) * (_Cij * _Cij + _Dij * _Dij)));
							}
							m++;
							break;
					}
				}
			}
			return J;
		}

		public static Matrix GetWeightMatrix(Matrix J, IList<OperationInfo> oiList, bool IsWeightCoefAuto)
		{
			var C = Matrix.IdentityMatrix(oiList.Count, oiList.Count);
			if (IsWeightCoefAuto)
			{
				for (var i = 0; i < oiList.Count; i++)
				{

					double cii = 0;
					for (var j = 0; j < J.cols; j++)
					{
						cii += J[i, j] * J[i, j];
					}
					C[i, i] = 1 / cii;
				}
			}

			else
			{
				for (var i = 0; i < oiList.Count; i++)
				{
					C[i, i] = 5;
				}
			}
			return C;
		}

		public static Matrix GetTrancMatrix(int nodeCount)
		{
			var K = 2 * nodeCount - 1;
			var D = Matrix.IdentityMatrix(K, K);
			//TODO: Пока генерируется единичная матрица. Далее вместо единиц необходимо рассчитывать коэф а=(х\х)/z
			return D;
		}
	}
}
