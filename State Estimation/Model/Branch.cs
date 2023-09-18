using System;
using State_Estimation.Infrastructure;

namespace State_Estimation.Model
{
    public class Branch
    {
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

        public Branch(bool sta, int numb, TypeBranch type, int ni, int nj, int parallelNumb, string name, double r,
            double x, double b, double g, double kt) : this()
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
        /// <param name="nodeI"></param>
        /// <param name="nodeJ"></param>
        public void GetBranchOi(Node nodeI, Node nodeJ)
        {
            var tuple = GetBranchParam(nodeI, nodeJ);
            double gij = tuple.gij;
            double bij = tuple.bij;
            double gii = tuple.gii;
            double bii = tuple.bii;

            var vj = nodeJ.U.Estimation;
            var deltaJ = nodeJ.Delta.Estimation * Math.PI / 180;
            var vi = nodeI.U.Estimation;
            var deltaI = nodeI.Delta.Estimation * Math.PI / 180;
            var c = (vi * (gii) - vj * (gij * Math.Cos(deltaI - deltaJ) - bij * Math.Sin(deltaI - deltaJ))) /
                      Math.Sqrt(3);
            var d = (vi * (bii) - vj * (gij * Math.Sin(deltaI - deltaJ) + bij * Math.Cos(deltaI - deltaJ))) /
                      Math.Sqrt(3);

            Pi.Estimation = c * vi * Math.Sqrt(3);
            Qi.Estimation = d * Math.Sqrt(3) * vi;
            Ii.Estimation = Math.Sqrt(c * c + d * d);
            Sigmai.Estimation = Math.Atan(d / c) / (Math.PI / 180);

            tuple = GetBranchParam(nodeJ, nodeI);
            gij = tuple.gij;
            bij = tuple.bij;
            gii = tuple.gii;
            bii = tuple.bii;

            vj = nodeI.U.Estimation;
            deltaJ = nodeI.Delta.Estimation * Math.PI / 180;
            vi = nodeJ.U.Estimation;
            deltaI = nodeJ.Delta.Estimation * Math.PI / 180;

            c = (vi * (gii) - vj * (gij * Math.Cos(deltaI - deltaJ) - bij * Math.Sin(deltaI - deltaJ))) /
                  Math.Sqrt(3);
            d = (vi * (bii) - vj * (gij * Math.Sin(deltaI - deltaJ) + bij * Math.Cos(deltaI - deltaJ))) /
                  Math.Sqrt(3);

            Pj.Estimation = c * vi * Math.Sqrt(3);
            Qj.Estimation = d * Math.Sqrt(3) * vi;
            Ij.Estimation = Math.Sqrt(c * c + d * d);
            Sigmaj.Estimation = Math.Atan(d / c) / (Math.PI / 180);
        }

        /// <summary>
        /// Расчёт параметров схемы замещения ветви
        /// </summary>
        /// <param name="nodeI"></param>
        /// <param name="nodeJ"></param>
        /// <returns></returns>		
        public (double gij, double bij, double gii, double bii) GetBranchParam(Node nodeI, Node nodeJ)
        {
            double gij;
            double bij;
            double gii;
            double bii;
            if (Math.Abs(Kt - 1) < 0.000001)
            {
                gij = R / (R * R + X * X);
                bij = X / (R * R + X * X);
                gii = gij + G / 2 * 0.000001;
                bii = bij + B / 2 * 0.000001 + nodeI.B;
            }
            else
            {
                gij = (R / (R * R + X * X)) / Kt;
                bij = (X / (R * R + X * X)) / Kt;
                if (nodeI.Unom > nodeJ.Unom)
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