using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MatterHackers.Agg.UI;
using MatterHackers.VectorMath;

namespace iLeif.CustomAggWidgets.Widgets
{
	public class CustomSliderWidget : GuiWidget
	{
		public event EventHandler? ValueChanged;
		public double Value { get => _slider.Value; set => _slider.Value = value; }
		public int DigitsForRound { get; set; }
		public int Lenght { get => _lenght; set { _lenght = value; Invalidate(); } }
		private int _lenght;

		private int _width;

		public double Minimum { get => _minimum; set => SetMinMax(value, _maximum); }
		private double _minimum;

		public double Maximum { get => _maximum; set => SetMinMax(_minimum, value); }
		private double _maximum;

		public Orientation Orientation { get => _orientation; set => SetOrientation(value); }
		private Orientation _orientation;

		public override string Text { get => _text; set => SetText(value); }
		private string _text;

		protected int _textBoxMaxSize = 30;
		protected int _fontSize = 12;

		protected Slider _slider;

		protected TextWidget _textWidget;
		protected TextWidget _valueWidget;

		public CustomSliderWidget(
			double min = 0, double max = 1,
			int width = 30, int lenght = 200,
			string text = "", int digitsForRound = 3,
			Orientation orientation = Orientation.Vertical)
		{
			MinimumSize = (orientation == Orientation.Horizontal) ? new Vector2(lenght, width) : new Vector2(width, lenght);
			DigitsForRound = digitsForRound;

			if (!string.IsNullOrEmpty(text))
			{
				Name = text;
			}

			_lenght = lenght;
			_width = width;
			_minimum = min;
			_maximum = max;
			_text = text;
			_orientation = orientation;
		}

		public override void Initialize()
		{
			if (Initialized)
			{
				return;
			}

			RemoveChildren();

			var length = _orientation == Orientation.Horizontal ? Width : Height;

			_slider = new Slider(new Vector2(0, 0), _lenght - (_fontSize * 4), orientation: _orientation);
			_slider.Minimum = _minimum;
			_slider.Maximum = _maximum;

			_slider.ThumbHeight = 17;
			_slider.ThumbWidth = 17;
			_slider.View.TrackColor = new MatterHackers.Agg.Color(30, 30, 30);
			_slider.View.ThumbColor = new MatterHackers.Agg.Color(240, 240, 240);

			var flowDirection = _orientation == Orientation.Horizontal ? FlowDirection.LeftToRight : FlowDirection.TopToBottom;
			//var textContainer = new LayoutLayer(flowDirection);

			var textContainer = new GuiWidget();
			_textWidget = new TextWidget(_text, _textBoxMaxSize, _textBoxMaxSize);
			_textWidget.AutoExpandBoundsToText = true;
			_textWidget.Text = _text;

			_valueWidget = new TextWidget(_text, _textBoxMaxSize, _textBoxMaxSize);
			_valueWidget.AutoExpandBoundsToText = true;
			_valueWidget.Text = _minimum.ToString();

			if (false)
			{
				ShowDebugInfo();

				textContainer.DebugShowBounds = true;
			}



			if (_orientation == Orientation.Horizontal)
			{
				_textWidget.MaximumSize = new Vector2(_textBoxMaxSize, _textBoxMaxSize);
				_valueWidget.MaximumSize = new Vector2(_textBoxMaxSize, _textBoxMaxSize);
				textContainer.Height = _fontSize;

				textContainer.VAnchor = VAnchor.Bottom;
				textContainer.HAnchor = HAnchor.Center;
				_slider.VAnchor = VAnchor.Top;
				_slider.HAnchor = HAnchor.Center;

				_textWidget.VAnchor = VAnchor.Center;
				_textWidget.HAnchor = HAnchor.Left;

				_valueWidget.VAnchor = VAnchor.Center;
				_valueWidget.HAnchor = HAnchor.Right;

				_valueWidget.Margin = new MatterHackers.Agg.BorderDouble(3, 0, 0, 0);

				_textWidget.Printer.Justification = MatterHackers.Agg.Font.Justification.Left;
				_valueWidget.Printer.Justification = MatterHackers.Agg.Font.Justification.Left;
			}
			else
			{
				textContainer.HAnchor = HAnchor.Stretch;
				textContainer.VAnchor = VAnchor.Bottom;
				textContainer.Height = _fontSize * 3.5;

				_slider.HAnchor = HAnchor.Center;
				_slider.VAnchor = VAnchor.Top;

				_textWidget.HAnchor = HAnchor.Center;
				_textWidget.VAnchor = VAnchor.Top;

				_valueWidget.HAnchor = HAnchor.Center;
				//_valueWidget.Margin = new MatterHackers.Agg.BorderDouble(0, 0, 0, 3);
				_valueWidget.VAnchor = VAnchor.Bottom;

				_textWidget.Printer.Justification = MatterHackers.Agg.Font.Justification.Center;
				_valueWidget.Printer.Justification = MatterHackers.Agg.Font.Justification.Center;
			}

			_valueWidget.DoExpandBoundsToText();
			_textWidget.DoExpandBoundsToText();

			_slider.ValueChanged += OnSliderValueChanged;

			using (LayoutLock())
			{
				this.AddChild(_slider);

				textContainer.AddChild(_textWidget);
				textContainer.AddChild(_valueWidget);

				this.AddChild(textContainer);
			}

			SetBoundsToEncloseChildren();
			textContainer.SetBoundsToEncloseChildren();
			//textContainer.SetBoundsRelativeToParent(new MatterHackers.Agg.RectangleInt(0, 0, _width, _textBoxMaxSize * 2));
			base.Initialize();
		}

		protected void OnSliderValueChanged(object? sender, EventArgs args)
		{
			if (DigitsForRound < 0)
			{
				Value = _slider.Value;
			}
			else
			{
				//var rounded = _slider.Value * (10 * DigitsForRound);
				Value = Math.Round(_slider.Value, DigitsForRound);
			}

			ValueChanged?.Invoke(sender, args);

			if (_valueWidget != null)
			{
				_valueWidget.Text = Value.ToString();
			}
		}

		private void ShowDebugInfo()
		{
			DebugShowBounds = true;
			_slider.DebugShowBounds = true;
			_textWidget.DebugShowBounds = true;
			_valueWidget.DebugShowBounds = true;
		}

		private void SetText(string text)
		{
			_text = text;
			_textWidget.Text = text;
		}

		private void SetMinMax(double min, double max)
		{
			if (min >= max)
			{
				return;
			}

			if (_minimum != min)
			{
				_minimum = min;
				_slider.Minimum = min;
			}

			if (_maximum != max)
			{
				_maximum = max;
				_slider.Maximum = max;
			}

			if (Value > max)
			{
				_slider.Value = max;
			}

			if (Value < min)
			{
				_slider.Value = min;
			}

			Invalidate();
		}

		private void SetOrientation(Orientation orientation)
		{
			_orientation = orientation;
			_slider.Orientation = orientation;
		}

		protected void OnValueChange()
		{

		}
	}
}
