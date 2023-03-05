using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iLeif.CustomAggWidgets.Widgets;
using iLeif.Extensions.Logging;
using MatterHackers.Agg.Font;
using MatterHackers.Agg.UI;
using MatterHackers.RayTracer.Light;
using MatterHackers.VectorMath;

namespace iLeif.CustomAggWidgets.Widgets
{

    public class ScrollableTextWidget : ScrollableWidget, ITextWriter
	{
		public class BarInfo
		{
			public int StringsCount { get; set; } = 0;
			public LinkedList<string> Strings { get; set; } = new LinkedList<string>();
			public Vector2 ScrollPosition { get; set; } = new Vector2(0, 0);
			public int StringsLimit { get; set; } = 60;
		}
		public int FontSize { get; private set; }

		public bool IsTextExpanded { get; private set; } = false;

		protected static Dictionary<string, BarInfo> _barInfos = new Dictionary<string, BarInfo>();
		protected BarInfo _barInfo;
		protected object _locker = new();

		private TextWidget? _textWidget;
		private string _initText = "Viewer is loaded and ready to beat them all... \n";

		private bool _isUsingDelayPrinting = true;
		private CancellationToken _token = CancellationToken.None;
		private long _lastPrintingTimeMs = 0;
		private long _timeForPrintingAfterDelayMs = 300;
		private Task? _printingTask;

		public ScrollableTextWidget(string name, int height, int fontSize = 12)
		{
			Name = name;
			Height = height;
			FontSize = fontSize;

			if (!_barInfos.ContainsKey(name))
			{
				_barInfos.Add(name, new BarInfo());
			}

			_barInfo = _barInfos[name];

			if (_barInfo.StringsCount == 0)
			{
				PutStringToCache(_initText);
			}

		}

		public override void Initialize()
		{
			_lastPrintingTimeMs = UiThread.CurrentTimerMs;
			_textWidget = new TextWidget(_initText, pointSize: FontSize, justification: Justification.Left);
			_textWidget.Height = Height;//new MatterHackers.VectorMath.Vector2(Width, 200);
			_textWidget.Margin = new MatterHackers.Agg.BorderDouble(20, 0);
			_textWidget.VAnchor = VAnchor.Bottom;
			_textWidget.Printer.Baseline = Baseline.BoundsTop;
			PutCacheToTextWidget();
			this.AddChild(_textWidget);

			BackgroundColor = new MatterHackers.Agg.Color(210, 210, 210);
			AutoScroll = true;
			ScrollPosition = _barInfo.ScrollPosition;
			ScrollPositionChanged += OnScrollPositionChanged;
			Padding = new MatterHackers.Agg.BorderDouble(20, 0, 0, 0);

			base.Initialize();
		}

		public override void OnMouseEnterBounds(MouseEventArgs mouseEvent)
		{
			base.OnMouseEnterBounds(mouseEvent);
			IsTextExpanded = true;
			PutCacheToTextWidget();
		}

		public override void OnMouseLeaveBounds(MouseEventArgs mouseEvent)
		{
			base.OnMouseLeaveBounds(mouseEvent);
			IsTextExpanded = false;
			PutCacheToTextWidget();
		}

		public void AnchorBottom()
		{
			VAnchor = VAnchor.Bottom;
			HAnchor = HAnchor.Stretch;
		}

		public void WriteLine(string text)
		{
			lock (_locker)
			{
				PutStringToCache(text);

				var timeFromLastPrinting = UiThread.CurrentTimerMs - _lastPrintingTimeMs;

				if (_isUsingDelayPrinting == false)
				{
					PutCacheToTextWidget();
					return;
				}

				if (timeFromLastPrinting < _timeForPrintingAfterDelayMs && _printingTask == null)
				{
					_printingTask = Task.Run(async () =>
					{
						await Task.Delay((int)_timeForPrintingAfterDelayMs, _token);

						PutCacheToTextWidget();
						_printingTask = null;
					});

					return;
				}

				if (timeFromLastPrinting >= _timeForPrintingAfterDelayMs && _printingTask != null)
				{
					try
					{
						_printingTask.Dispose();
					}
					catch { };

					_printingTask = null;
				}

				if (_printingTask == null)
				{
					PutCacheToTextWidget();
				}			
			}
		}

		private void PutStringToCache(string text)
		{
			if (!text.EndsWith("\n"))
			{
				text += "\n";
			}

			if (_barInfo.StringsCount > _barInfo.StringsLimit)
			{
				_barInfo.Strings.RemoveFirst();
				_barInfo.StringsCount--;
			}

			_barInfo.StringsCount++;
			_barInfo.Strings.AddLast(text);
		}

		public void ClearBuffer()
		{
			if (_barInfo == null)
			{
				return;
			}

			_barInfo.StringsCount = 0;
			_barInfo.Strings = new LinkedList<string>();
			_barInfo.ScrollPosition = new Vector2(0, 0);
		}

		private void PutCacheToTextWidget()
		{
			if (_textWidget == null)
			{
				return;
			}

			bool isNeedToScroll = (ScrollPosition.Y > -0.5);

			StringBuilder stringBuilder = new StringBuilder();

			if (!IsTextExpanded)
			{
				if (_barInfo.StringsCount > 2)
				{
					stringBuilder.Append("\n");
				}
				foreach (var str in _barInfo.Strings.TakeLast(2))
				{
					stringBuilder.Append(str);
				}
			}
			else
			{
				foreach (var str in _barInfo.Strings)
				{
					stringBuilder.Append(str);
				}
			}


			UiThread.RunOnUiThread(() =>
			{
				_textWidget.Text = stringBuilder.ToString();
				_textWidget.DoExpandBoundsToText();
				_textWidget.Printer.Baseline = Baseline.BoundsTop;

				if (isNeedToScroll || IsTextExpanded == false)
				{
					ScrollPosition = new Vector2(ScrollPosition.X, 0);
				}

				_lastPrintingTimeMs = UiThread.CurrentTimerMs;
				_textWidget.Invalidate();
			});
		}

		private void OnScrollPositionChanged(object? sender, EventArgs e)
		{
			//TODO: Make possibility to clip text area in accordance with scroll view (for performance issue)

			_barInfo.ScrollPosition = ScrollPosition;
		}
	}
}
