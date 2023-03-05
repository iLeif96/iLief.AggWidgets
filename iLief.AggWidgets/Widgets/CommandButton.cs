using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using iLeif.CustomAggWidgets;
using iLeif.CustomAggWidgets.Widgets.Interfaces;

using MatterHackers.Agg.UI;

namespace iLeif.CustomAggWidgets.Widgets
{


	public class CommandButton : Button, ICommandInvoker
	{
		private class CommandWrapper : ICommand
		{
			public Func<object>? GetParameterFunction;
			private readonly ICommand _command;
			public CommandWrapper(ICommand command, Func<object>? getParameterFunction)
			{
				_command = command;
				GetParameterFunction = getParameterFunction;
			}

			public event EventHandler? CanExecuteChanged
			{
				add { _command.CanExecuteChanged += value; }
				remove { _command.CanExecuteChanged -= value; }
			}

			public bool CanExecute(object? parameter)
			{
				return _command.CanExecute(parameter);
			}

			public void Execute(object? parameter)
			{
				if (GetParameterFunction != null)
				{
					_command.Execute(GetParameterFunction());
					return;
				}
				_command.Execute(parameter);
			}
		}

		private CommandWrapper _commandWrapper;
		public ICommand Command
		{
			get => _commandWrapper;
			set { _commandWrapper = new CommandWrapper(value, GetParameterForCommand); }
		}


		private Func<object>? _getParameterForCommand;
		public Func<object>? GetParameterForCommand
		{
			get => _getParameterForCommand;
			set { _getParameterForCommand = value; _commandWrapper.GetParameterFunction = value; }
		}

		public CommandButton(string title, ICommand command) : base(title)
		{
			Width = Defaults.ButtonSizeX;
			Height = Defaults.ButtonSizeY;
			Command = command;
		}

		public CommandButton(string title, double x, double y, ICommand command) : base(title, x, y)
		{
			Command = command;
		}

		protected override void OnClick(MouseEventArgs mouseEvent)
		{
			Command?.Execute(this);
			base.OnClick(mouseEvent);
		}

	}
}
