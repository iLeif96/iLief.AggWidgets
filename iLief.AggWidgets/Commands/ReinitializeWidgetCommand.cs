using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using iLeif.CustomAggWidgets.Widgets.Interfaces;

namespace iLeif.CustomAggWidgets.Commands
{
	public class ReinitializeWidgetCommand : ICommand
	{
		public event EventHandler? CanExecuteChanged;

		private readonly IReinitializable _widget;
		public ReinitializeWidgetCommand(IReinitializable widget)
		{
			_widget = widget;
		}


		public bool CanExecute(object? parameter)
		{
			return true;
		}

		public void Execute(object? parameter)
		{
			_widget.Reinitializing();
		}
	}
}
