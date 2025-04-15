namespace Caxivitual.Lunacub.Tests.Importing;

public class TagFilterTests {
    public TagFilterTests(ITestOutputHelper output) {
        DebugHelpers.RedirectConsoleOutput(output);
    }

    [Fact]
    public void Parse_SingleTag_HaveExpectedExpressionTree() {
        TagFilter filter = new("A");

        filter.RootExpression.Should().BeOfType<TagFilter.LiteralExpression>().Which.ValueRange.Should().Be(..1);
    }
    
    [Fact]
    public void Parse_And_HaveExpectedExpressionTree() {
        TagFilter filter = new("A & B");

        TagFilter.BinaryExpression binary = filter.RootExpression.Should().BeOfType<TagFilter.BinaryExpression>().Which;
        binary.Left.Should().BeOfType<TagFilter.LiteralExpression>().Which.ValueRange.Should().Be(..1);
        binary.Operator.Should().Be(TagFilter.TokenType.And);
        binary.Right.Should().BeOfType<TagFilter.LiteralExpression>().Which.ValueRange.Should().Be(4..5);
    }
    
    [Fact]
    public void Parse_Or_HaveExpectedExpressionTree() {
        TagFilter filter = new("A | B");

        TagFilter.BinaryExpression binary = filter.RootExpression.Should().BeOfType<TagFilter.BinaryExpression>().Which;
        binary.Left.Should().BeOfType<TagFilter.LiteralExpression>().Which.ValueRange.Should().Be(..1);
        binary.Operator.Should().Be(TagFilter.TokenType.Or);
        binary.Right.Should().BeOfType<TagFilter.LiteralExpression>().Which.ValueRange.Should().Be(4..5);
    }
    
    [Fact]
    public void Parse_Xor_HaveExpectedExpressionTree() {
        TagFilter filter = new("A ^ B");

        TagFilter.BinaryExpression binary = filter.RootExpression.Should().BeOfType<TagFilter.BinaryExpression>().Which;
        binary.Left.Should().BeOfType<TagFilter.LiteralExpression>().Which.ValueRange.Should().Be(..1);
        binary.Operator.Should().Be(TagFilter.TokenType.Xor);
        binary.Right.Should().BeOfType<TagFilter.LiteralExpression>().Which.ValueRange.Should().Be(4..5);    }
    
    [Fact]
    public void Parse_Not_HaveExpectedExpressionTree() {
        TagFilter filter = new("!A");

        TagFilter.UnaryExpression unary = filter.RootExpression.Should().BeOfType<TagFilter.UnaryExpression>().Which;
        unary.Operator.Should().Be(TagFilter.TokenType.Not);
        unary.Expression.Should().BeOfType<TagFilter.LiteralExpression>().Which.ValueRange.Should().Be(1..2);
    }
    
    [Fact]
    public void Parse_PrecedenceDifference1_HaveExpectedExpressionTree() {
        TagFilter filter = new("A & B | C");
        
        var root = filter.RootExpression.Should().BeOfType<TagFilter.BinaryExpression>().Which;
        
        var rootL = root.Left.Should().BeOfType<TagFilter.BinaryExpression>().Which;
        rootL.Left.Should().BeOfType<TagFilter.LiteralExpression>().Which.ValueRange.Should().Be(..1);
        rootL.Operator.Should().Be(TagFilter.TokenType.And);
        rootL.Right.Should().BeOfType<TagFilter.LiteralExpression>().Which.ValueRange.Should().Be(4..5);
        
        root.Operator.Should().Be(TagFilter.TokenType.Or);
        root.Right.Should().BeOfType<TagFilter.LiteralExpression>().Which.ValueRange.Should().Be(8..9);
    }

    [Fact]
    public void Parse_PrecedenceDifference2_HaveExpectedExpressionTree() {
        TagFilter filter = new("A & !B");
        
        var root = filter.RootExpression.Should().BeOfType<TagFilter.BinaryExpression>().Which;
        root.Left.Should().BeOfType<TagFilter.LiteralExpression>().Which.ValueRange.Should().Be(..1);
        root.Operator.Should().Be(TagFilter.TokenType.And);
        
        var rootR = root.Right.Should().BeOfType<TagFilter.UnaryExpression>().Which;
        rootR.Operator.Should().Be(TagFilter.TokenType.Not);
        rootR.Expression.Should().BeOfType<TagFilter.LiteralExpression>().Which.ValueRange.Should().Be(5..6);
    }

    [Fact]
    public void Parse_PrecedenceDifference3_HaveExpectedExpressionTree() {
        TagFilter filter = new("A & B ^ C | !D");
        
        var root = filter.RootExpression.Should().BeOfType<TagFilter.BinaryExpression>().Which;
        
        var rootL = root.Left.Should().BeOfType<TagFilter.BinaryExpression>().Which;
        
        var rootLL = rootL.Left.Should().BeOfType<TagFilter.BinaryExpression>().Which;
        
        rootLL.Left.Should().BeOfType<TagFilter.LiteralExpression>().Which.ValueRange.Should().Be(..1);
        rootLL.Operator.Should().Be(TagFilter.TokenType.And);
        rootLL.Right.Should().BeOfType<TagFilter.LiteralExpression>().Which.ValueRange.Should().Be(4..5);
        
        rootL.Right.Should().BeOfType<TagFilter.LiteralExpression>().Which.ValueRange.Should().Be(8..9);
        
        root.Operator.Should().Be(TagFilter.TokenType.Or);

        var rootR = root.Right.Should().BeOfType<TagFilter.UnaryExpression>().Which;
        rootR.Operator.Should().Be(TagFilter.TokenType.Not);
        rootR.Expression.Should().BeOfType<TagFilter.LiteralExpression>().Which.ValueRange.Should().Be(13..14);
    }

    [Fact]
    public void Parse_Grouping_HaveExpectedExpressionTree() {
        TagFilter filter = new("A & (B | C)");
        
        var root = filter.RootExpression.Should().BeOfType<TagFilter.BinaryExpression>().Which;
        root.Left.Should().BeOfType<TagFilter.LiteralExpression>().Which.ValueRange.Should().Be(..1);
        root.Operator.Should().Be(TagFilter.TokenType.And);
        
        var rootR = root.Right.Should().BeOfType<TagFilter.BinaryExpression>().Which;

        rootR.Left.Should().BeOfType<TagFilter.LiteralExpression>().Which.ValueRange.Should().Be(5..6);
        rootR.Operator.Should().Be(TagFilter.TokenType.Or);
        rootR.Right.Should().BeOfType<TagFilter.LiteralExpression>().Which.ValueRange.Should().Be(9..10);
    }

    [Fact]
    public void Parse_TagCapture_HaveExpectedExpressionTree() {
        TagFilter filter = new("ModeA & (ModeB | ModeC)");
        
        var root = filter.RootExpression.Should().BeOfType<TagFilter.BinaryExpression>().Which;
        root.Left.Should().BeOfType<TagFilter.LiteralExpression>().Which.ValueRange.Should().Be(..5);
        root.Operator.Should().Be(TagFilter.TokenType.And);
        
        var rootR = root.Right.Should().BeOfType<TagFilter.BinaryExpression>().Which;

        rootR.Left.Should().BeOfType<TagFilter.LiteralExpression>().Which.ValueRange.Should().Be(9..14);
        rootR.Operator.Should().Be(TagFilter.TokenType.Or);
        rootR.Right.Should().BeOfType<TagFilter.LiteralExpression>().Which.ValueRange.Should().Be(17..22);
    }
    
    [Fact]
    public void Check_SingleTag_ReturnsCorrectly() {
        TagFilter filter = new("C");

        filter.Check(["A", "B", "C"]).Should().BeTrue();
        filter.Check(["A", "D", "E"]).Should().BeFalse();
    }

    [Fact]
    public void Check_And_ReturnsCorrectly() {
        TagFilter filter = new("A & B");
        
        filter.Check(["A", "B", "C"]).Should().BeTrue();
        filter.Check(["A", "D", "E"]).Should().BeFalse();
        filter.Check(["B", "F"]).Should().BeFalse();
    }

    [Fact]
    public void Check_Or_ReturnsCorrectly() {
        TagFilter filter = new("A | B");
        
        filter.Check(["A"]).Should().BeTrue();
        filter.Check(["B"]).Should().BeTrue();
        filter.Check(["A", "B"]).Should().BeTrue();
        filter.Check(["C"]).Should().BeFalse();
    }

    [Fact]
    public void Check_Xor_ReturnsCorrectly() {
        TagFilter filter = new("A ^ B");
        
        filter.Check([]).Should().BeFalse();
        filter.Check(["A"]).Should().BeTrue();
        filter.Check(["B"]).Should().BeTrue();
        filter.Check(["A", "B"]).Should().BeFalse();
    }

    [Fact]
    public void Check_Not_ReturnsCorrectly() {
        TagFilter filter = new("!A");
        
        filter.Check(["A"]).Should().BeFalse();
        filter.Check(["B"]).Should().BeTrue();
    }

    [Fact]
    public void Check_PrecedenceDifference1_ReturnsCorrectly() {
        TagFilter filter = new("A & B | C");
        
        filter.Check(["A", "B", "C"]).Should().BeTrue();
        filter.Check(["A", "B"]).Should().BeTrue();
        filter.Check(["A", "C"]).Should().BeTrue();
        filter.Check(["B", "C"]).Should().BeTrue();
        filter.Check(["A"]).Should().BeFalse();
        filter.Check(["B"]).Should().BeFalse();
        filter.Check(["C"]).Should().BeTrue();
    }
    
    [Fact]
    public void Check_PrecedenceDifference2_ReturnsCorrectly() {
        TagFilter filter = new("A & (B | C)");
        
        filter.Check(["A", "B", "C"]).Should().BeTrue();
        filter.Check(["A", "B"]).Should().BeTrue();
        filter.Check(["A", "C"]).Should().BeTrue();
        filter.Check(["B", "C"]).Should().BeFalse();
        filter.Check(["A"]).Should().BeFalse();
        filter.Check(["B"]).Should().BeFalse();
        filter.Check(["C"]).Should().BeFalse();
    }
    
    [Fact]
    public void Check_PrecedenceDifference3_ReturnsCorrectly() {
        TagFilter filter = new("A & !(B | C)");

        filter.Check(["A", "B", "C"]).Should().BeFalse();
        filter.Check(["A", "B"]).Should().BeFalse();
        filter.Check(["A", "C"]).Should().BeFalse();
        filter.Check(["B", "C"]).Should().BeFalse();
        filter.Check(["A"]).Should().BeTrue();
        filter.Check(["B"]).Should().BeFalse();
        filter.Check(["C"]).Should().BeFalse();
    }
}