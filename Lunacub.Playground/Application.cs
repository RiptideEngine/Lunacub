using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Silk.NET.Windowing;

namespace Lunacub.Playground;

public static class Application {
    public static IWindow MainWindow { get; private set; }
    public static ILogger Logger { get; private set; }
    
    private static void Main(string[] args) {
        using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddSimpleConsole(options => {
            options.IncludeScopes = true;
            options.SingleLine = true;
            options.TimestampFormat = "[HH:mm:ss yyyy/MM/dd] ";
        }));
        Logger = loggerFactory.CreateLogger("Application");
        
        IWindow window = Window.Create(WindowOptions.Default with {
            WindowState = WindowState.Maximized,
            API = GraphicsAPI.None,
        });
        
        window.Load += ApplicationLifecycle.Initialize;
        window.Update += ApplicationLifecycle.Update;
        window.Render += ApplicationLifecycle.Render;
        window.Closing += ApplicationLifecycle.Shutdown;

        MainWindow = window;

        try {
            window.Run();
        } finally {
            window.Dispose();
        }
    }
}