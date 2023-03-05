using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using iLeif.Extensions.Logging;

using MatterHackers.Agg.UI;

namespace iLeif.CustomAggWidgets.Widgets
{
	public class StatusBar : LayoutLayer, ITextWriter
	{
		public override string Text { get => _textWidget.Text; }

		private TextWidget _textWidget;
		public StatusBar(FlowDirection flowDirection, int width, int height) : base(flowDirection, width, height)
		{
			MatterHackers.Agg.Font.Justification justification = MatterHackers.Agg.Font.Justification.Left;
			if (flowDirection == FlowDirection.RightToLeft)
			{
				justification = MatterHackers.Agg.Font.Justification.Right;
			}

			_textWidget = new TextWidget("Status bar", justification: justification);
		}

		public override void Initialize()
		{
			if (Initialized == true)
			{
				return;
			}

			_textWidget.Width = Width;
			AddChild(_textWidget, VAnchor.Center, HAnchor.Center);

			base.Initialize();
		}

		public void WriteLine(string text)
		{
			if (_textWidget.Text != text)
			{
				_textWidget.Text = text;
				_textWidget.DoExpandBoundsToText();
			}

			DebugShowBounds = false;
			_textWidget.DebugShowBounds= false;
		}
	}
}
