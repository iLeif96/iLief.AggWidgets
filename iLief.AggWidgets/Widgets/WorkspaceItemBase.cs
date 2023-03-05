using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using iLeif.CustomAggWidgets.Widgets.Interfaces;

using MatterHackers.Agg.UI;

namespace iLeif.CustomAggWidgets.Widgets
{
	public class WorkspaceItemBase : IWorkspaceItem
	{
		public event EventHandler ReinitializationBegin;
		public event EventHandler ReinitializationEnd;

		public GuiWidget Widget { get; protected set; }
		public bool Initialized { get; protected set; } = false;
		public string Name { get => Widget.Name; set { Widget.Name = value; } }

		public WorkspaceItemBase()
		{

		}

		public WorkspaceItemBase(GuiWidget widget)
		{
			Widget = widget;
		}

		virtual public void Initialize()
		{
			if (Initialized)
			{
				return;
			}

			Widget?.Initialize();

			Initialized = true;
		}

		protected virtual void OnReinitializing()
		{
			Initialized = false;
			Initialize();
		}

		public void Reinitializing()
		{
			ReinitializationBegin?.Invoke(this, EventArgs.Empty);

			OnReinitializing();

			ReinitializationEnd?.Invoke(this, EventArgs.Empty);
		}
	}
}
