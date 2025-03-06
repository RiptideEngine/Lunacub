namespace Caxivitual.Lunacub.Building;

internal static class Helpers {
    // ReSharper disable PossibleMultipleEnumeration
  //   public static IEnumerable<T> TopologicalSort<T>(T source, Func<T, IEnumerable<T>?> dependencySelector) where T : notnull {
  //       List<T> result = [];
  //       Dictionary<T, bool> visited = [];
		// Stack<T> stack = [];
  //
  //       Visit(source, dependencySelector, visited, result, stack);
  //       
  //       return result;
  //
  //       static void Visit(T node, Func<T, IEnumerable<T>?> dependencySelector, Dictionary<T, bool> visited, List<T> result, Stack<T> stack) {
  //           if (visited.TryGetValue(node, out bool visiting)) {
  //               if (visiting) {
		// 			var cycle = stack.Reverse().SkipWhile(n => n != node).Append(node);
  //                   throw new ArgumentException($"Cyclic dependency found: {string.Join(" -> ", cycle)}.");
  //               }
  //           } else {
  //               visited[node] = true;
		// 		stack.Push(node);
  //
  //               if (dependencySelector(node) is { } dependencies) {
  //                   foreach (var dependency in dependencies) {
  //                       Visit(source, dependency, dependencySelector, visited, result, stack);
  //                   }
  //               }
  //
  //               visited[node] = false;
		// 		stack.Pop();
  //               result.Add(node);
  //           }
  //       }
  //
  //       static void CollectCyclePath(T node, Func<T, IEnumerable<T>?> dependencySelector, Dictionary<T, bool> visited, List<T> path) {
  //           // visited.Add(node, false);
  //           // path.Add(node);
  //           //
  //           // IEnumerable<T>? dependencies = dependencySelector(node);
  //           //
  //           // if (dependencies == null || dependencies.All(visited.ContainsKey)) return;
  //           //
  //           // foreach (var dependency in dependencies) {
  //           //     if (!visited.ContainsKey(dependency)) {
  //           //         CollectCyclePath(dependency, dependencySelector, visited, path);
  //           //     }
  //           // }
  //           //
  //           // path.RemoveAt(path.Count - 1);
  //           // visited.Remove(node);
  //       }
  //   }
    // ReSharper restore PossibleMultipleEnumeration
}
