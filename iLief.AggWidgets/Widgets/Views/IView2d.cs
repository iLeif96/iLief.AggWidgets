using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MatterHackers.Agg;
using MatterHackers.Agg.VertexSource;

namespace iLeif.CustomAggWidgets.Widgets.Views
{
    public interface IView2d : IView
    {
		ItemBounds TotalBounds { get; }
        void ResolveBounds(bool hard = false);

        void AddRenderItem(Item2d item);
        void AddRenderItems(IEnumerable<Item2d> item);

        void RemoveRenderItem(Item2d item, bool isAutoResolveBounds = true);
        void RemoveRenderItems(List<Item2d> item, bool isAutoResolveBounds = true);
		void AddDebugRenderItem(Item2d item);
		void ClearDebugRenderItems();
	}
}
