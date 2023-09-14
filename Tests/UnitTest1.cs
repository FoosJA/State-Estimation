using State_Estimation;
using State_Estimation.Command;
using System.Windows.Input;
using State_Estimation.Model;
using System;
using Xunit;

namespace Tests
{
	public class UnitTest1
	{
		[Fact]
		public void Test1()
		{
			var vm = new AppViewModel();

			var node1 = new Node(true, 1, TypeNode.Gen, "узел 1", 110, 0);
			var node2 = new Node(true, 2, TypeNode.Load, "узел 2", 110, 0);
			var node3 = new Node(true, 3, TypeNode.Base, "узел 3", 115, 0);
			vm.NodeList.Add(node3);
			vm.NodeList.Add(node1);
			vm.NodeList.Add(node2);

			var branch1 = new Branch(true, 1, TypeBranch.Line, 1, 2, 1, "ветвь 1-2", 10, 25, 0, 0, 1);
			var branch2 = new Branch(true, 2, TypeBranch.Line, 3, 1, 1, "ветвь 3-1", 10, 20, 0, 0, 1);
			var branch3 = new Branch(true, 3, TypeBranch.Line, 3, 2, 1, "ветвь 3-2", 15, 30, 0, 0, 1);
			vm.BranchList.Add(branch1);
			vm.BranchList.Add(branch2);
			vm.BranchList.Add(branch3);

			var p1 = new OperationInfo() { NodeNumb = 1, Type = OperationInfo.KeyType.P, Measurement = 30 };
			var q1 = new OperationInfo() { NodeNumb = 1, Type = OperationInfo.KeyType.Q, Measurement = 19 };

			var p3 = new OperationInfo() { NodeNumb = 3, Type = OperationInfo.KeyType.P, Measurement = 19 };
			var q3 = new OperationInfo() { NodeNumb = 3, Type = OperationInfo.KeyType.Q, Measurement = 2 };

			var p32 = new OperationInfo() { NodeNumb = 3, NodeNumb2 = 2, Type = OperationInfo.KeyType.P, Measurement = 21 };
			var q32 = new OperationInfo() { NodeNumb = 3, NodeNumb2 = 2, Type = OperationInfo.KeyType.Q, Measurement = 6 };

			vm.OiList.Add(p1);
			vm.OiList.Add(q1);
			vm.OiList.Add(p3);
			vm.OiList.Add(q3);
			vm.OiList.Add(p32);
			vm.OiList.Add(q32);


			ICommand command = new StaticStateEstimationCommand(vm);
			command.Execute(null);
			var t = vm.NodeList.First(x => x.Numb == 1).U.Estimation;
			double tt = 116.2115;
			//Assert.True(Math.Abs(t - 116.2115) < 0.1);
			Assert.True(vm.CanSE() == true);
			//Assert.Equal(t, tt);
		}
	}
}