namespace Caxivitual.Lunacub;

public static class ResourceRegistry {
    public readonly record struct Element(string? Name, TagCollection Tags) : IResourceRegistryElement {
        public bool Equals(Element other) {
            if (Name != other.Name) return false;
            if (Tags.Count != other.Tags.Count) return false;

            for (int i = 0; i < Tags.Count; i++) {
                if (Tags[i] != other.Tags[i]) return false;
            }

            return true;
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

    public readonly record struct Element<TOption>(string? Name, TagCollection Tags, TOption Option) : IResourceRegistryElement {
        public bool Equals(Element<TOption> other) {
            if (Name != other.Name) return false;
            if (Tags.Count != other.Tags.Count) return false;

            for (int i = 0; i < Tags.Count; i++) {
                if (Tags[i] != other.Tags[i]) return false;
            }

            return EqualityComparer<TOption>.Default.Equals(Option, other.Option);
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