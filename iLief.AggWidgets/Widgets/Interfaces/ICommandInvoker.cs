using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace iLeif.CustomAggWidgets.Widgets.Interfaces
{
	public interface ICommandInvoker
    {
        Func<object>? GetParameterForCommand { get; set; }
        ICommand Command { get; set; }
    }
}
