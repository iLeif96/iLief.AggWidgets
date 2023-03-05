using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using MatterHackers.Agg.Platform;

namespace apisCor.PolyViewer2d.Commands
{
	public class OpenFileCommand : ICommand
	{
		public event EventHandler? CanExecuteChanged;

		private Action<OpenFileDialogParams> _action;
		private OpenFileDialogParams _params;

		public OpenFileCommand(Action<OpenFileDialogParams> action, OpenFileDialogParams inputParams)
		{
			_action = action;
			_params = inputParams;
		}

		public bool CanExecute(object? parameter)
		{
			return true;
		}

		public void Execute(object? parameter)
		{
			AggContext.FileDialogs.OpenFileDialog(_params, _action);
		}
	}
}
