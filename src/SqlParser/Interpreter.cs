using System.Xml;

namespace SqlParser;

public class Interpreter : NodeVisitor
{
    private readonly Parser _parser;

    public Interpreter(Parser parser)
    {
        _parser = parser;
    }

    public int Evaluate()
    {
        var tree = _parser.Parse();

        return Visit(tree);
    }
    
    private int VisitBinaryOperator(BinaryOperator node)
    {
        return node.Operator.Type switch
        {
            TokenType.Plus => Visit(node.Left) + Visit(node.Right),
            TokenType.Minus => Visit(node.Left) - Visit(node.Right),
            TokenType.Multiply => Visit(node.Left) * Visit(node.Right),
            TokenType.Divide => Visit(node.Left) / Visit(node.Right),
            TokenType.Power => (int)Math.Pow(Visit(node.Left), Visit(node.Right)),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private int VisitNumberNode(NumberNode node)
    {
        return node.Token.ValueAsInt!.Value;
    }
}