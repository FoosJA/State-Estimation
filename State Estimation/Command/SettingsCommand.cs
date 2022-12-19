using System;
using System.Windows;
using System.Windows.Input;

namespace State_Estimation.Command
{
	internal class SettingsCommand : ICommand
	{
		private readonly AppViewModel _vm;
		public SettingsCommand(AppViewModel vm)
		{
			_vm = vm;
		}

		public bool CanExecute(object parameter) => true;

		public void Execute(object parameter)
		{
			var myWindow = parameter as Window;
			var settings = new Settings(_vm.GetRatioByJacobi, _vm.MaxError, _vm.MaxIteration, _vm.A) { Owner = myWindow };
			settings.ShowDialog();
			if (settings.SaveChange)
			{
				_vm.MaxIteration = settings.MaxIterac;
				_vm.MaxError = settings.MaxError;
				_vm.A = settings.A;
				_vm.GetRatioByJacobi = settings.FlagVesCoef;
			}
		}

		public event EventHandler CanExecuteChanged
		{
			add => CommandManager.RequerySuggested += value;
			remove => CommandManager.RequerySuggested -= value;
		}
	}
}
