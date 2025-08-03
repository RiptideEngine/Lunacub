using System.Collections.Immutable;

namespace Caxivitual.Lunacub;

public static class ResourceRegistry {
    public readonly record struct Element(string? Name, ImmutableArray<string> Tags) : IResourceRegistryElement {
        public bool Equals(Element other) {
            return Name == other.Name && Tags.SequenceEqual(other.Tags);
        }

        [ExcludeFromCodeCoverage]
        public override int GetHashCode() {
            HashCode hc = new HashCode();
            
            hc.Add(Name);

            foreach (var tag in Tags) {
                hc.Add(tag);
            }

            return hc.ToHashCode();
        }
    }

    public readonly record struct Element<TOption>(string? Name, ImmutableArray<string> Tags, TOption Option) : IResourceRegistryElement {
        public bool Equals(Element<TOption> other) {
            return Name == other.Name && Tags.SequenceEqual(other.Tags) && EqualityComparer<TOption>.Default.Equals(Option, other.Option);
        }

        [ExcludeFromCodeCoverage]
        public override int GetHashCode() {
            HashCode hc = new HashCode();
            
            hc.Add(Name);

            foreach (var tag in Tags) {
                hc.Add(tag);
            }
            
            hc.Add(Option);

            return hc.ToHashCode();
        }
    }
}