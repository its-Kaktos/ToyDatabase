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

    public Dictionary<string, int> Evaluate()
    {
        var tree = _parser.Parse();

        Visit(tree);
        return _globalScope;
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
            TokenType.IntegerDivide => Visit(node.Left) / Visit(node.Right),
            TokenType.RealDivide => Visit(node.Left) / Visit(node.Right),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private int VisitNumberNode(NumberNode node)
    {
        return node.Token.ValueAsInt!.Value;
    }

    private int VisitNoOpNode(NoOpNode node)
    {
        // No operation to evaluate.

        return 0;
    }
    
    private int VisitCompoundNode(CompoundNode node)
    {
        foreach (var childNode in node.Children)
        {
            Visit(childNode);
        }
        
        return 0;
    }

    private int VisitAssignNode(AssignNode node)
    {
        _globalScope.Add(node.Left.GetValue(), Visit(node.Right));
        
        return 0;
    }
    
    private int VisitVariableNode(VariableNode node)
    {
        if (_globalScope.TryGetValue(node.Value, out _))
        {
            throw new InvalidOperationException($"{node.Value} is already defined");
        }
        
        return 0;
    }
    
    private int VisitBlockNode(BlockNode node)
    {
        foreach (var declaration in node.Declarations)
        {
            Visit(declaration);
        }

        return 0;
    }
    
    private int VisitProgramNode(ProgramNode node)
    {
        return Visit(node.Block);
    }
    
    private int VisitVarDeclNode(VarDeclNode declNode)
    {
        // Do nothing
        return 0;
    }
}