using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using iLeif.CustomAggWidgets.Widgets.Interfaces;

using MatterHackers.Agg;
using MatterHackers.Agg.UI;

namespace iLeif.CustomAggWidgets.Widgets.Views
{
    public interface IView : IInitializable, IDisposable
	{
        Action<Graphics2D>? ActionOnDraw { get; set; }
        GuiWidget? UiLayer { get; }
		GuiWidget DrawLayer { get; }
		GuiWidget Widget { get; }
        bool IsFitContentIntoView { get; set; }
        void ClearScene();
        void ResetView();
    }
}
