using SqlParser.Nodes;

namespace SqlParser;

// ReSharper disable UnusedMember.Local
public class Interpreter : NodeVisitor
{
    private static class InterpreterHelper
    {
        public static bool IsFLoat(object o)
        {
            return o is float;
        }
    }

    private readonly Parser _parser;
    private readonly Dictionary<string, object?> _globalScope = new();

    public Interpreter(Parser parser)
    {
        _parser = parser;
    }

    public Dictionary<string, object?> Evaluate()
    {
        var tree = _parser.Parse();

        Visit(tree);
        return _globalScope;
    }

    private object? VisitUnaryOperator(UnaryOperator node)
    {
        switch (node.Operator.Type)
        {
            case TokenType.Plus:
                return Visit(node.Child);
            case TokenType.Minus:
                var o = Visit(node.Child);
                if (o is null) return o;
                if (o is float f) return -f;
                if (o is int i) return -i;

                return o;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private object? VisitBinaryOperator(BinaryOperator node)
    {
        var l = Visit(node.Left);
        var r = Visit(node.Right);
        if (l is null && r is null) return null;

        switch (node.Operator.Type)
        {
            case TokenType.Plus:
                if (l is float lfp) return lfp + float.Parse(r!.ToString()!);
                if (r is float rfp) return float.Parse(l!.ToString()!) + rfp;

                return int.Parse(l!.ToString()!) + int.Parse(r!.ToString()!);
            case TokenType.Minus:
                if (l is float lfm) return lfm - float.Parse(r!.ToString()!);
                if (r is float rfm) return float.Parse(l!.ToString()!) + rfm;

                return int.Parse(l!.ToString()!) - int.Parse(r!.ToString()!);
            case TokenType.Multiply:
                if (l is float lfmu) return lfmu * float.Parse(r!.ToString()!);
                if (r is float rfmu) return float.Parse(l!.ToString()!) + rfmu;

                return int.Parse(l!.ToString()!) * int.Parse(r!.ToString()!);
            case TokenType.IntegerDivide:
                return int.Parse(l!.ToString()!) / int.Parse(r!.ToString()!);
            case TokenType.RealDivide:
                return float.Parse(l!.ToString()!) + float.Parse(r!.ToString()!);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private object? VisitNumberNode(NumberNode node)
    {
        return node.Token.ValueAsInt ?? node.Token.ValueAsFloat;
    }

    private object? VisitNoOpNode(NoOpNode node)
    {
        // No operation to evaluate.

        return null;
    }

    private object? VisitCompoundNode(CompoundNode node)
    {
        foreach (var childNode in node.Children)
        {
            Visit(childNode);
        }

        return null;
    }

    private object? VisitAssignNode(AssignNode node)
    {
        _globalScope.Add(node.Left.GetValue(), Visit(node.Right));

        return null;
    }

    private object? VisitVariableNode(VariableNode node)
    {
        if (!_globalScope.TryGetValue(node.Value, out var result))
        {
            throw new InvalidOperationException($"{node.Value} is not defined");
        }

        return result;
    }

    private object? VisitBlockNode(BlockNode node)
    {
        foreach (var declaration in node.Declarations)
        {
            Visit(declaration);
        }
        
        Visit(node.CompoundStatement);

        return null;
    }

    private object? VisitProgramNode(ProgramNode node)
    {
        return Visit(node.Block);
    }

    private int VisitVarDeclNode(VarDeclNode declNode)
    {
        // Do nothing
        return 0;
    }
}