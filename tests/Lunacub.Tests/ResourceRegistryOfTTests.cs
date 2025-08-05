// ReSharper disable CollectionNeverQueried.Local

namespace Caxivitual.Lunacub.Tests;

public class ResourceRegistryOfTTests {
    private readonly ResourceRegistry<ResourceRegistry.Element> _registry = [];
    
    [Fact]
    public void Add_Normal_InsertsCorrectly() {
        new Action(() => _registry.Add(1, new("A", []))).Should().NotThrow();
        new Action(() => _registry.Add(2, new("B", []))).Should().NotThrow();

        _registry.Should().BeEquivalentTo([
            KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(1, new("A", [])),
            KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(2, new("B", [])),
        ]);

        _registry.NameMap.Should().BeEquivalentTo([
            KeyValuePair.Create<string, ResourceID>("A", 1),
            KeyValuePair.Create<string, ResourceID>("B", 2),
        ]);
    }

    [Fact]
    public void Add_DuplicateId_ThrowsArgumentExceptionReportIdAlreadyRegistered() {
        new Action(() => _registry.Add(1, new("A", []))).Should().NotThrow();
        new Action(() => _registry.Add(1, new("B", []))).Should().Throw<ArgumentException>().WithMessage("*id*already*registered*");
    }

    [Fact]
    public void Add_DuplicateName_ThrowsArgumentExceptionReportNameAlreadyRegistered() {
        new Action(() => _registry.Add(1, new("A", []))).Should().NotThrow();
        new Action(() => _registry.Add(2, new("A", []))).Should().Throw<ArgumentException>().WithMessage("*name*already*registered*");
    }

    [Fact]
    public void Add_NullName_InsertsElementAndNotRegisterNameMap() {
        new Action(() => _registry.Add(1, new(null!, []))).Should().NotThrow();
        
        _registry.Should().BeEquivalentTo([
            KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(1, new(null, [])),
        ]);

        _registry.NameMap.Should().BeEmpty();
    }
    
    [Fact]
    public void Add_EmptyName_InsertsElementAndNotRegisterNameMap() {
        new Action(() => _registry.Add(1, new(string.Empty, []))).Should().NotThrow();
        
        _registry.Should().BeEquivalentTo([
            KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(1, new(string.Empty, [])),
        ]);

        _registry.NameMap.Should().BeEmpty();
    }
    
    [Fact]
    public void Add_NullTagCollection_ThrowsNullReferenceException() {
        new Action(() => _registry.Add(1, new("A", default))).Should().Throw<NullReferenceException>();
    }
    
    [Fact]
    public void Add_NullTag_ThrowsArgumentExceptionReportNullTag() {
        new Action(() => _registry.Add(1, new("A", [
            null!,
        ]))).Should().Throw<ArgumentException>().WithMessage("*tag*null*");
    }
    
    [Fact]
    public void Add_EmptyTag_ThrowsArgumentExceptionReportEmptyTag() {
        new Action(() => _registry.Add(1, new("A", [
            null!,
        ]))).Should().Throw<ArgumentException>().WithMessage("*tag*empty*");
    }
    
    [Fact]
    public void Add_InvalidTagCharacter_ThrowsArgumentExceptionReportInvalidTagCharacter() {
        new Action(() => _registry.Add(1, new("A", [
            "Something^Invalid",
        ]))).Should().Throw<ArgumentException>().WithMessage("*tag*invalid*character*");
    }
    
    [Fact]
    public void ICollectionAdd_Normal_InsertsCorrectly() {
        ICollection<KeyValuePair<ResourceID, ResourceRegistry.Element>> registry = _registry;
        
        new Action(() => registry.Add(KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(1, new("A", [])))).Should().NotThrow();
        new Action(() => registry.Add(KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(2, new("B", [])))).Should().NotThrow();

        _registry.Should().BeEquivalentTo([
            KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(1, new("A", [])),
            KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(2, new("B", [])),
        ]);

        _registry.NameMap.Should().BeEquivalentTo([
            KeyValuePair.Create<string, ResourceID>("A", 1),
            KeyValuePair.Create<string, ResourceID>("B", 2),
        ]);
    }

    [Fact]
    public void InterfaceAdd_DuplicateId_ThrowsArgumentExceptionReportIdAlreadyRegistered() {
        ICollection<KeyValuePair<ResourceID, ResourceRegistry.Element>> registry = _registry;
        
        new Action(() => {
            registry.Add(KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(1, new("A", [])));
        }).Should().NotThrow();
        
        new Action(() => {
            registry.Add(KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(1, new("B", [])));
        }).Should().Throw<ArgumentException>().WithMessage("*id*already*registered*");
    }

    [Fact]
    public void InterfaceAdd_DuplicateName_ThrowsArgumentExceptionReportNameAlreadyRegistered() {
        ICollection<KeyValuePair<ResourceID, ResourceRegistry.Element>> registry = _registry;
        
        new Action(() => {
            registry.Add(KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(1, new("A", [])));
        }).Should().NotThrow();
        
        new Action(() => {
            registry.Add(KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(2, new("A", [])));
        }).Should().Throw<ArgumentException>().WithMessage("*name*already*registered*");
    }

    [Fact]
    public void InterfaceAdd_NullName_InsertsElementAndNotRegisterNameMap() {
        ICollection<KeyValuePair<ResourceID, ResourceRegistry.Element>> registry = _registry;

        new Action(() => {
            registry.Add(KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(1, new(null!, [])));
        }).Should().NotThrow();

        _registry.Should().BeEquivalentTo([
            KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(1, new(null!, [])),
        ]);
        
        _registry.NameMap.Should().BeEmpty();
    }
    
    [Fact]
    public void InterfaceAdd_EmptyName_InsertsElementAndNotRegisterNameMap() {
        ICollection<KeyValuePair<ResourceID, ResourceRegistry.Element>> registry = _registry;

        new Action(() => {
            registry.Add(KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(1, new(string.Empty, [])));
        }).Should().NotThrow();
        
        _registry.Should().BeEquivalentTo([
            KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(1, new(string.Empty, [])),
        ]);
        
        _registry.NameMap.Should().BeEmpty();
    }
    
    [Fact]
    public void InterfaceAdd_NullTagCollection_ThrowsNullReferenceException() {
        ICollection<KeyValuePair<ResourceID, ResourceRegistry.Element>> registry = _registry;
        
        new Action(() => {
            registry.Add(KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(1, new("A", default)));
        }).Should().Throw<NullReferenceException>();
    }
    
    [Fact]
    public void InterfaceAdd_NullTag_ThrowsArgumentExceptionReportNullTag() {
        ICollection<KeyValuePair<ResourceID, ResourceRegistry.Element>> registry = _registry;
        
        new Action(() => {
            registry.Add(KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(1, new("A", [null!])));
        }).Should().Throw<ArgumentException>().WithMessage("*tag*null*");
    }
    
    [Fact]
    public void InterfaceAdd_EmptyTag_ThrowsArgumentExceptionReportEmptyTag() {
        ICollection<KeyValuePair<ResourceID, ResourceRegistry.Element>> registry = _registry;
        
        new Action(() => {
            registry.Add(KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(1, new("A", [string.Empty])));
        }).Should().Throw<ArgumentException>().WithMessage("*tag*empty*");
    }
    
    [Fact]
    public void InterfaceAdd_InvalidTagCharacter_ThrowsArgumentExceptionReportInvalidTagCharacter() {
        ICollection<KeyValuePair<ResourceID, ResourceRegistry.Element>> registry = _registry;
        
        new Action(() => {
            registry.Add(KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(1, new("A", ["Something&Invalid"])));
        }).Should().Throw<ArgumentException>().WithMessage("*tag*invalid*character*");
    }

    [Fact]
    public void Count_Normal_ChangesCorrectly() {
        _registry.Should().HaveCount(0);
        _registry.Add(1, new("A", []));
        _registry.Should().HaveCount(1);
    }

    [Fact]
    public void Count_DuplicateId_NotChanged() {
        _registry.Should().HaveCount(0);
        _registry.Add(1, new("A", []));
        new Action(() => _registry.Add(1, new("B", []))).Should().Throw<ArgumentException>();
        _registry.Should().HaveCount(1);
    }
    
    [Fact]
    public void Count_DuplicateName_NotChanged() {
        _registry.Should().HaveCount(0);
        _registry.Add(1, new("A", []));
        new Action(() => _registry.Add(2, new("A", []))).Should().Throw<ArgumentException>();
        _registry.Should().HaveCount(1);
    }

    [Fact]
    public void RemoveFromId_Contains_RemovesCorrectly() {
        _registry.Add(1, new("A", []));
        _registry.Add(2, new("B", []));
        _registry.Add(3, new("C", []));

        _registry.Remove(2).Should().BeTrue();

        _registry.Should().BeEquivalentTo([
            KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(1, new("A", [])),
            KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(3, new("C", [])),
        ]);
        
        _registry.NameMap.Should().BeEquivalentTo([
            KeyValuePair.Create<string, ResourceID>("A", 1),
            KeyValuePair.Create<string, ResourceID>("C", 3),
        ]);
    }
    
    [Fact]
    public void RemoveFromId_Contains_RemovesCorrectlyAndReturnsCorrectObject() {
        _registry.Add(1, new("A", []));
        _registry.Add(2, new("B", []));
        _registry.Add(3, new("C", []));

        _registry.Remove(2, out var removed).Should().BeTrue();
        removed.Should().Be(new ResourceRegistry.Element("B", []));
        
        _registry.Should().BeEquivalentTo([
            KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(1, new("A", [])),
            KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(3, new("C", [])),
        ]);
        
        _registry.NameMap.Should().BeEquivalentTo([
            KeyValuePair.Create<string, ResourceID>("A", 1),
            KeyValuePair.Create<string, ResourceID>("C", 3),
        ]);
    }

    [Fact]
    public void RemoveFromId_NotContains_DoesNothing() {
        _registry.Add(1, new("A", []));
        _registry.Add(2, new("B", []));
        
        _registry.Remove(3).Should().BeFalse();
        
        _registry.Should().BeEquivalentTo([
            KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(1, new("A", [])),
            KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(2, new("B", [])),
        ]);
        
        _registry.NameMap.Should().BeEquivalentTo([
            KeyValuePair.Create<string, ResourceID>("A", 1),
            KeyValuePair.Create<string, ResourceID>("B", 2),
        ]);
    }
    
    [Fact]
    public void RemoveFromName_Contains_RemovesCorrectly() {
        _registry.Add(1, new("A", []));
        _registry.Add(2, new("B", []));
        _registry.Add(3, new("C", []));

        _registry.Remove("B").Should().BeTrue();

        _registry.Should().BeEquivalentTo([
            KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(1, new("A", [])),
            KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(3, new("C", [])),
        ]);
        
        _registry.NameMap.Should().BeEquivalentTo([
            KeyValuePair.Create<string, ResourceID>("A", 1),
            KeyValuePair.Create<string, ResourceID>("C", 3),
        ]);
    }
    
    [Fact]
    public void RemoveFromName_Contains_RemovesCorrectlyAndReturnsCorrectObject() {
        _registry.Add(1, new("A", []));
        _registry.Add(2, new("B", []));
        _registry.Add(3, new("C", []));

        _registry.Remove("B", out var removed).Should().BeTrue();
        removed.Should().Be(new ResourceRegistry.Element("B", []));
        
        _registry.Should().BeEquivalentTo([
            KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(1, new("A", [])),
            KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(3, new("C", [])),
        ]);
        
        _registry.NameMap.Should().BeEquivalentTo([
            KeyValuePair.Create<string, ResourceID>("A", 1),
            KeyValuePair.Create<string, ResourceID>("C", 3),
        ]);
    }

    [Fact]
    public void RemoveFromName_NotContains_DoesNothing() {
        _registry.Add(1, new("A", []));
        _registry.Add(2, new("B", []));
        
        _registry.Remove("C").Should().BeFalse();
        
        _registry.Should().BeEquivalentTo([
            KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(1, new("A", [])),
            KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(2, new("B", [])),
        ]);
        
        _registry.NameMap.Should().BeEquivalentTo([
            KeyValuePair.Create<string, ResourceID>("A", 1),
            KeyValuePair.Create<string, ResourceID>("B", 2),
        ]);
    }

    [Fact]
    public void InterfaceRemove_Normal_RemovesCorrectly() {
        _registry.Add(1, new("A", []));
        _registry.Add(2, new("B", []));

        _registry.Should().HaveCount(2);
        
        ICollection<KeyValuePair<ResourceID, ResourceRegistry.Element>> registry = _registry;

        new Func<bool>(() => registry.Remove(KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(1, new("A", [])))).Should().NotThrow().Which.Should().BeTrue();
        registry.Should().HaveCount(1);
    }
    
    [Fact]
    public void InterfaceRemove_DifferentName_NotRemove() {
        _registry.Add(1, new("A", []));
        _registry.Add(2, new("B", []));
        
        ICollection<KeyValuePair<ResourceID, ResourceRegistry.Element>> registry = _registry;

        new Func<bool>(() => registry.Remove(KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(1, new("B", [])))).Should().NotThrow().Which.Should().BeFalse();
        registry.Should().HaveCount(2);
    }
    
    [Fact]
    public void InterfaceRemove_UnregisteredId_NotRemove() {
        _registry.Add(1, new("A", []));
        _registry.Add(2, new("B", []));
        
        ICollection<KeyValuePair<ResourceID, ResourceRegistry.Element>> registry = _registry;

        new Func<bool>(() => registry.Remove(KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(999, new("A", [])))).Should().NotThrow().Which.Should().BeFalse();
        registry.Should().HaveCount(2);
    }
    
    [Fact]
    public void InterfaceRemove_DifferentTagCollection_NotRemove() {
        _registry.Add(1, new("A", []));
        _registry.Add(2, new("B", []));
        
        ICollection<KeyValuePair<ResourceID, ResourceRegistry.Element>> registry = _registry;

        new Func<bool>(() => registry.Remove(KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(1, new("A", ["Test"])))).Should().NotThrow().Which.Should().BeFalse();
        registry.Should().HaveCount(2);
    }

    [Fact]
    public void Clear_ClearsEverything() {
        _registry.Add(1, new("A", []));
        _registry.Add(2, new("B", []));

        _registry.Should().HaveCount(2).And.BeEquivalentTo([
            KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(1, new("A", [])),
            KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(2, new("B", [])),
        ]);
        
        _registry.Clear();

        _registry.Should().BeEmpty().And.BeEquivalentTo<KeyValuePair<ResourceID, ResourceRegistry.Element>>([]);
        _registry.NameMap.Should().BeEmpty();
    }

    [Fact]
    public void ContainsKey_Contains_ReturnsTrue() {
        _registry.Add(1, new("A", []));
        _registry.Add(2, new("B", []));

        _registry.ContainsKey(1).Should().BeTrue();
    }
    
    [Fact]
    public void ContainsKey_NotContains_ReturnsTrue() {
        _registry.Add(1, new("A", []));
        _registry.Add(2, new("B", []));

        _registry.ContainsKey(4).Should().BeFalse();
    }
    
    [Fact]
    public void ContainsName_Contains_ReturnsTrue() {
        _registry.Add(1, new("A", []));
        _registry.Add(2, new("B", []));

        _registry.ContainsName("B").Should().BeTrue();
    }
    
    [Fact]
    public void ContainsName_NotContains_ReturnsTrue() {
        _registry.Add(1, new("A", []));
        _registry.Add(2, new("B", []));

        _registry.ContainsName("E").Should().BeFalse();
    }

    [Fact]
    public void TryGetValueId_Contains_ReturnsTrueAndReturnsCorrectObject() {
        _registry.Add(1, new("A", []));
        _registry.Add(2, new("B", []));

        _registry.TryGetValue(1, out var element).Should().BeTrue();
        element.Should().Be(new ResourceRegistry.Element("A", []));
    }
    
    [Fact]
    public void TryGetValueId_NotContains_ReturnsFalse() {
        _registry.Add(1, new("A", []));
        _registry.Add(2, new("B", []));

        _registry.TryGetValue(10, out _).Should().BeFalse();
    }
    
    [Fact]
    public void TryGetValueName_Contains_ReturnsTrueAndReturnsCorrectObject() {
        _registry.Add(1, new("A", []));
        _registry.Add(2, new("B", []));

        _registry.TryGetValue("B", out ResourceRegistry.Element element).Should().BeTrue();
        element.Should().Be(new ResourceRegistry.Element("B", []));
        
        _registry.TryGetValue("B", out ResourceID id).Should().BeTrue();
        id.Should().Be((ResourceID)2);
    }
    
    [Fact]
    public void TryGetValueName_NotContains_ReturnsFalse() {
        _registry.Add(1, new("A", []));
        _registry.Add(2, new("B", []));

        _registry.TryGetValue("E", out ResourceRegistry.Element _).Should().BeFalse();
        _registry.TryGetValue("E", out ResourceID _).Should().BeFalse();
    }

    [Fact]
    public void GetAccessor_Contains_ReturnsCorrectObject() {
        _registry.Add(1, new("A", []));
        _registry.Add(2, new("B", []));

        new Func<ResourceRegistry.Element>(() => _registry[2]).Should().NotThrow().Which.Should().Be(new ResourceRegistry.Element("B", []));
    }

    [Fact]
    public void GetAccessor_NotContains_ThrowssArgumentExceptionReportsKeyNotFound() {
        _registry.Add(1, new("A", []));

        new Func<ResourceRegistry.Element>(() => _registry[2]).Should().Throw<KeyNotFoundException>().WithMessage("*key*not*present*");
    }

    [Fact]
    public void JsonSerialization_OptionlessElement_ReturnsCorrectJson() {
        _registry.Add(1, new("A", ["Texture"]));
        _registry.Add(2, new("B", ["Shader"]));
        _registry.Add(5, new("E", ["Audio"]));

        string json = JsonSerializer.Serialize(_registry);

        json.Should().Be("""{"1":{"Name":"A","Tags":["Texture"]},"2":{"Name":"B","Tags":["Shader"]},"5":{"Name":"E","Tags":["Audio"]}}""");
    }
    
    [Fact]
    public void JsonDeserialization_OptionlessElement_ReturnsCorrectJson() {
        const string json = """{"1":{"Name":"A","Tags":["Locale"]},"2":{"Name":"B","Tags":["Container"]},"3":{"Name":"C","Tags":["Control"]}}""";

        var registry = new Func<ResourceRegistry<ResourceRegistry.Element>>(() => JsonSerializer.Deserialize<ResourceRegistry<ResourceRegistry.Element>>(json)!).Should().NotThrow().Which;

        registry.Should().BeEquivalentTo([
            KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(1, new("A", ["Locale"])),
            KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(2, new("B", ["Container"])),
            KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(3, new("C", ["Control"])),
        ]);

        registry.NameMap.Should().BeEquivalentTo([
            KeyValuePair.Create<string, ResourceID>("A", 1),
            KeyValuePair.Create<string, ResourceID>("B", 2),
            KeyValuePair.Create<string, ResourceID>("C", 3),
        ]);
    }
    
    [Fact]
    public void JsonDeserialization_OptionElement_ReturnsCorrectJson() {
        const string json = """{"1":{"Name":"A","Tags":["Texture"],"Option":10},"2":{"Name":"B","Tags":["Audio","Video"],"Option":30},"3":{"Name":"C","Tags":["Shader","Material"],"Option":50}}""";
    
        var registry = new Func<ResourceRegistry<ResourceRegistry.Element<int>>>(() => JsonSerializer.Deserialize<ResourceRegistry<ResourceRegistry.Element<int>>>(json)!).Should().NotThrow().Which;
        
        registry.Should().BeEquivalentTo([
            KeyValuePair.Create<ResourceID, ResourceRegistry.Element<int>>(1, new("A", ["Texture"], 10)),
            KeyValuePair.Create<ResourceID, ResourceRegistry.Element<int>>(2, new("B", ["Audio", "Video"], 30)),
            KeyValuePair.Create<ResourceID, ResourceRegistry.Element<int>>(3, new("C", ["Shader", "Material"], 50)),
        ]);
        
        registry.NameMap.Should().BeEquivalentTo([
            KeyValuePair.Create<string, ResourceID>("A", 1),
            KeyValuePair.Create<string, ResourceID>("B", 2),
            KeyValuePair.Create<string, ResourceID>("C", 3),
        ]);
    }
    
    [Fact]
    public void JsonSerialization_OptionElement_ReturnsCorrectJson() {
        string json = JsonSerializer.Serialize(new ResourceRegistry<ResourceRegistry.Element<int>> {
            [1] = new("A", ["Texture"], 10),
            [2] = new("B", ["Audio", "Video"], 30),
            [3] = new("C", ["Shader", "Material"], 50),
        });

        json.Should().Be("""{"1":{"Name":"A","Tags":["Texture"],"Option":10},"2":{"Name":"B","Tags":["Audio","Video"],"Option":30},"3":{"Name":"C","Tags":["Shader","Material"],"Option":50}}""");
    }

    [Fact]
    public void Keys_ReturnsCorrectSequence() {
        _registry.Add(1, new("A", []));
        _registry.Add(2, new("B", []));
        _registry.Add(3, new("C", []));
        _registry.Add(4, new("D", []));

        _registry.Keys.Should().BeEquivalentTo(new ResourceID[] {
            1, 2, 3, 4,
        });
    }
    
    [Fact]
    public void Values_ReturnsCorrectSequence() {
        _registry.Add(1, new("A", []));
        _registry.Add(2, new("B", []));
        _registry.Add(3, new("C", []));
        _registry.Add(4, new("D", []));

        _registry.Values.Should().BeEquivalentTo(new ResourceRegistry.Element[] {
            new("A", []),
            new("B", []),
            new("C", []),
            new("D", []),
        });
    }
    
    [Fact]
    public void InterfaceKeys_ReturnsCorrectSequence() {
        _registry.Add(1, new("A", []));
        _registry.Add(2, new("B", []));
        _registry.Add(3, new("C", []));
        _registry.Add(4, new("D", []));

        ((IDictionary<ResourceID, ResourceRegistry.Element>)_registry).Keys.Should().BeEquivalentTo(new ResourceID[] {
            1, 2, 3, 4,
        });
        ((IReadOnlyDictionary<ResourceID, ResourceRegistry.Element>)_registry).Keys.Should().BeEquivalentTo(new ResourceID[] {
            1, 2, 3, 4,
        });
    }
    
    [Fact]
    public void InterfaceValues_ReturnsCorrectSequence() {
        _registry.Add(1, new("A", []));
        _registry.Add(2, new("B", []));
        _registry.Add(3, new("C", []));
        _registry.Add(4, new("D", []));

        ((IDictionary<ResourceID, ResourceRegistry.Element>)_registry).Values.Should().BeEquivalentTo(new ResourceRegistry.Element[] {
            new("A", []),
            new("B", []),
            new("C", []),
            new("D", []),
        });
        ((IReadOnlyDictionary<ResourceID, ResourceRegistry.Element>)_registry).Values.Should().BeEquivalentTo(new ResourceRegistry.Element[] {
            new("A", []),
            new("B", []),
            new("C", []),
            new("D", []),
        });
    }

    [Fact]
    public void GetIndexer_ReturnsCorrectly() {
        _registry.Add(1, new("A", []));
        _registry.Add(2, new("B", []));
        _registry.Add(3, new("C", []));
        _registry.Add(4, new("D", []));

        _registry[2].Should().Be(new ResourceRegistry.Element("B", []));
    }

    [Fact]
    public void SetIndexer_UnregisteredKey_AddsElement() {
        _registry.Add(1, new("A", []));
        _registry.Add(2, new("B", []));

        _registry[3] = new("C", []);

        _registry.Should().BeEquivalentTo([
            KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(1, new("A", [])),
            KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(2, new("B", [])),
            KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(3, new("C", [])),
        ]);
        
        _registry.NameMap.Should().BeEquivalentTo([
            KeyValuePair.Create<string, ResourceID>("A", 1),
            KeyValuePair.Create<string, ResourceID>("B", 2),
            KeyValuePair.Create<string, ResourceID>("C", 3),
        ]);
    }
    
    [Fact]
    public void SetIndexer_OverrideKey_OverridesElement() {
        _registry.Add(1, new("A", []));
        _registry.Add(2, new("B", []));

        _registry[1] = new("AAA", []);

        _registry.Should().BeEquivalentTo([
            KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(1, new("AAA", [])),
            KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(2, new("B", [])),
        ]);
        
        _registry.NameMap.Should().BeEquivalentTo([
            KeyValuePair.Create<string, ResourceID>("AAA", 1),
            KeyValuePair.Create<string, ResourceID>("B", 2),
        ]);
    }

    [Fact]
    public void SetIndexer_DuplicateName_ThrowsArgumentException() {
        _registry.Add(1, new("A", []));
        _registry.Add(2, new("B", []));

        new Action(() => _registry[3] = new("A", [])).Should().Throw<ArgumentException>().WithMessage("*name*");
    }
}