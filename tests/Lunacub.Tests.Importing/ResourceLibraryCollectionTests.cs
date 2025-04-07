namespace Caxivitual.Lunacub.Tests.Importing;

public class ResourceLibraryCollectionTests {
    [Fact]
    public void Add_ShouldBeCorrect() {
        ResourceLibraryCollection collection = [];
        Guid guid1 = Guid.Parse("c9cecd943b5558929de0403e22d3ad3c");
        Guid guid2 = Guid.Parse("146dfb18e0195452a5bdd849a9a9a594");
        new Action(() => collection.Add(new FakeResourceLibrary(guid1))).Should().NotThrow();
        new Action(() => collection.Add(new FakeResourceLibrary(guid2))).Should().NotThrow();

        collection.Should().HaveCount(2);
        collection[0].Id.Should().Be(guid1);
        collection[0].Should().BeOfType<FakeResourceLibrary>();
        
        collection.Should().HaveCount(2);
        collection[1].Id.Should().Be(guid2);
        collection[1].Should().BeOfType<FakeResourceLibrary>();
    }
    
    [Fact]
    public void Remove_ShouldBeCorrect() {
        Guid guid1 = Guid.Parse("c9cecd943b5558929de0403e22d3ad3c");
        Guid guid2 = Guid.Parse("146dfb18e0195452a5bdd849a9a9a594");
        
        ResourceLibraryCollection collection = [
            new FakeResourceLibrary(guid1),
            new FakeResourceLibrary(guid2),
        ];

        collection.Remove(guid1).Should().BeTrue();
        collection.Should().HaveCount(1);
        collection[0].Id.Should().Be(guid2);

        collection.Remove(Guid.NewGuid()).Should().BeFalse();
        collection.Should().HaveCount(1);
        
        collection.Remove(collection[0]).Should().BeTrue();
        collection.Should().BeEmpty();
    }
    
    [Fact]
    public void Add_DuplicateId_ShouldThrowException() {
        ResourceLibraryCollection collection = [];
        Guid guid = Guid.Parse("c9cecd943b5558929de0403e22d3ad3c");
        new Action(() => collection.Add(new FakeResourceLibrary(guid))).Should().NotThrow();
        new Action(() => collection.Add(new FakeResourceLibrary(guid))).Should().Throw<ArgumentException>("*already*");
    }

    private sealed class FakeResourceLibrary : ResourceLibrary {
        public FakeResourceLibrary(Guid guid) : base(guid) { }

        public override bool Contains(ResourceID rid) => false;
        protected override Stream? CreateStreamImpl(ResourceID rid) => null;
    }
}