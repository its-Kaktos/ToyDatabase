using SqlParser.Nodes;

namespace SqlParser;

// ReSharper disable UnusedMember.Local
public class Interpreter : NodeVisitor
{
    private readonly Parser _parser;
    private readonly Dictionary<string, int> _globalScope = new();

    public Interpreter(Parser parser)
    {
        _parser = parser;
    }

    public int Evaluate()
    {
        var tree = _parser.Parse();

        return Visit(tree);
    }
    
    private int VisitUnaryOperator(UnaryOperator node)
    {
        return node.Operator.Type switch
        {
            TokenType.Plus => Visit(node.Child),
            TokenType.Minus => -Visit(node.Child),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
    private int VisitBinaryOperator(BinaryOperator node)
    {
        return node.Operator.Type switch
        {
            TokenType.Plus => Visit(node.Left) + Visit(node.Right),
            TokenType.Minus => Visit(node.Left) - Visit(node.Right),
            TokenType.Multiply => Visit(node.Left) * Visit(node.Right),
            TokenType.Divide => Visit(node.Left) / Visit(node.Right),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private int VisitNumberNode(NumberNode node)
    {
        return node.Token.ValueAsInt!.Value;
    }

    private void VisitNoOpNode(NoOpNode node)
    {
        // No operation to evaluate.
    }
    
    private void VisitCompoundNode(CompoundNode node)
    {
        foreach (var childNode in node.Children)
        {
            Visit(childNode);
        }
    }

    private void VisitAssignNode(AssignNode node)
    {
        _globalScope.Add(node.Left.GetValue(), Visit(node.Right));
    }
    
    private string VisitVariableNode(VariableNode node)
    {
        if (_globalScope.TryGetValue(node.Value, out _))
        {
            throw new InvalidOperationException($"{node.Value} is already defiend");
        }
        
        return node.Value;
    }
}