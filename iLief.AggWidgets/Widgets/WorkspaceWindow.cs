using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using iLeif.CustomAggWidgets;
using iLeif.CustomAggWidgets.Commands;
using iLeif.CustomAggWidgets.Widgets;
using iLeif.CustomAggWidgets.Widgets.Interfaces;
using iLeif.Extensions.Logging;
using MatterHackers.Agg;
using MatterHackers.Agg.Platform;
using MatterHackers.Agg.UI;
using MatterHackers.DataConverters3D;

namespace iLeif.CustomAggWidgets.Widgets
{
    public class WorkspaceWindow : IOutputHolder, IWorkspaceHolder, IWindow, IReinitializable
	{
		public event EventHandler? ReinitializationBegin;
		public event EventHandler? ReinitializationEnd;

		public bool IsNeedShowReinitButton { get; set; }
		public ILogger Logger { get; }

		public bool Initialized { get; private set; } = false;

		protected SystemWindow _systemWindow;
		protected GuiWidget? _workspace;
		protected ScrollableTextWidget _textOutput;
		protected HashSet<IWorkspaceItem> _workspaceItems;

		private TextWriterLogger _textWriterLogger;

		public WorkspaceWindow(LoggerAggregator logger, bool isNeedShowReinitButton, bool forceGl = true)
		{
			_textWriterLogger = new TextWriterLogger(null);
			_workspaceItems = new HashSet<IWorkspaceItem>();
			IsNeedShowReinitButton = isNeedShowReinitButton;
			logger.AddLoggers(_textWriterLogger);
			Logger = logger;

			if (forceGl)
			{
				InitGl();
			}

			Initialize();
		}

		public void InitGl()
		{
			// Force OpenGL
			var Glfw = false;
			if (Glfw)
			{
				AggContext.Config.ProviderTypes.SystemWindowProvider = "MatterHackers.GlfwProvider.GlfwWindowProvider, MatterHackers.GlfwProvider";
			}
			else
			{
				AggContext.Config.ProviderTypes.SystemWindowProvider = "MatterHackers.Agg.UI.OpenGLWinformsWindowProvider, agg_platform_win32";
			}
		}

		public void Initialize()
		{
			if (Initialized)
			{
				return;
			}

			_systemWindow = new SystemWindow(Defaults.MainWindowSizeX, Defaults.MainWindowSizeY);
			_systemWindow.AnchorAll();

			_textOutput = new ScrollableTextWidget("MainOutputBar", 35);
			_textOutput.AnchorBottom();
			_textWriterLogger.SetWriter(_textOutput);
			_systemWindow.AddChild(_textOutput);

			_workspace = new GuiWidget();
			_workspace.VAnchor = VAnchor.Top;
			_workspace.HAnchor = HAnchor.Stretch;
			_workspace.Height = _systemWindow.Height - _textOutput.Height;
			_systemWindow.AddChild(_workspace);

			_systemWindow.SizeChanged += OnResize;

			if (IsNeedShowReinitButton)
			{
				var buttonsLayer = new LayoutLayer(FlowDirection.RightToLeft)
				{
					HAnchor = HAnchor.Right,
					VAnchor = VAnchor.Top
				};

				var reinitButton = new CommandButton("Reinit", new ReinitializeWidgetCommand(this));
				buttonsLayer.AddChild(reinitButton);

				_systemWindow.AddChild(buttonsLayer);

				buttonsLayer.SetBoundsToEncloseChildren();
			}

			_systemWindow.AnchorAll();

			_systemWindow.Initialize();

			Initialized = true;
		}

		public void ClearOutput()
		{
			_textOutput?.ClearBuffer();
		}

		public void Reinitializing()
		{
			ReinitializationBegin?.Invoke(this, new EventArgs());

			foreach (var wItem in _workspaceItems)
			{
				wItem.Widget?.Invalidate();
				wItem.Reinitializing();
			}

			_workspace?.Invalidate();

			ReinitializationEnd?.Invoke(this, new EventArgs());
		}

		public void ShowAsSystemWindow()
		{
			_systemWindow.ShowAsSystemWindow();
		}

		public void AddToWorkspace(params IWorkspaceItem[] wItems)
		{
			if (!Initialized)
			{
				Initialize();
			}

			string message;

			if (_workspace == null)
			{
				message = "Workspace is null";
				_textWriterLogger.Error(message);
				throw new Exception(message);
			}

			int invalidWidgets = 0;
			foreach (var wItem in wItems)
			{
				if (_workspaceItems.Contains(wItem))
				{
					invalidWidgets++;
					continue;
				}
				_workspaceItems.Add(wItem);

				if (wItem == null)
				{
					invalidWidgets++;
					continue;
				}

				if (wItem.Initialized == false)
				{
					wItem.Initialize();
				}

				_workspace?.AddChild(wItem.Widget);
			}

			if (invalidWidgets > 0)
			{
				message = $"Cant load widgets: {invalidWidgets}";
				_textWriterLogger.Error(message);
			}


			_workspace.Invalidate();
			//foreach (var widget in widgets)
			//{
			//	Exception exception = null;

			//	if (widget == null)
			//	{
			//		exception = new Exception("Widget is null");
			//	}
			//	else if (string.IsNullOrEmpty(widget.Name))
			//	{
			//		exception = new Exception("Invalid widget`s name");
			//	}
			//	else if (_widgets.ContainsKey(widget.Name))
			//	{
			//		exception = new Exception($"Widget with '{widget.Name}' already exist");
			//	}

			//	if (exception != null)
			//	{
			//		_textWriterLogger.Error(exception.Message);
			//		throw exception;
			//	}

			//	_widgets.Add(widget.Name, widget);
			//}
		}

		public void RemoveFromWorkspace(IWorkspaceItem wItem)
		{
			_workspaceItems.Remove(wItem);
			_workspace?.RemoveChild(wItem?.Widget);

			_workspace?.Invalidate();
		}

		public void RemoveFromWorkspace()
		{
			_workspace?.RemoveChildren();
			_workspaceItems = new HashSet<IWorkspaceItem>();

			_workspace?.Invalidate();
		}

		protected virtual void OnResize(object sender, EventArgs args)
		{
			if (_workspace == null || _systemWindow == null)
			{
				var message = "Can`t resize";
				Logger.Error(message);
				throw new Exception(message);
			}

			double newHeight = _systemWindow.Height;

			if (_textOutput != null)
			{
				newHeight -= _textOutput.Height;
			}

			_workspace.Height = newHeight;
		}
	}
}
