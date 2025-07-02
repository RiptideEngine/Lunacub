using Cysharp.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Lunacub.SourceValidator;

internal static class Program {
    private static async Task<int> Main(string[] args) {
        Dictionary<string, List<InvalidationReport>> fileReports = [];
        
        foreach (var directory in args) {
            if (!Directory.Exists(directory)) continue;

            foreach (var filePath in Directory.EnumerateFiles(directory, "*.cs", SearchOption.AllDirectories)) {
                ReadOnlySpan<char> fileName = Path.GetFileName(filePath.AsSpan());

                if (fileName.EndsWith(".g.cs") || fileName.EndsWith(".generated.cs")) continue;

                await using Utf8TextReader reader = new Utf8StreamReader(filePath).AsTextReader();

                while (await reader.LoadIntoBufferAsync()) {
                    int lineNumber = 0;
                    
                    while (reader.TryReadLine(out var line)) {
                        lineNumber++;
                        
                        if (line.IsEmpty || line.Trim().IsEmpty) continue;

                        if (line.Length >= 130) {
                            ref var reports = ref CollectionsMarshal.GetValueRefOrAddDefault(fileReports, filePath, out bool exists);

                            if (!exists) {
                                reports = [];
                            }
                            
                            reports!.Add(new(lineNumber, "Surpassed maximum characters allowed per line."));
                        }
                    }
                }
            }
        }

        if (fileReports.Count == 0) return 0;

        StringBuilder sb = new StringBuilder();
        foreach ((var filePath, var reports) in fileReports.OrderBy(x => x.Key)) {
            sb.Append(filePath).AppendLine(":");

            for (int i = 0; i < reports.Count; i++) {
                var report = reports[i];
                
                sb.Append("- ").Append(i).Append(". Line ").Append(report.Line).Append(": ").Append(report.Message).AppendLine();
            }
        }
        
        Console.WriteLine(sb.ToString());
        return 1;
    }

    private readonly record struct InvalidationReport(int Line, string Message);
}