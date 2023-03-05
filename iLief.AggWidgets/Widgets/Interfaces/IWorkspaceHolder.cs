using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iLeif.CustomAggWidgets.Widgets.Interfaces
{
	public interface IWorkspaceHolder : IInitializable
	{
		void AddToWorkspace(params IWorkspaceItem[] wItems);
		void RemoveFromWorkspace();
		void RemoveFromWorkspace(IWorkspaceItem wItem);
	}
}
