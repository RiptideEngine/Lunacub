namespace Caxivitual.Lunacub.Tests.Importing;

public class ResourceLibraryCollectionTests {
    private readonly ResourceLibraryCollection _collection = [];
    
    [Theory]
    [InlineData("c9cecd943b5558929de0403e22d3ad3c")]
    [InlineData("146dfb18e0195452a5bdd849a9a9a594")]
    public void Add_ShouldBeCorrect(string guid) {
        new Action(() => _collection.Add(new FakeResourceLibrary(new(guid)))).Should().NotThrow();

        var library = _collection.Should().ContainSingle().Which;
        library.Id.Should().Be(new Guid(guid));
        library.Should().BeOfType<FakeResourceLibrary>();
    }
    
    [Fact]
    public void Remove_ShouldBeCorrect() {
        Guid guid1 = Guid.Parse("c9cecd943b5558929de0403e22d3ad3c");
        Guid guid2 = Guid.Parse("146dfb18e0195452a5bdd849a9a9a594");
        
        _collection.Add(new FakeResourceLibrary(guid1));
        _collection.Add(new FakeResourceLibrary(guid2));

        _collection.Remove(guid1).Should().BeTrue();
        _collection.Should().HaveCount(1);
        _collection[0].Id.Should().Be(guid2);

        _collection.Remove(Guid.NewGuid()).Should().BeFalse();
        _collection.Should().HaveCount(1);
        
        _collection.Remove(_collection[0]).Should().BeTrue();
        _collection.Should().BeEmpty();
    }
    
    [Fact]
    public void Add_DuplicateId_ShouldThrowException() {
        Guid guid = Guid.Parse("c9cecd943b5558929de0403e22d3ad3c");
        new Action(() => _collection.Add(new FakeResourceLibrary(guid))).Should().NotThrow();
        new Action(() => _collection.Add(new FakeResourceLibrary(guid))).Should().Throw<ArgumentException>("*already*");
    }

    private sealed class FakeResourceLibrary : ResourceLibrary {
        public FakeResourceLibrary(Guid guid) : base(guid) { }

        public override bool Contains(ResourceID rid) => false;
        protected override Stream? CreateStreamImpl(ResourceID rid) => null;

        public override IEnumerator<ResourceID> GetEnumerator() => Enumerable.Empty<ResourceID>().GetEnumerator();
    }
}