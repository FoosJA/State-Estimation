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

                var delta = oiList.SingleOrDefault(
                    x => x.Type == OperationInfo.KeyType.Delta && x.NodeNumb == node.Numb);
                st.Delta = delta?.Measurement ?? 0.0001;

                stateList.Add(st);
            }

            return new List<State>(stateList.OrderByDescending(x => x.Node.Type));
        }

        /// <summary>
        /// Обновить лист состояния. Обновляется на каждой итерации
        /// </summary>
        /// <param name="u"></param>
        /// <param name="stateList"></param>
        public static List<State> UpdateState(Matrix u, List<State> stateList)
        {
            var i = 0;
            for (var j = 0; j < stateList.Count(); j++)
            {
                stateList[j].U = u[i, 0];
                i++;
                if (stateList[j].Node.Type == TypeNode.Base)
                {
                    continue;
                }

                stateList[j].Delta = u[i, 0];
                i++;
            }

            return stateList;
        }

        public static Matrix GetStateVector(IList<State> stateList, IList<Node> nodeList)
        {
            var k = 2 * nodeList.Count - 1;
            var u = new Matrix(k, 1);
            int j = 0;
            foreach (var st in stateList)
            {
                u[j, 0] = st.U;
                j++;
                if (st.Node.Type != TypeNode.Base)
                {
                    u[j, 0] = st.Delta;
                    j++;
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

        public static Matrix GetCalcVector(IList<State> stateList, IList<Node> nodeList, IList<Branch> branchList,
            IList<OperationInfo> oiList)
        {
            GetAllOi(stateList, nodeList, branchList);
            var fErr = new Matrix(oiList.Count, 1);
            var m = 0;
            foreach (var meas in oiList)
            {
                var nodeI = nodeList.First(x => x.Numb == meas.NodeNumb);
                var nodeJ = nodeList.FirstOrDefault(x => x.Numb == meas.NodeNumb2);
                var type = meas.Type;
                if (nodeJ == null)
                {
                    switch (type)
                    {
                        case OperationInfo.KeyType.U:
                            fErr[m, 0] = nodeI.U.Estimation;
                            m++;
                            break;
                        case OperationInfo.KeyType.Delta:
                            //if (node_i.Type != TypeNode.Base)
                            //{
                            fErr[m, 0] = nodeI.Delta.Estimation;
                            m++;
                            //}
                            break;
                        case OperationInfo.KeyType.P:
                            fErr[m, 0] = nodeI.P.Estimation;
                            m++;
                            break;
                        case OperationInfo.KeyType.Q:
                            fErr[m, 0] = nodeI.Q.Estimation;
                            m++;
                            break;
                    }
                }
                else
                {
                    var branch = branchList.First(x => (x.Ni == nodeI.Numb && x.Nj == nodeJ.Numb) ||
                                                       (x.Ni == nodeJ.Numb && x.Nj == nodeI.Numb));
                    switch (type)
                    {
                        case OperationInfo.KeyType.Pij:
                            if (branch.Ni == nodeI.Numb)
                            {
                                fErr[m, 0] = branch.Pi.Estimation;
                            }
                            else
                            {
                                fErr[m, 0] = branch.Pj.Estimation;
                            }

                            m++;
                            break;
                        case OperationInfo.KeyType.Qij:
                            if (branch.Ni == nodeI.Numb)
                            {
                                fErr[m, 0] = branch.Qi.Estimation;
                            }
                            else
                            {
                                fErr[m, 0] = branch.Qj.Estimation;
                            }

                            m++;
                            break;
                        case OperationInfo.KeyType.Iij:
                            if (branch.Ni == nodeI.Numb)
                            {
                                fErr[m, 0] = branch.Ii.Estimation;
                            }
                            else
                            {
                                fErr[m, 0] = branch.Ij.Estimation;
                            }

                            m++;
                            break;
                        case OperationInfo.KeyType.Sigma:
                            if (branch.Ni == nodeI.Numb)
                            {
                                fErr[m, 0] = branch.Sigmai.Estimation;
                            }
                            else
                            {
                                fErr[m, 0] = branch.Sigmaj.Estimation;
                            }

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
                var nodeI = nodeList.First(x => x.Numb == branch.Ni);
                var nodeJ = nodeList.First(x => x.Numb == branch.Nj);
                branch.GetBranchOi(nodeI, nodeJ);
            }

            foreach (var node in nodeList)
            {
                //var Vi = node.U.Estimation;
                //var delta_i = node.Delta.Estimation * Math.PI / 180;
                var branchesNode = branchList.Where(x => x.Ni == node.Numb || x.Nj == node.Numb);
                node.GetNodeOi(branchesNode);
            }
        }

        public static Matrix GetJacobian(IList<State> stateList, IList<Node> nodeList, IList<Branch> branchList,
            IList<OperationInfo> oiList)
        {
            var jacobian = new Matrix(oiList.Count, 2 * nodeList.Count - 1);
            var m = 0;
            foreach (var meas in oiList)
            {
                int k;
                double J_Vi = 0;
                double J_Deltai = 0;
                var nodeI = nodeList.First(x => x.Numb == meas.NodeNumb);
                var branchesNode = branchList.Where(x => x.Ni == nodeI.Numb || x.Nj == nodeI.Numb);
                var nodeJ = nodeList.FirstOrDefault(x => x.Numb == meas.NodeNumb2);
                var type = meas.Type;
                if (nodeJ == null)
                {
                    switch (type)
                    {
                        case OperationInfo.KeyType.U:
                            k = 2 * nodeList.IndexOf(nodeI);
                            jacobian[m, k] = 1;
                            m++;
                            break;

                        case OperationInfo.KeyType.Delta:
                            if (nodeI.Type != TypeNode.Base)
                            {
                                k = 2 * nodeList.IndexOf(nodeI);
                                jacobian[m, k + 1] = 1;
                                m++;
                            }

                            break;

                        case OperationInfo.KeyType.P:
                            foreach (var branchNode in branchesNode)
                            {
                                if (branchNode.Ni != nodeI.Numb)
                                    nodeJ = nodeList.First(x => x.Numb == branchNode.Ni);
                                else
                                    nodeJ = nodeList.First(x => x.Numb == branchNode.Nj);

                                var (gij, bij, gii, _) = branchNode.GetBranchParam(nodeI, nodeJ);

                                var vi = stateList.First(x => x.Node == nodeI).U;
                                var deltaI = stateList.First(x => x.Node == nodeI).Delta * Math.PI / 180;
                                var vj = stateList.First(x => x.Node == nodeJ).U;
                                var deltaJ = stateList.First(x => x.Node == nodeJ).Delta * Math.PI / 180;

                                var a = bij * Math.Sin(deltaI - deltaJ) - gij * Math.Cos(deltaI - deltaJ);
                                var b = -bij * Math.Cos(deltaI - deltaJ) - gij * Math.Sin(deltaI - deltaJ);
                                J_Vi += 2 * vi * (gii) + vj * a;
                                J_Deltai += (-vi) * vj * b * Math.PI / 180;
                                k = 2 * nodeList.IndexOf(nodeJ);
                                jacobian[m, k] = vi * a;
                                if (nodeJ.Type != TypeNode.Base)
                                {
                                    jacobian[m, k + 1] = vi * vj * b * Math.PI / 180;
                                }
                            }

                            k = 2 * nodeList.IndexOf(nodeI);
                            jacobian[m, k] = J_Vi;
                            if (nodeI.Type != TypeNode.Base)
                            {
                                jacobian[m, k + 1] = J_Deltai;
                            }

                            m++;
                            break;

                        case OperationInfo.KeyType.Q:
                            foreach (var branchNode in branchesNode)
                            {
                                if (branchNode.Ni != nodeI.Numb)
                                    nodeJ = nodeList.First(x => x.Numb == branchNode.Ni);
                                else
                                    nodeJ = nodeList.First(x => x.Numb == branchNode.Nj);

                                var (gij, bij, _, bii) = branchNode.GetBranchParam(nodeI, nodeJ);

                                var vi = stateList.First(x => x.Node == nodeI).U;
                                var deltaI = stateList.First(x => x.Node == nodeI).Delta * Math.PI / 180;
                                var vj = stateList.First(x => x.Node == nodeJ).U;
                                var deltaJ = stateList.First(x => x.Node == nodeJ).Delta * Math.PI / 180;

                                var a = bij * Math.Sin(deltaI - deltaJ) - gij * Math.Cos(deltaI - deltaJ);
                                var b = -bij * Math.Cos(deltaI - deltaJ) - gij * Math.Sin(deltaI - deltaJ);

                                J_Vi += 2 * vi * (bii) + vj * b;
                                J_Deltai += vi * vj * a * Math.PI / 180;
                                k = 2 * nodeList.IndexOf(nodeJ);
                                jacobian[m, k] = vi * b;
                                if (nodeJ.Type != TypeNode.Base)
                                {
                                    jacobian[m, k + 1] = (-vi) * vj * a * Math.PI / 180;
                                }
                            }

                            k = 2 * nodeList.IndexOf(nodeI);
                            jacobian[m, k] = J_Vi;
                            if (nodeI.Type != TypeNode.Base)
                            {
                                jacobian[m, k + 1] = J_Deltai;
                            }

                            m++;
                            break;
                    }
                }
                else
                {
                    var vi = stateList.First(x => x.Node == nodeI).U;
                    var deltaI = stateList.First(x => x.Node == nodeI).Delta * Math.PI / 180;
                    var vj = stateList.First(x => x.Node == nodeJ).U;
                    var deltaJ = stateList.First(x => x.Node == nodeJ).Delta * Math.PI / 180;

                    var branch = branchList.First(x => (x.Ni == nodeI.Numb && x.Nj == nodeJ.Numb) ||
                                                       (x.Ni == nodeJ.Numb && x.Nj == nodeI.Numb));

                    var (gij, bij, gii, bii) = branch.GetBranchParam(nodeI, nodeJ);

                    var c = (vi * (gii) -
                                vj * (gij * Math.Cos(deltaI - deltaJ) - bij * Math.Sin(deltaI - deltaJ))) /
                               Math.Sqrt(3);
                    var d = (vi * (bii) -
                                vj * (gij * Math.Sin(deltaI - deltaJ) + bij * Math.Cos(deltaI - deltaJ))) /
                               Math.Sqrt(3);
                    if (c == 0 && d == 0)
                    {
                        c = 0.0001; //TODO: чтобы учесть корректно токи
                        d = 0.0001;
                    }

                    var a = bij * Math.Sin(deltaI - deltaJ) - gij * Math.Cos(deltaI - deltaJ);
                    var b = -bij * Math.Cos(deltaI - deltaJ) - gij * Math.Sin(deltaI - deltaJ);

                    switch (type)
                    {
                        case OperationInfo.KeyType.Pij:
                            k = 2 * nodeList.IndexOf(nodeI);
                            jacobian[m, k] = 2 * vi * (gii) + vj * a;
                            if (nodeI.Type != TypeNode.Base)
                            {
                                jacobian[m, k + 1] = (-vi) * vj * b * Math.PI / 180;
                            }

                            k = 2 * nodeList.IndexOf(nodeJ);
                            jacobian[m, k] = vi * a;
                            if (nodeJ.Type != TypeNode.Base)
                            {
                                jacobian[m, k + 1] = vi * vj * b * Math.PI / 180;
                            }

                            m++;
                            break;
                        case OperationInfo.KeyType.Qij:
                            k = 2 * nodeList.IndexOf(nodeI);
                            jacobian[m, k] = 2 * vi * (bii) + vj * b;
                            if (nodeI.Type != TypeNode.Base)
                            {
                                jacobian[m, k + 1] = vi * vj * a * Math.PI / 180;
                            }

                            k = 2 * nodeList.IndexOf(nodeJ);
                            jacobian[m, k] = vi * b;
                            if (nodeJ.Type != TypeNode.Base)
                            {
                                jacobian[m, k + 1] = (-vi) * vj * a * Math.PI / 180;
                            }

                            m++;
                            break;
                        case OperationInfo.KeyType.Iij:
                            k = 2 * nodeList.IndexOf(nodeI);
                            jacobian[m, k] = (Math.Pow(c * c + d * d, (-0.5)) / Math.Sqrt(3)) *
                                      (c * (gij + gii) + d * (bij + bii));
                            if (nodeI.Type != TypeNode.Base)
                            {
                                jacobian[m, k + 1] = ((vj * Math.Pow(c * c + d * d, (-0.5))) / Math.Sqrt(3)) *
                                              (Math.PI / 180) * (d * a - c * b);
                            }

                            k = 2 * nodeList.IndexOf(nodeJ);
                            jacobian[m, k] = (Math.Pow(c * c + d * d, (-0.5)) / Math.Sqrt(3)) *
                                      (c * a + d * b);
                            if (nodeJ.Type != TypeNode.Base)
                            {
                                jacobian[m, k + 1] = ((vj * Math.Pow(c * c + d * d, (-0.5))) / Math.Sqrt(3)) *
                                              (Math.PI / 180) * (c * b - d * a);
                            }

                            m++;
                            break;
                        case OperationInfo.KeyType.Sigma:
                            k = 2 * nodeList.IndexOf(nodeI);
                            jacobian[m, k] = ((bij + bii) * c - (gij + gii) * d) /
                                      (Math.Sqrt(3) * (c * c + d * d) * (Math.PI / 180));
                            if (nodeI.Type != TypeNode.Base)
                            {
                                jacobian[m, k + 1] = (vj * (a * c + b * d) /
                                               (Math.Sqrt(3) * (c * c + d * d)));
                            }

                            k = 2 * nodeList.IndexOf(nodeJ);
                            jacobian[m, k] = (b * c - a * d) /
                                      (Math.Sqrt(3) * (c * c + d * d) * (Math.PI / 180));

                            if (nodeJ.Type != TypeNode.Base)
                            {
                                jacobian[m, k + 1] = (-vj * (a * c + b * d) /
                                               (Math.Sqrt(3) * (c * c + d * d)));
                            }

                            m++;
                            break;
                    }
                }
            }

            return jacobian;
        }

        public static Matrix GetWeightMatrix(Matrix jacobian, IList<OperationInfo> oiList, bool isWeightCoefAuto)
        {
            var weightMatrix = Matrix.IdentityMatrix(oiList.Count, oiList.Count);
            if (isWeightCoefAuto)
            {
                for (var i = 0; i < oiList.Count; i++)
                {
                    double cii = 0;
                    for (var j = 0; j < jacobian.cols; j++)
                    {
                        cii += jacobian[i, j] * jacobian[i, j];
                    }

                    weightMatrix[i, i] = 1 / cii;
                }
            }

            else
            {
                for (var i = 0; i < oiList.Count; i++)
                {
                    weightMatrix[i, i] = 5;
                }
            }

            return weightMatrix;
        }

        public static Matrix GetTrancMatrix(int nodeCount)
        {
            var k = 2 * nodeCount - 1;
            var D = Matrix.IdentityMatrix(k, k);
            //TODO: Пока генерируется единичная матрица. Далее вместо единиц необходимо рассчитывать коэф а=(х\х)/z
            return D;
        }
    }
}