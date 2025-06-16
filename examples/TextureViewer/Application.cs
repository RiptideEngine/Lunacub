using Silk.NET.Windowing;

namespace Caxivitual.Lunacub.Examples.TextureViewer;

internal static class Application {
    private static IWindow _window = null!;
    public static IWindow MainWindow => _window;
    
    private static void Main(string[] args) {
        WindowOptions options = WindowOptions.DefaultVulkan with {
            WindowState = WindowState.Maximized,
        };

        _window = Window.Create(options);

        _window.Load += ApplicationLifecycle.Initialize;
        _window.Update += _ => ApplicationLifecycle.Update();
        _window.Render += _ => ApplicationLifecycle.Render();
        _window.Closing += ApplicationLifecycle.Shutdown;
        
        _window.Run();
        _window.Dispose();
    }
}