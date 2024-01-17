using State_Estimation;
using State_Estimation.Command;
using System.Windows.Input;
using State_Estimation.Model;
using System.Collections.ObjectModel;
using State_Estimation.Foundation;
using Xunit.Abstractions;

namespace Tests
{
    public class LogicTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public LogicTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        /// <summary>
        /// Статическое ОС для 3х узловой схемы
        /// </summary>
        [Fact]
        public void SseThreeNodes()
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

        /// <summary>
        /// Проверка расчета параметров схемы по вектору состояния
        /// </summary>
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

        /// <summary>
        /// Расчет параметров для 3х узловой кольцевой схемы для статьи
        /// </summary>
        [Fact]
        public void ForPaperRing()
        {
            var node1 = new Node(true, 1, TypeNode.Gen, "узел 1", 110, 0);
            var node2 = new Node(true, 2, TypeNode.Load, "узел 2", 110, 0);
            var node3 = new Node(true, 3, TypeNode.Base, "узел 3", 115, 0);
            var nodeList = new ObservableCollection<Node>
            {
                node3,
                node1,
                node2
            };

            var branch1 = new Branch(true, 1, TypeBranch.Line, 1, 2, 1, "ветвь 1-2", 10, 25, 0, 0, 1);
            var branch2 = new Branch(true, 2, TypeBranch.Line, 3, 1, 1, "ветвь 3-1", 10, 20, 0, 0, 1);
            var branch3 = new Branch(true, 3, TypeBranch.Line, 3, 2, 1, "ветвь 3-2", 15, 30, 0, 0, 1);
            var branchList = new ObservableCollection<Branch>
            {
                branch1,
                branch2,
                branch3
            };

            var stateList = new List<State>
            {
                new() { Node = node1, Delta = 0.00077, U = 116.007 },
                new() { Node = node2, Delta = -3.432, U = 110.736 },
                new() { Node = node3, Delta = 0, U = 115 },
            };

            Estimation.GetAllOi(stateList, nodeList, branchList);
            _testOutputHelper.WriteLine($"P1={node1.P.Estimation};Q1={node1.Q.Estimation}");
            _testOutputHelper.WriteLine($"P2={node2.P.Estimation};Q2={node2.Q.Estimation}");
            _testOutputHelper.WriteLine($"P3={node3.P.Estimation};Q3={node3.Q.Estimation}");
            _testOutputHelper.WriteLine($"P12={branch1.Pi.Estimation};Q12={branch1.Qi.Estimation}");
            _testOutputHelper.WriteLine($"P21={branch1.Pj.Estimation};Q21={branch1.Qj.Estimation}");

            _testOutputHelper.WriteLine($"P13={branch3.Pj.Estimation};Q13={branch3.Qj.Estimation}");
            _testOutputHelper.WriteLine($"P31={branch3.Pi.Estimation};Q33={branch3.Qi.Estimation}");

            _testOutputHelper.WriteLine($"P23={branch2.Pj.Estimation};Q23={branch2.Qj.Estimation}");
            _testOutputHelper.WriteLine($"P32={branch2.Pi.Estimation};Q32={branch2.Qi.Estimation}");
        }

        /// <summary>
        /// Расчет параметров для 3х узловой радиальной схемы для статьи
        /// </summary>
        [Fact]
        public void ForPaperRadial()
        {
            var node1 = new Node(true, 1, TypeNode.Gen, "узел 1", 110, 0);
            var node2 = new Node(true, 2, TypeNode.Load, "узел 2", 110, 0);
            var node3 = new Node(true, 3, TypeNode.Base, "узел 3", 115, 0);
            var nodeList = new ObservableCollection<Node>
            {
                node3,
                node1,
                node2
            };

            var branch1 = new Branch(true, 1, TypeBranch.Line, 1, 2, 1, "ветвь 1-2", 10, 25, 0, 0, 1);
            var branch3 = new Branch(true, 3, TypeBranch.Line, 3, 2, 1, "ветвь 3-2", 15, 30, 0, 0, 1);
            var branchList = new ObservableCollection<Branch>
            {
                branch1,
                branch3
            };

            var stateList = new List<State>
            {
                new() { Node = node1, Delta = 4.778, U = 120.7 },
                new() { Node = node2, Delta = 3.454, U = 114.374 },
                new() { Node = node3, Delta = 0, U = 111.737 },
            };

            Estimation.GetAllOi(stateList, nodeList, branchList);
            _testOutputHelper.WriteLine($"P1={node1.P.Estimation};Q1={node1.Q.Estimation}");
            _testOutputHelper.WriteLine($"P2={node2.P.Estimation};Q2={node2.Q.Estimation}");
            _testOutputHelper.WriteLine($"P3={node3.P.Estimation};Q3={node3.Q.Estimation}");
            _testOutputHelper.WriteLine($"P12={branch1.Pi.Estimation};Q12={branch1.Qi.Estimation}");
            _testOutputHelper.WriteLine($"P21={branch1.Pj.Estimation};Q21={branch1.Qj.Estimation}");

            _testOutputHelper.WriteLine($"P13={branch3.Pj.Estimation};Q13={branch3.Qj.Estimation}");
            _testOutputHelper.WriteLine($"P31={branch3.Pi.Estimation};Q33={branch3.Qi.Estimation}");
        }

        /// <summary>
        /// Проверка расчета схемы замещения ЛЭП
        /// </summary>
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

        /// <summary>
        /// Простой ФЛ 2х мерный
        /// </summary>
        [Fact]
        public void SimpleKalman2D()
        {
            var measurements = new List<Matrix>();
            for (var i = 0; i < 3; i++)
            {
                var item = new Matrix(1, 1)
                {
                    [0, 0] = i + 1
                };
                measurements.Add(item);
            }

            const int k = 2;
            const int col = 2;
            const int m = 1;

            var f = new Matrix(k, k)
            {
                [0, 0] = 1,
                [0, 1] = 1,
                [1, 0] = 0,
                [1, 1] = 1
            };

            var j = new Matrix(m, k)
            {
                [0, 0] = 1,
                [0, 1] = 0
            };

            var q = new Matrix(m, m)
            {
                [0, 0] = 1
            };

            var b = new Matrix(k, 1)
            {
                [0, 0] = 0,
                [1, 0] = 0
            };

            var c = new Matrix(k, col)
            {
                [0, 0] = 0,
                [0, 1] = 0,
                [1, 0] = 0,
                [1, 1] = 0
            };

            var x = new Matrix(k, 1)
            {
                [0, 0] = 0,
                [1, 0] = 0
            };

            var p = new Matrix(k, col)
            {
                [0, 0] = 1000,
                [0, 1] = 0,
                [1, 0] = 0,
                [1, 1] = 1000
            };

            // Задаем начальные значение State и Covariance
            var kalman = new KalmanFilter(b, c, f, x, p);
            foreach (var measureMatrix in measurements)
            {
                kalman.Correct(measureMatrix, j, q);
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


        /// <summary>
        /// ДинОС для 3х узловой схемы
        /// </summary>
        [Fact]
        public void DseThreeNodes()
        {
            var vm = new AppViewModel();

            #region подготовка схемы

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

            #endregion

            #region подготовка ОИ

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

            #endregion


            ICommand command = new DynamicStateEstimationCommand(vm);
            command.Execute(null);

            var stateVector = vm.StateVectorList.Last();

            //тут при q=по весовым, c=1, p=0 2 итераций
            Assert.True(Math.Abs(stateVector[0, 0] - 116.886) < 0.1, "3х узловая схема статОс: ошибка расчета U");
            Assert.True(Math.Abs(stateVector[1, 0] - 0.0473) < 0.1, "3х узловая схема статОс: ошибка расчета U");
            Assert.True(Math.Abs(stateVector[2, 0] - 110.786) < 0.1, "3х узловая схема статОс: ошибка расчета U");
            Assert.True(Math.Abs(stateVector[3, 0] + 2.503) < 0.1, "3х узловая схема статОс: ошибка расчета U");
            Assert.True(Math.Abs(stateVector[4, 0] - 110.798) < 0.1, "3х узловая схема статОс: ошибка расчета U");

            var resultNode2 = vm.NodeList.First(x => x.Numb == 2);
            Assert.True(Math.Abs(resultNode2.P.Estimation + 43.407) < 0.8,
                "3х узловая схема статОс: ошибка расчета Pij");
            Assert.True(Math.Abs(resultNode2.Q.Estimation + 7.703) < 0.8,
                "3х узловая схема статОс: ошибка расчета Qij");
        }

        /// <summary>
        /// СтатОС для 3х узловой схемы только по ТИ напряжения
        /// </summary>
        [Fact]
        public void SSEThreeNodesByOnlyVoltage()
        {
            var vm = new AppViewModel();
            CreateModelByVoltage(vm);

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

        /// <summary>
        /// СтатОС для 3х узловой схемы только по ТИ нагрузки
        /// </summary>
        [Fact]
        public void SSEThreeNodesByOnlyLoad()
        {
            var vm = new AppViewModel();
            CreateModelByLoad(vm);

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

        /// <summary>
        /// ДинОС для 3х узловой схемы толькопо ТИ напряжения
        /// </summary>
        [Fact]
        public void DSEThreeNodesByOnlyVoltage()
        {
            var vm = new AppViewModel();
            CreateModelByVoltage(vm);

            ICommand command = new DynamicStateEstimationCommand(vm);
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

        /// <summary>
        /// Создать 3х узловую схему с ТМ по напряжению
        /// </summary>
        /// <param name="vm"></param>
        private static void CreateModelByVoltage(AppViewModel vm)
        {
            #region подготовка схемы

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

            #endregion

            #region подготовка ОИ

            var u1 = new OperationInfo()
                { NodeNumb = 1, Type = OperationInfo.KeyType.U, Measurement = 116.007 };
            var delta1 = new OperationInfo()
                { NodeNumb = 1, Type = OperationInfo.KeyType.Delta, Measurement = 0.000746 };
            var u2 = new OperationInfo()
                { NodeNumb = 2, Type = OperationInfo.KeyType.U, Measurement = 110.73679 };
            var delta2 = new OperationInfo()
                { NodeNumb = 2, Type = OperationInfo.KeyType.Delta, Measurement = -2.43292 };
            var u3 = new OperationInfo()
                { NodeNumb = 3, Type = OperationInfo.KeyType.U, Measurement = 114.99987 };
            var delta3 = new OperationInfo()
                { NodeNumb = 3, Type = OperationInfo.KeyType.Delta, Measurement = 0 };


            vm.OiList = new ObservableCollection<OperationInfo>
            {
                u1,
                delta1,
                u2,
                delta2,
                u3,
                delta3,
            };

            #endregion
        }

        /// <summary>
        /// Создать 3х узловую схему с ТМ по нагрузке
        /// </summary>
        /// <param name="vm"></param>
        private static void CreateModelByLoad(AppViewModel vm)
        {
            #region подготовка схемы

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

            #endregion

            #region подготовка ОИ

            var p1 = new OperationInfo()
                { NodeNumb = 1, Type = OperationInfo.KeyType.P, Measurement = 29.746 };
            var q1 = new OperationInfo()
                { NodeNumb = 1, Type = OperationInfo.KeyType.Q, Measurement = 18.627 };
            var p2 = new OperationInfo()
                { NodeNumb = 2, Type = OperationInfo.KeyType.P, Measurement = -47.256 };
            var q2 = new OperationInfo()
                { NodeNumb = 2, Type = OperationInfo.KeyType.Q, Measurement = -17.27 };
            var p3 = new OperationInfo()
                { NodeNumb = 3, Type = OperationInfo.KeyType.P, Measurement = 18.782 };
            var q3 = new OperationInfo()
                { NodeNumb = 3, Type = OperationInfo.KeyType.Q, Measurement = 1.542 };

            var u3 = new OperationInfo()
                { NodeNumb = 3, Type = OperationInfo.KeyType.U, Measurement = 114.99987 };
            vm.OiList = new ObservableCollection<OperationInfo>
            {
                p1,
                q1,
                p2,
                q2,
                p3,
                q3,
                u3
            };

            #endregion
        }
    }
}