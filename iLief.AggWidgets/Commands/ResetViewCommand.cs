using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using iLeif.CustomAggWidgets.Widgets.Views;

namespace iLeif.CustomAggWidgets.Commands
{
    public class ResetViewCommand : ICommand
	{
		private IView _view;
		public ResetViewCommand(IView view)
		{
			_view = view;
		}

		public event EventHandler? CanExecuteChanged;

		public bool CanExecute(object? parameter)
		{
			return true;
		}

		public void Execute(object? parameter)
		{
			_view.ResetView();
		}
	}
}
