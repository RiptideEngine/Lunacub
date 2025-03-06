namespace Caxivitual.Lunacub.Building;

public static class TopologicalHelper {
    public static SortResult<T> TopologicalSort<T>(this T source, Func<T, IEnumerable<T>> outgoingSelector, IEqualityComparer<T>? comparer = null) {
        HashSet<T> visited = new(comparer);         // Temporary Mark
        HashSet<T> permark = new(comparer);         // Permanent Mark
        Stack<T> path = [];
        List<T> sorted = [];

        if (TopologicalSortInner(source, outgoingSelector, visited, permark, path, sorted)) {
            return new(SortStatus.Cyclic, path.Reverse());
        } else {
            return new(SortStatus.Success, sorted);
        }
    }
    
    public static SortResult<T> TopologicalSort<T>(this IEnumerable<T> graphNodes, Func<T, IEnumerable<T>> outgoingSelector, IEqualityComparer<T>? comparer = null) {
        HashSet<T> visited = new(comparer);         // Temporary Mark
        HashSet<T> permark = new(comparer);         // Permanent Mark
        Stack<T> path = [];
        List<T> sorted = [];

        foreach (var node in graphNodes) {
            if (!visited.Contains(node)) {
                if (TopologicalSortInner(node, outgoingSelector, visited, permark, path, sorted)) {
                    return new(SortStatus.Cyclic, path.Reverse());
                }
            }
        }

        return new(SortStatus.Success, sorted);
    }

    private static bool TopologicalSortInner<T>(T node, Func<T, IEnumerable<T>> outgoingSelector, HashSet<T> tempmark, HashSet<T> permark, Stack<T> path, List<T> sorted) {
        if (permark.Contains(node)) return false;

        if (!tempmark.Add(node)) {
            path.Push(node);
            return true;
        }
        
        path.Push(node);
        
        if (outgoingSelector(node) is { } outgoings) {
            foreach (var outgoing in outgoings) {
                if (TopologicalSortInner(outgoing, outgoingSelector, tempmark, permark, path, sorted)) return true;
            }
        }

        permark.Add(node);
        sorted.Add(node);
        path.Pop();

        return false;
    }

    public enum SortStatus {
        Success,
        Cyclic,
    }

    public readonly record struct SortResult<T>(SortStatus Status, IEnumerable<T> Collection);
}