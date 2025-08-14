namespace Caxivitual.Lunacub;

partial struct TagCollection {
    public static Builder CreateBuilder(int capacity = 4) => new(capacity);
    
    public sealed class Builder : IList<string>, IReadOnlyList<string> {
        private readonly List<string> _list;
        
        public int Count => _list.Count;

        bool ICollection<string>.IsReadOnly => false;

        internal Builder(int capacity = 4) {
            _list = new(capacity);
        }

        public void Add(string tag) {
            ValidateTag(tag);
            
            _list.Add(tag);
        }

        public void Insert(int index, string tag) {
            ValidateTag(tag);
            
            _list.Insert(index, tag);
        }
        
        public bool Remove(string tag) => _list.Remove(tag);
        
        public void RemoveAt(int index) => _list.RemoveAt(index);
        
        public void Clear() => _list.Clear();
        
        public bool Contains(string tag) => _list.Contains(tag);
        
        public void CopyTo(string[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);
        
        public int IndexOf(string tag) => _list.IndexOf(tag);

        public string this[int index] {
            get => _list[index];
            set {
                ValidateTag(value);
                
                _list[index] = value;
            }
        }
        
        public List<string>.Enumerator GetEnumerator() => _list.GetEnumerator();
        
        IEnumerator<string> IEnumerable<string>.GetEnumerator() => ((IEnumerable<string>)_list).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_list).GetEnumerator();

        public TagCollection ToCollection() {
            return new([..CollectionsMarshal.AsSpan(_list)[.._list.Count]]);
        }
    }
}