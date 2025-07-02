using Silk.NET.Input;
using Silk.NET.Windowing;

namespace Caxivitual.Lunacub.Examples.TextureViewer;

internal sealed class Application {
    public static IWindow MainWindow { get; private set; } = null!;
    public static IInputContext InputContext { get; private set; } = null!;
    
    private static void Main(string[] args) {
        WindowOptions options = WindowOptions.DefaultVulkan with {
            WindowState = WindowState.Maximized,
        };

        MainWindow = Window.Create(options);

        MainWindow.Load += () => {
            InputContext = MainWindow!.CreateInput();
            ApplicationLifecycle.Initialize();
        };
        MainWindow.Update += _ => ApplicationLifecycle.Update();
        MainWindow.Render += _ => ApplicationLifecycle.Render();
        MainWindow.Closing += () => {
            ApplicationLifecycle.Shutdown();
            
            InputContext.Dispose();
            InputContext = null!;
        };
        
        MainWindow.Run();
        MainWindow.Dispose();
        MainWindow = null!;
    }
}