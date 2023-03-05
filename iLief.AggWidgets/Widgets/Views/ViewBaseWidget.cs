using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using iLeif.CustomAggWidgets.Commands;
using iLeif.Extensions.Logging;
using MatterHackers.Agg;
using MatterHackers.Agg.Transform;
using MatterHackers.Agg.UI;
using MatterHackers.RenderOpenGl.OpenGl;

namespace iLeif.CustomAggWidgets.Widgets.Views
{
    public abstract class ViewBaseWidget : IView
    {
        public int FPS { get => FrequenceMs * 1000; set => FrequenceMs = 1000 / value; }
        public int FrequenceMs { get; private set; }
        private long _lastDrawMs = 0;

        public bool Initialized { get; protected set; } = false;

        public virtual GuiWidget? UiLayer { get; protected set; }
        public virtual GuiWidget DrawLayer { get; protected set; }
        public virtual StatusBar StatusBar { get; protected set; }
        public virtual GuiWidget Widget { get; protected set; }
        public Action<Graphics2D>? ActionOnDraw { get; set; }
        public bool IsFitContentIntoView { get; set; } = true;

		protected ILogger _logger;

        protected bool _clearFlag = false;

        private bool _isDrawActionSuspend = false;

        private ViewBaseWidget(ILogger logger)
        {
            _logger = logger;

            FPS = 60;
			Widget = new GuiWidget();
            DrawLayer = new GuiWidget();
            StatusBar = new StatusBar(FlowDirection.RightToLeft, 250, 25);
            DrawLayer.BeforeDraw += OnDrawHandler;

            Widget.AnchorAll();
            DrawLayer.AnchorAll();
        }

        public ViewBaseWidget(bool createButtons, ILogger logger) : this(logger)
        {
            if (createButtons)
            {
                CreateButtons(FlowDirection.TopToBottom, VAnchor.Center, HAnchor.Left);
            }
        }

        public ViewBaseWidget(FlowDirection flowDirection, VAnchor vAnchor, HAnchor hAnchor, ILogger logger) : this(logger)
        {
            CreateButtons(flowDirection, vAnchor, hAnchor);
        }

        public virtual void Initialize()
        {
            if (Initialized == true)
            {
                return;
            }

            StatusBar.HAnchor = HAnchor.Right;
            StatusBar.VAnchor = VAnchor.Bottom;
            StatusBar.Margin = new BorderDouble(5, 5, 0, 0);
            StatusBar.Padding = new BorderDouble(3, 1);

			Widget.Initialize();
            Widget.AddChild(DrawLayer);
            Widget.AddChild(UiLayer);
            Widget.AddChild(StatusBar);
            Initialized = true;
        }

        protected void CreateButtons(FlowDirection flowDirection, VAnchor vAnchor, HAnchor hAnchor)
        {
            UiLayer = new FlowLayoutWidget(flowDirection);
            UiLayer.HAnchor = hAnchor;
            UiLayer.VAnchor = vAnchor;

            var resetViewButton = new CommandButton("Reset\nView", new ResetViewCommand(this));
            UiLayer.AddChild(resetViewButton);

            UiLayer.SetBoundsToEncloseChildren();
        }

        private void OnDrawHandler(object? sender, DrawEventArgs args)
        {
            OnDraw(args.Graphics2D);
            return;

            var deltaDraw = UiThread.CurrentTimerMs - _lastDrawMs;

            if (deltaDraw >= FrequenceMs)
            {
                //TODO: Here we need render new frame
            }
            //TODO: Here we need render buffered frame
            _lastDrawMs = UiThread.CurrentTimerMs;
        }

        public virtual void OnDraw(Graphics2D graphics2D)
        {
            if (!Initialized)
            {
                Initialize();
            }

            if (_clearFlag)
            {
                _clearFlag = false;
                graphics2D.Clear(Color.Transparent);
                return;
            }

            if (_isDrawActionSuspend == true)
            {
                _isDrawActionSuspend = false;
                return;
            }

            ActionOnDraw?.Invoke(graphics2D);
        }

        protected abstract void FitIntoView();

        public virtual void ClearScene()
        {
            _clearFlag = true;

            Widget.Invalidate();
        }

        public virtual void Dispose()
        {
            Widget.BeforeDraw -= OnDrawHandler;
        }

        public virtual void ResetView()
        {
			if (IsFitContentIntoView)
			{
				FitIntoView();
			}

            DrawLayer?.Invalidate();
		}

        protected void SuspendBaseDrawActionForNextDraw()
        {
            _isDrawActionSuspend = true;
        }
    }
}
