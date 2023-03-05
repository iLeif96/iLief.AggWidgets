using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iLeif.CustomAggWidgets.Widgets.Interfaces;
using iLeif.Extensions.Logging;
using MatterHackers.Agg.UI;

namespace iLeif.CustomAggWidgets.Widgets.Interfaces
{
    public interface IOutputHolder : IInitializable
	{
		ILogger Logger { get; }
		void ClearOutput();
	}
}
