using State_Estimation;
using State_Estimation.Command;
using System.Windows.Input;
using State_Estimation.Model;
using System.Collections.ObjectModel;

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
            Assert.True(Math.Abs(stateVector[0, 0] - 116.007) < 0.1);
            Assert.True(Math.Abs(stateVector[1, 0] - 0.000746) < 0.1);
            Assert.True(Math.Abs(stateVector[2, 0] - 110.73679) < 0.1);
            Assert.True(Math.Abs(stateVector[3, 0] + 2.43292) < 0.1);
            Assert.True(Math.Abs(stateVector[4, 0] - 114.99987) < 0.1);

            var resultNode2 = vm.NodeList.First(x => x.Numb == 2);
            Assert.True(Math.Abs(resultNode2.P.Estimation + 47.257) < 0.1);
            Assert.True(Math.Abs(resultNode2.Q.Estimation + 17.276) < 0.1);
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

            try
            {
                Assert.True(Math.Abs(node1.P.Estimation + 99.764) < 0.1);
                Assert.True(Math.Abs(node1.Q.Estimation + 49.88205) < 0.1);
            }
            catch
            {
                throw new Exception("Ошибка расчета параметров узла по вектору состояния");
            }

            try
            {
                Assert.True(Math.Abs(branch1.Pj.Estimation - 112.227) < 0.1);
                Assert.True(Math.Abs(branch1.Qj.Estimation - 62.345) < 0.1);
                Assert.True(Math.Abs(branch1.Ij.Estimation * 1000 - 644.528) < 0.1);
            }
            catch 
            {
                throw new Exception("Ошибка расчета параметров ветви по вектору состояния");
            }
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

            try
            {
                Assert.True(Math.Abs(node1.P.Estimation + 99.731) < 0.1);
                Assert.True(Math.Abs(node1.Q.Estimation + 49.866) < 0.1);
            }
            catch
            {
                throw new Exception("Ошибка расчета параметров узла по вектору состояния");
            }

            try
            {
                Assert.True(Math.Abs(branch1.Pj.Estimation - 123.116) < 0.1);
                Assert.True(Math.Abs(branch1.Qj.Estimation - 77.846) < 0.1);
                Assert.True(Math.Abs(branch1.Ij.Estimation * 1000 - 731.29) < 0.1);
            }
            catch
            {
                throw new Exception("Ошибка расчета параметров ветви по вектору состояния");
            }
        }
    }
}