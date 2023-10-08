using State_Estimation;
using State_Estimation.Command;
using System.Windows.Input;
using State_Estimation.Model;
using System.Collections.ObjectModel;
using State_Estimation.Foundation;

namespace Tests
{
    public class LogicTests
    {
        [Fact]
        public void StateEstimationThreeNodes()
        {
            var vm = new AppViewModel();

            var node1 = new Node(true, 1, TypeNode.Gen, "узел 1", 110, 0);
            var node2 = new Node(true, 2, TypeNode.Load, "узел 2", 110, 0);
            var node3 = new Node(true, 3, TypeNode.Base, "узел 3", 115, 0);
            vm.NodeList = new ObservableCollection<Node>
            {
                node3,
                node1,
                node2
            };

            var branch1 = new Branch(true, 1, TypeBranch.Line, 1, 2, 1, "ветвь 1-2", 10, 25, 0, 0, 1);
            var branch2 = new Branch(true, 2, TypeBranch.Line, 3, 1, 1, "ветвь 3-1", 10, 20, 0, 0, 1);
            var branch3 = new Branch(true, 3, TypeBranch.Line, 3, 2, 1, "ветвь 3-2", 15, 30, 0, 0, 1);
            vm.BranchList = new ObservableCollection<Branch>
            {
                branch1,
                branch2,
                branch3
            };

            var p1 = new OperationInfo() { NodeNumb = 1, Type = OperationInfo.KeyType.P, Measurement = 30 };
            var q1 = new OperationInfo() { NodeNumb = 1, Type = OperationInfo.KeyType.Q, Measurement = 19 };

            var p3 = new OperationInfo() { NodeNumb = 3, Type = OperationInfo.KeyType.P, Measurement = 19 };
            var q3 = new OperationInfo() { NodeNumb = 3, Type = OperationInfo.KeyType.Q, Measurement = 2 };

            var p32 = new OperationInfo()
                { NodeNumb = 3, NodeNumb2 = 2, Type = OperationInfo.KeyType.Pij, Measurement = 21 };
            var q32 = new OperationInfo()
                { NodeNumb = 3, NodeNumb2 = 2, Type = OperationInfo.KeyType.Qij, Measurement = 6 };

            var u3 = new OperationInfo()
                { NodeNumb = 3, Type = OperationInfo.KeyType.U, Measurement = 115 };

            vm.OiList = new ObservableCollection<OperationInfo>
            {
                p1,
                q1,
                p3,
                q3,
                p32,
                q32,
                u3
            };

            ICommand command = new StaticStateEstimationCommand(vm);
            command.Execute(null);

            var stateVector = vm.StateVectorList.Last();
            Assert.True(Math.Abs(stateVector[0, 0] - 116.007) < 0.1, "3х узловая схема статОс: ошибка расчета U");
            Assert.True(Math.Abs(stateVector[1, 0] - 0.000746) < 0.1, "3х узловая схема статОс: ошибка расчета U");
            Assert.True(Math.Abs(stateVector[2, 0] - 110.73679) < 0.1, "3х узловая схема статОс: ошибка расчета U");
            Assert.True(Math.Abs(stateVector[3, 0] + 2.43292) < 0.1, "3х узловая схема статОс: ошибка расчета U");
            Assert.True(Math.Abs(stateVector[4, 0] - 114.99987) < 0.1, "3х узловая схема статОс: ошибка расчета U");

            var resultNode2 = vm.NodeList.First(x => x.Numb == 2);
            Assert.True(Math.Abs(resultNode2.P.Estimation + 47.257) < 0.1,
                "3х узловая схема статОс: ошибка расчета Pij");
            Assert.True(Math.Abs(resultNode2.Q.Estimation + 17.276) < 0.1,
                "3х узловая схема статОс: ошибка расчета Qij");
        }

        [Fact]
        public void GetAllOiTest()
        {
            var node1 = new Node(true, 1, TypeNode.Gen, "узел 1", 110, 0);
            var node2 = new Node(true, 2, TypeNode.Base, "узел 2", 115, 0);
            var nodeList = new ObservableCollection<Node>
            {
                node1,
                node2
            };

            var stateList = new List<State>
            {
                new() { Node = node1, Delta = -2.48816, U = 99.914 },
                new() { Node = node2, Delta = 0, U = 115 }
            };

            var branch1 = new Branch(true, 1, TypeBranch.Line, 1, 2, 1, "ветвь 1-2", 10, 10, 0, 0, 1);
            var branchList = new ObservableCollection<Branch> { branch1 };

            Estimation.GetAllOi(stateList, nodeList, branchList);

            Assert.True(Math.Abs(node1.P.Estimation + 99.764) < 0.1, "Некорректный расчет Pi по U");
            Assert.True(Math.Abs(node1.Q.Estimation + 49.88205) < 0.1, "Некорректный расчет Qi по U");

            Assert.True(Math.Abs(branch1.Pj.Estimation - 112.227) < 0.1, "Некорректный расчет Pij по U");
            Assert.True(Math.Abs(branch1.Qj.Estimation - 62.345) < 0.1, "Некорректный расчет Qij по U");
            Assert.True(Math.Abs(branch1.Ij.Estimation * 1000 - 644.528) < 0.1, "Некорректный расчет Iij по U");
        }

        [Fact]
        public void GetOiLineTest()
        {
            var node1 = new Node(true, 1, TypeNode.Gen, "узел 1", 110, 0);
            var node2 = new Node(true, 2, TypeNode.Base, "узел 2", 115, 0);
            var nodeList = new ObservableCollection<Node>
            {
                node1,
                node2
            };

            var stateList = new List<State>
            {
                new() { Node = node1, Delta = -2.41785, U = 98.76293 },
                new() { Node = node2, Delta = 0, U = 115 }
            };

            var branch1 = new Branch(true, 1, TypeBranch.Line, 1, 2, 1, "ветвь 1-2", 10, 10, 1200, 800, 1);
            var branchList = new ObservableCollection<Branch> { branch1 };

            Estimation.GetAllOi(stateList, nodeList, branchList);

            Assert.True(Math.Abs(node1.P.Estimation + 99.731) < 0.1, "Некорректный расчет Pi по U");
            Assert.True(Math.Abs(node1.Q.Estimation + 49.866) < 0.1, "Некорректный расчет Qi по U");

            Assert.True(Math.Abs(branch1.Pj.Estimation - 123.116) < 0.1, "Некорректный расчет Pij по U");
            Assert.True(Math.Abs(branch1.Qj.Estimation - 77.846) < 0.1, "Некорректный расчет Qij по U");
            Assert.True(Math.Abs(branch1.Ij.Estimation * 1000 - 731.29) < 0.1, "Некорректный расчет Iij по U");
        }

        [Fact]
        public void SimpleKalman2D()
        {
            var measurements = new List<Matrix>();
            for (var i = 0; i < 3; i++)
            {
                var item = new Matrix(1, 1);
                item[0, 0] = i + 1;
                measurements.Add(item);
            }

            const int row = 2;
            const int col = 2;

            var f = new Matrix(row, col);
            f[0, 0] = 1;
            f[0, 1] = 1;
            f[1, 0] = 0;
            f[1, 1] = 1;

            var h = new Matrix(1, col);
            h[0, 0] = 1;
            h[0, 1] = 0;

            var r = new Matrix(1, 1);
            r[0, 0] = 1;

            var q = new Matrix(row, 1);
            q[0, 0] = 0;
            q[1, 0] = 0;

            var c = new Matrix(row, col);
            c[0, 0] = 0;
            c[0, 1] = 0;
            c[1, 0] = 0;
            c[1, 1] = 0;

            var kalman = new KalmanFilter(q, r, f, h, c);

            var x = new Matrix(row, 1);
            x[0, 0] = 0;
            x[1, 0] = 0;

            var p = new Matrix(row, col);
            p[0, 0] = 1000;
            p[0, 1] = 0;
            p[1, 0] = 0;
            p[1, 1] = 1000;

            // Задаем начальные значение State и Covariance
            kalman.SetState(x, p);

            foreach (var measureMatrix in measurements)
            {
                kalman.Correct(measureMatrix);
            }

            var state = kalman.State;
            var covariance = kalman.Covariance;
            Assert.True(Math.Abs(state[0, 0] - 3.9996) < 0.01 && Math.Abs(state[1, 0] - 0.9999) < 0.01,
                "вектор состояния не совпадает с результатом");
            
            Assert.True(Math.Abs(covariance[0, 0] - 2.331) < 0.01 && Math.Abs(covariance[0, 1] - 0.99916) < 0.01
                                                                  && Math.Abs(covariance[1, 0] - 0.999167) < 0.01 &&
                                                                  Math.Abs(covariance[1, 1] - 0.4995) < 0.01,
                "ковариация не совпадает с результатом");
        }
    }
}