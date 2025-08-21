namespace Caxivitual.Lunacub.Tests;

public class TagQueryTests {
    [Fact]
    public void Parse_SingleTag_HaveExpectedExpressionTree() {
        TagQuery query = new("A");

        query.RootExpression.Should().BeOfType<TagQuery.LiteralExpression>().Which.ValueRange.Should().Be(..1);
    }
    
    [Fact]
    public void Parse_And_HaveExpectedExpressionTree() {
        TagQuery query = new("A & B");

        TagQuery.BinaryExpression binary = query.RootExpression.Should().BeOfType<TagQuery.BinaryExpression>().Which;
        binary.Left.Should().BeOfType<TagQuery.LiteralExpression>().Which.ValueRange.Should().Be(..1);
        binary.Operator.Should().Be(TagQuery.TokenType.And);
        binary.Right.Should().BeOfType<TagQuery.LiteralExpression>().Which.ValueRange.Should().Be(4..5);
    }
    
    [Fact]
    public void Parse_Or_HaveExpectedExpressionTree() {
        TagQuery query = new("A | B");

        TagQuery.BinaryExpression binary = query.RootExpression.Should().BeOfType<TagQuery.BinaryExpression>().Which;
        binary.Left.Should().BeOfType<TagQuery.LiteralExpression>().Which.ValueRange.Should().Be(..1);
        binary.Operator.Should().Be(TagQuery.TokenType.Or);
        binary.Right.Should().BeOfType<TagQuery.LiteralExpression>().Which.ValueRange.Should().Be(4..5);
    }
    
    [Fact]
    public void Parse_Xor_HaveExpectedExpressionTree() {
        TagQuery query = new("A ^ B");

        TagQuery.BinaryExpression binary = query.RootExpression.Should().BeOfType<TagQuery.BinaryExpression>().Which;
        binary.Left.Should().BeOfType<TagQuery.LiteralExpression>().Which.ValueRange.Should().Be(..1);
        binary.Operator.Should().Be(TagQuery.TokenType.Xor);
        binary.Right.Should().BeOfType<TagQuery.LiteralExpression>().Which.ValueRange.Should().Be(4..5);    }
    
    [Fact]
    public void Parse_Not_HaveExpectedExpressionTree() {
        TagQuery query = new("!A");

        TagQuery.UnaryExpression unary = query.RootExpression.Should().BeOfType<TagQuery.UnaryExpression>().Which;
        unary.Operator.Should().Be(TagQuery.TokenType.Not);
        unary.Expression.Should().BeOfType<TagQuery.LiteralExpression>().Which.ValueRange.Should().Be(1..2);
    }
    
    [Fact]
    public void Parse_PrecedenceDifference1_HaveExpectedExpressionTree() {
        TagQuery query = new("A & B | C");
        
        var root = query.RootExpression.Should().BeOfType<TagQuery.BinaryExpression>().Which;
        
        var rootL = root.Left.Should().BeOfType<TagQuery.BinaryExpression>().Which;
        rootL.Left.Should().BeOfType<TagQuery.LiteralExpression>().Which.ValueRange.Should().Be(..1);
        rootL.Operator.Should().Be(TagQuery.TokenType.And);
        rootL.Right.Should().BeOfType<TagQuery.LiteralExpression>().Which.ValueRange.Should().Be(4..5);
        
        root.Operator.Should().Be(TagQuery.TokenType.Or);
        root.Right.Should().BeOfType<TagQuery.LiteralExpression>().Which.ValueRange.Should().Be(8..9);
    }

    [Fact]
    public void Parse_PrecedenceDifference2_HaveExpectedExpressionTree() {
        TagQuery query = new("A & !B");
        
        var root = query.RootExpression.Should().BeOfType<TagQuery.BinaryExpression>().Which;
        root.Left.Should().BeOfType<TagQuery.LiteralExpression>().Which.ValueRange.Should().Be(..1);
        root.Operator.Should().Be(TagQuery.TokenType.And);
        
        var rootR = root.Right.Should().BeOfType<TagQuery.UnaryExpression>().Which;
        rootR.Operator.Should().Be(TagQuery.TokenType.Not);
        rootR.Expression.Should().BeOfType<TagQuery.LiteralExpression>().Which.ValueRange.Should().Be(5..6);
    }

    [Fact]
    public void Parse_PrecedenceDifference3_HaveExpectedExpressionTree() {
        TagQuery query = new("A & B ^ C | !D");
        
        var root = query.RootExpression.Should().BeOfType<TagQuery.BinaryExpression>().Which;
        
        var rootL = root.Left.Should().BeOfType<TagQuery.BinaryExpression>().Which;
        
        var rootLL = rootL.Left.Should().BeOfType<TagQuery.BinaryExpression>().Which;
        
        rootLL.Left.Should().BeOfType<TagQuery.LiteralExpression>().Which.ValueRange.Should().Be(..1);
        rootLL.Operator.Should().Be(TagQuery.TokenType.And);
        rootLL.Right.Should().BeOfType<TagQuery.LiteralExpression>().Which.ValueRange.Should().Be(4..5);
        
        rootL.Right.Should().BeOfType<TagQuery.LiteralExpression>().Which.ValueRange.Should().Be(8..9);
        
        root.Operator.Should().Be(TagQuery.TokenType.Or);

        var rootR = root.Right.Should().BeOfType<TagQuery.UnaryExpression>().Which;
        rootR.Operator.Should().Be(TagQuery.TokenType.Not);
        rootR.Expression.Should().BeOfType<TagQuery.LiteralExpression>().Which.ValueRange.Should().Be(13..14);
    }

    [Fact]
    public void Parse_Grouping_HaveExpectedExpressionTree() {
        TagQuery query = new("A & (B | C)");
        
        var root = query.RootExpression.Should().BeOfType<TagQuery.BinaryExpression>().Which;
        root.Left.Should().BeOfType<TagQuery.LiteralExpression>().Which.ValueRange.Should().Be(..1);
        root.Operator.Should().Be(TagQuery.TokenType.And);
        
        var rootR = root.Right.Should().BeOfType<TagQuery.BinaryExpression>().Which;

        rootR.Left.Should().BeOfType<TagQuery.LiteralExpression>().Which.ValueRange.Should().Be(5..6);
        rootR.Operator.Should().Be(TagQuery.TokenType.Or);
        rootR.Right.Should().BeOfType<TagQuery.LiteralExpression>().Which.ValueRange.Should().Be(9..10);
    }

    [Fact]
    public void Parse_TagCapture_HaveExpectedExpressionTree() {
        TagQuery query = new("ModeA & (ModeB | ModeC)");
        
        var root = query.RootExpression.Should().BeOfType<TagQuery.BinaryExpression>().Which;
        root.Left.Should().BeOfType<TagQuery.LiteralExpression>().Which.ValueRange.Should().Be(..5);
        root.Operator.Should().Be(TagQuery.TokenType.And);
        
        var rootR = root.Right.Should().BeOfType<TagQuery.BinaryExpression>().Which;

        rootR.Left.Should().BeOfType<TagQuery.LiteralExpression>().Which.ValueRange.Should().Be(9..14);
        rootR.Operator.Should().Be(TagQuery.TokenType.Or);
        rootR.Right.Should().BeOfType<TagQuery.LiteralExpression>().Which.ValueRange.Should().Be(17..22);
    }
    
    [Fact]
    public void Check_SingleTag_ReturnsCorrectly() {
        TagQuery query = new("C");

        query.Check(["A", "B", "C"]).Should().BeTrue();
        query.Check(["A", "D", "E"]).Should().BeFalse();
    }

    [Fact]
    public void Check_And_ReturnsCorrectly() {
        TagQuery query = new("A & B");
        
        query.Check(["A", "B", "C"]).Should().BeTrue();
        query.Check(["A", "D", "E"]).Should().BeFalse();
        query.Check(["B", "F"]).Should().BeFalse();
    }

    [Fact]
    public void Check_Or_ReturnsCorrectly() {
        TagQuery query = new("A | B");
        
        query.Check(["A"]).Should().BeTrue();
        query.Check(["B"]).Should().BeTrue();
        query.Check(["A", "B"]).Should().BeTrue();
        query.Check(["C"]).Should().BeFalse();
    }

    [Fact]
    public void Check_Xor_ReturnsCorrectly() {
        TagQuery query = new("A ^ B");
        
        query.Check([]).Should().BeFalse();
        query.Check(["A"]).Should().BeTrue();
        query.Check(["B"]).Should().BeTrue();
        query.Check(["A", "B"]).Should().BeFalse();
    }

    [Fact]
    public void Check_Not_ReturnsCorrectly() {
        TagQuery query = new("!A");
        
        query.Check(["A"]).Should().BeFalse();
        query.Check(["B"]).Should().BeTrue();
    }

    [Fact]
    public void Check_PrecedenceDifference1_ReturnsCorrectly() {
        TagQuery query = new("A & B | C");
        
        query.Check(["A", "B", "C"]).Should().BeTrue();
        query.Check(["A", "B"]).Should().BeTrue();
        query.Check(["A", "C"]).Should().BeTrue();
        query.Check(["B", "C"]).Should().BeTrue();
        query.Check(["A"]).Should().BeFalse();
        query.Check(["B"]).Should().BeFalse();
        query.Check(["C"]).Should().BeTrue();
    }
    
    [Fact]
    public void Check_PrecedenceDifference2_ReturnsCorrectly() {
        TagQuery query = new("A & (B | C)");
        
        query.Check(["A", "B", "C"]).Should().BeTrue();
        query.Check(["A", "B"]).Should().BeTrue();
        query.Check(["A", "C"]).Should().BeTrue();
        query.Check(["B", "C"]).Should().BeFalse();
        query.Check(["A"]).Should().BeFalse();
        query.Check(["B"]).Should().BeFalse();
        query.Check(["C"]).Should().BeFalse();
    }
    
    [Fact]
    public void Check_PrecedenceDifference3_ReturnsCorrectly() {
        TagQuery query = new("A & !(B | C)");

        query.Check(["A", "B", "C"]).Should().BeFalse();
        query.Check(["A", "B"]).Should().BeFalse();
        query.Check(["A", "C"]).Should().BeFalse();
        query.Check(["B", "C"]).Should().BeFalse();
        query.Check(["A"]).Should().BeTrue();
        query.Check(["B"]).Should().BeFalse();
        query.Check(["C"]).Should().BeFalse();
    }
}