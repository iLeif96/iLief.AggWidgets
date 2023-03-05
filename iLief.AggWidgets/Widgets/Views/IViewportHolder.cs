using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using iLeif.CustomAggWidgets.Widgets.Interfaces;

using MatterHackers.Agg.UI;
using MatterHackers.DataConverters3D;

namespace iLeif.CustomAggWidgets.Widgets.Views
{
    public interface IViewportHolder : IInitializable
	{
        event EventHandler Object3DCollectionChanged;
        public Dictionary<string, IObject3D> Objects3D { get; }

        IView? Viewport { get; }
        GuiWidget Widget { get; }

        void AttachViewport(IView viewport);
    }
}
