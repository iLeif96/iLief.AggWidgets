using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MatterHackers.DataConverters3D;

namespace iLeif.CustomAggWidgets.Widgets.Views
{
    public interface IView3d : IView
    {
        Settings3d Settings { get; }

        void AppendToScene(IObject3D item);
    }
}
