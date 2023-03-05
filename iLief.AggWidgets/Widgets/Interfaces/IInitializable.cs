using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iLeif.CustomAggWidgets.Widgets.Interfaces
{
	public interface IInitializable
	{
		public bool Initialized { get; }

		void Initialize();
	}
}
