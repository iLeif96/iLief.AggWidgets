using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MatterHackers.Agg.UI;

namespace iLeif.CustomAggWidgets.Widgets.Interfaces
{
	public interface IWorkspaceItem : IReinitializable, IInitializable
	{
		string Name { get; }

		GuiWidget Widget { get; }
	}
}
