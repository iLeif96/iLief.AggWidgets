using System;
using iLeif.CustomAggWidgets.Widgets;
using iLeif.CustomAggWidgets.Widgets.Interfaces;
using iLeif.Extensions.Logging;

namespace iLeif.CustomAggWidgets
{
	public class DemoWindow
	{

		[STAThread]
		public static void Main(string[] args)
		{
			var loggerAggregator = new LoggerAggregator(new DebugLogger());

			var window = new WorkspaceWindow(new(loggerAggregator), true);

			var sliderItem = new WorkspaceItemBase(new CustomSliderWidget(0, 10, 50, text: "Demo"));
			sliderItem.Name = "Simple slider";
			sliderItem.Widget.AnchorCenter();
			window.AddToWorkspace(sliderItem);

			window.Initialize();
			window.ShowAsSystemWindow();


		}
	}
}
