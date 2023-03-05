using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iLeif.CustomAggWidgets.Widgets.Interfaces
{
	public interface IReinitializable : IInitializable
	{
		event EventHandler ReinitializationBegin;
		event EventHandler ReinitializationEnd;

		void Reinitializing();
	}
}
