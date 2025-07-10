namespace Caxivitual.Lunacub.Tests;

public class ResourceRegistryOfTTests {
    private readonly ResourceRegistry<ResourceRegistry.Element> _registry = [];
    
    [Fact]
    public void Add_Normal_ShouldBeCorrect() {
        new Action(() => _registry.Add(1, new("A", []))).Should().NotThrow();
        new Action(() => _registry.Add(2, new("B", []))).Should().NotThrow();

        _registry.Should().BeEquivalentTo([
            KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(1, new("A", [])),
            KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(2, new("B", [])),
        ]);
    }

    [Fact]
    public void Add_DuplicateId_ShouldThrowArgumentExceptionReportIdAlreadyRegistered() {
        new Action(() => _registry.Add(1, new("A", []))).Should().NotThrow();
        new Action(() => _registry.Add(1, new("B", []))).Should().Throw<ArgumentException>().WithMessage("*id*already*registered*");
    }

    [Fact]
    public void Add_DuplicateName_ShouldThrowArgumentExceptionReportNameAlreadyRegistered() {
        new Action(() => _registry.Add(1, new("A", []))).Should().NotThrow();
        new Action(() => _registry.Add(2, new("A", []))).Should().Throw<ArgumentException>().WithMessage("*name*already*registered*");
    }

    [Fact]
    public void Add_NullName_ShouldThrowArgumentExceptionReportInvalidName() {
        new Action(() => _registry.Add(1, new(null!, []))).Should().Throw<ArgumentException>().WithMessage("*name*null*");
    }
    
    [Fact]
    public void Add_EmptyName_ShouldThrowArgumentExceptionReportInvalidName() {
        new Action(() => _registry.Add(1, new(string.Empty, []))).Should().Throw<ArgumentException>().WithMessage("*name*empty*");
    }
    
    [Fact]
    public void Add_NullTagCollection_ShouldThrowNullReferenceException() {
        new Action(() => _registry.Add(1, new("A", default))).Should().Throw<NullReferenceException>();
    }
    
    [Fact]
    public void Add_NullTag_ShouldThrowArgumentExceptionReportNullTag() {
        new Action(() => _registry.Add(1, new("A", [
            null!,
        ]))).Should().Throw<ArgumentException>().WithMessage("*tag*null*");
    }
    
    [Fact]
    public void Add_EmptyTag_ShouldThrowArgumentExceptionReportEmptyTag() {
        new Action(() => _registry.Add(1, new("A", [
            null!,
        ]))).Should().Throw<ArgumentException>().WithMessage("*tag*empty*");
    }
    
    [Fact]
    public void Add_InvalidTagCharacter_ShouldThrowArgumentExceptionReportInvalidTagCharacter() {
        new Action(() => _registry.Add(1, new("A", [
            "Something^Invalid",
        ]))).Should().Throw<ArgumentException>().WithMessage("*tag*invalid*character*");
    }

    [Fact]
    public void Count_Normal_ShouldChangeCorrectly() {
        _registry.Should().HaveCount(0);
        _registry.Add(1, new("A", []));
        _registry.Should().HaveCount(1);
    }

    [Fact]
    public void Count_DuplicateId_ShouldNotChanged() {
        _registry.Should().HaveCount(0);
        _registry.Add(1, new("A", []));
        new Action(() => _registry.Add(1, new("B", []))).Should().Throw<ArgumentException>();
        _registry.Should().HaveCount(1);
    }
    
    [Fact]
    public void Count_DuplicateName_ShouldNotChanged() {
        _registry.Should().HaveCount(0);
        _registry.Add(1, new("A", []));
        new Action(() => _registry.Add(2, new("A", []))).Should().Throw<ArgumentException>();
        _registry.Should().HaveCount(1);
    }

    [Fact]
    public void RemoveFromId_Contains_ShouldRemovesCorrectly() {
        _registry.Add(1, new("A", []));
        _registry.Add(2, new("B", []));
        _registry.Add(3, new("C", []));

        _registry.Remove(2).Should().BeTrue();

        _registry.Should().BeEquivalentTo([
            KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(1, new("A", [])),
            KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(3, new("C", [])),
        ]);
    }
    
    [Fact]
    public void RemoveFromId_Contains_ShouldRemovesCorrectlyAndReturnsCorrectObject() {
        _registry.Add(1, new("A", []));
        _registry.Add(2, new("B", []));
        _registry.Add(3, new("C", []));

        _registry.Remove(2, out var removed).Should().BeTrue();
        removed.Should().Be(new ResourceRegistry.Element("B", []));
        
        _registry.Should().BeEquivalentTo([
            KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(1, new("A", [])),
            KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(3, new("C", [])),
        ]);
    }

    [Fact]
    public void RemoveFromId_NotContains_ShouldDoNothing() {
        _registry.Add(1, new("A", []));
        _registry.Add(2, new("B", []));
        
        _registry.Remove(3).Should().BeFalse();
        
        _registry.Should().BeEquivalentTo([
            KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(1, new("A", [])),
            KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(2, new("B", [])),
        ]);
    }
    
    [Fact]
    public void RemoveFromName_Contains_ShouldRemovesCorrectly() {
        _registry.Add(1, new("A", []));
        _registry.Add(2, new("B", []));
        _registry.Add(3, new("C", []));

        _registry.Remove("B").Should().BeTrue();

        _registry.Should().BeEquivalentTo([
            KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(1, new("A", [])),
            KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(3, new("C", [])),
        ]);
    }
    
    [Fact]
    public void RemoveFromName_Contains_ShouldRemovesCorrectlyAndReturnsCorrectObject() {
        _registry.Add(1, new("A", []));
        _registry.Add(2, new("B", []));
        _registry.Add(3, new("C", []));

        _registry.Remove("B", out var removed).Should().BeTrue();
        removed.Should().Be(new ResourceRegistry.Element("B", []));
        
        _registry.Should().BeEquivalentTo([
            KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(1, new("A", [])),
            KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(3, new("C", [])),
        ]);
    }

    [Fact]
    public void RemoveFromName_NotContains_ShouldDoNothing() {
        _registry.Add(1, new("A", []));
        _registry.Add(2, new("B", []));
        
        _registry.Remove("C").Should().BeFalse();
        
        _registry.Should().BeEquivalentTo([
            KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(1, new("A", [])),
            KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(2, new("B", [])),
        ]);
    }

    [Fact]
    public void Clear_ShouldClearEverything() {
        _registry.Add(1, new("A", []));
        _registry.Add(2, new("B", []));

        _registry.Should().HaveCount(2).And.BeEquivalentTo([
            KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(1, new("A", [])),
            KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(2, new("B", [])),
        ]);
        
        _registry.Clear();

        _registry.Should().HaveCount(0).And.BeEquivalentTo<KeyValuePair<ResourceID, ResourceRegistry.Element>>([]);
    }

    [Fact]
    public void ContainsKeyId_Contains_ShouldReturnsTrue() {
        _registry.Add(1, new("A", []));
        _registry.Add(2, new("B", []));

        _registry.ContainsKey(1).Should().BeTrue();
    }
    
    [Fact]
    public void ContainsKeyId_NotContains_ShouldReturnsTrue() {
        _registry.Add(1, new("A", []));
        _registry.Add(2, new("B", []));

        _registry.ContainsKey(4).Should().BeFalse();
    }
    
    [Fact]
    public void ContainsKeyName_Contains_ShouldReturnsTrue() {
        _registry.Add(1, new("A", []));
        _registry.Add(2, new("B", []));

        _registry.ContainsKey("B").Should().BeTrue();
    }
    
    [Fact]
    public void ContainsKeyName_NotContains_ShouldReturnsTrue() {
        _registry.Add(1, new("A", []));
        _registry.Add(2, new("B", []));

        _registry.ContainsKey("E").Should().BeFalse();
    }

    [Fact]
    public void TryGetValueId_Contains_ShouldReturnsTrueAndReturnsCorrectObject() {
        _registry.Add(1, new("A", []));
        _registry.Add(2, new("B", []));

        _registry.TryGetValue(1, out var element).Should().BeTrue();
        element.Should().Be(new ResourceRegistry.Element("A", []));
    }
    
    [Fact]
    public void TryGetValueId_NotContains_ShouldReturnsFalse() {
        _registry.Add(1, new("A", []));
        _registry.Add(2, new("B", []));

        _registry.TryGetValue(10, out _).Should().BeFalse();
    }
    
    [Fact]
    public void TryGetValueName_Contains_ShouldReturnsTrueAndReturnsCorrectObject() {
        _registry.Add(1, new("A", []));
        _registry.Add(2, new("B", []));

        _registry.TryGetValue("B", out var element).Should().BeTrue();
        element.Should().Be(new ResourceRegistry.Element("B", []));
    }
    
    [Fact]
    public void TryGetValueName_NotContains_ShouldReturnsFalse() {
        _registry.Add(1, new("A", []));
        _registry.Add(2, new("B", []));

        _registry.TryGetValue("E", out _).Should().BeFalse();
    }

    [Fact]
    public void GetAccessor_Contains_ShouldReturnsCorrectObject() {
        _registry.Add(1, new("A", []));
        _registry.Add(2, new("B", []));

        new Func<ResourceRegistry.Element>(() => _registry[2]).Should().NotThrow().Which.Should().Be(new ResourceRegistry.Element("B", []));
    }

    [Fact]
    public void GetAccessor_NotContains_ShouldThrowsArgumentExceptionReportsKeyNotFound() {
        _registry.Add(1, new("A", []));

        new Func<ResourceRegistry.Element>(() => _registry[2]).Should().Throw<KeyNotFoundException>().WithMessage("*key*not*present*");
    }

    [Fact]
    public void JsonSerialization_OptionlessElement_ShouldReturnsCorrectJson() {
        _registry.Add(1, new("A", ["Texture"]));
        _registry.Add(2, new("B", ["Shader"]));
        _registry.Add(5, new("E", ["Audio"]));

        string json = JsonSerializer.Serialize(_registry);

        json.Should().Be("""{"1":{"Name":"A","Tags":["Texture"]},"2":{"Name":"B","Tags":["Shader"]},"5":{"Name":"E","Tags":["Audio"]}}""");
    }
    
    [Fact]
    public void JsonDeserialization_OptionlessElement_ShouldReturnsCorrectJson() {
        const string json = """{"1":{"Name":"A","Tags":["Locale"]},"2":{"Name":"B","Tags":["Container"]},"3":{"Name":"C","Tags":["Control"]}}""";

        new Func<ResourceRegistry<ResourceRegistry.Element>>(() => JsonSerializer.Deserialize<ResourceRegistry<ResourceRegistry.Element>>(json)!).Should().NotThrow().Which.Should().BeEquivalentTo([
            KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(1, new("A", ["Locale"])),
            KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(2, new("B", ["Container"])),
            KeyValuePair.Create<ResourceID, ResourceRegistry.Element>(3, new("C", ["Control"])),
        ]);
    }
    
    [Fact]
    public void JsonSerialization_OptionElement_ShouldReturnsCorrectJson() {
        string json = JsonSerializer.Serialize(new ResourceRegistry<ResourceRegistry.Element<int>> {
            [1] = new("A", ["Texture"], 10),
            [2] = new("B", ["Audio", "Video"], 30),
            [3] = new("C", ["Shader", "Material"], 50),
        });

        json.Should().Be("""{"1":{"Name":"A","Tags":["Texture"],"Option":10},"2":{"Name":"B","Tags":["Audio","Video"],"Option":30},"3":{"Name":"C","Tags":["Shader","Material"],"Option":50}}""");
    }
    
    [Fact]
    public void JsonDeserialization_OptionElement_ShouldReturnsCorrectJson() {
        const string json = """{"1":{"Name":"A","Tags":["Texture"],"Option":10},"2":{"Name":"B","Tags":["Audio","Video"],"Option":30},"3":{"Name":"C","Tags":["Shader","Material"],"Option":50}}""";
    
        new Func<ResourceRegistry<ResourceRegistry.Element<int>>>(() => JsonSerializer.Deserialize<ResourceRegistry<ResourceRegistry.Element<int>>>(json)!).Should().NotThrow().Which.Should().BeEquivalentTo([
            KeyValuePair.Create<ResourceID, ResourceRegistry.Element<int>>(1, new("A", ["Texture"], 10)),
            KeyValuePair.Create<ResourceID, ResourceRegistry.Element<int>>(2, new("B", ["Audio", "Video"], 30)),
            KeyValuePair.Create<ResourceID, ResourceRegistry.Element<int>>(3, new("C", ["Shader", "Material"], 50)),
        ]);
    }
}