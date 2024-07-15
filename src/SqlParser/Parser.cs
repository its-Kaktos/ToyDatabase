using System.ComponentModel.Design;
using SqlParser.Nodes;

namespace SqlParser;

/*
program : PROGRAM variable SEMI block DOT

block : declarations compound_statement

declarations : VAR (variable_declaration SEMI)+
               | empty

variable_declaration : ID (COMMA ID)* COLON type_spec

type_spec : INTEGER | REAL

compound_statement : BEGIN statement_list END

statement_list : statement
               | statement SEMI statement_list

statement : compound_statement
           | assignment_statement
           | empty

assignment_statement : variable ASSIGN expr

empty :

expr : term ((PLUS | MINUS) term)*

term : factor ((MUL | INTEGER_DIV | FLOAT_DIV) factor)*

factor : PLUS factor
       | MINUS factor
       | INTEGER_CONST
       | REAL_CONST
       | LPAREN expr RPAREN
       | variable

variable: ID
 */
public class Parser
{
    private readonly Lexer _lexer;
    private Token _currentToken;

    public Parser(Lexer lexer)
    {
        _lexer = lexer;
        _currentToken = lexer.NextToken();
    }

    public IAST Parse()
    {
        return GetProgram();
    }

    // program : PROGRAM variable SEMI block DOT
    private IAST GetProgram()
    {
        Eat(TokenType.Program);
        var id = GetVariable();
        Eat(TokenType.Semi);

        var block = GetBlock();
        Eat(TokenType.Dot);

        return new ProgramNode(new Token(TokenType.Program), id, block);
    }

    // block : declarations compound_statement
    private IAST GetBlock()
    {
        var declarations = GetDeclarations();

        return new BlockNode(declarations, GetCompoundStatement());
    }

    // declarations : VAR (variable_declaration SEMI)+
    // | empty
    private List<IAST> GetDeclarations()
    {
        var result = new List<IAST>();
        if (_currentToken.Type is TokenType.VarDecl)
        {
            Eat(TokenType.VarDecl);
            while (_currentToken.Type is TokenType.Id)
            {
                result.AddRange(GetVariableDeclaration());
                Eat(TokenType.Semi);
            }
        }

        return result;
    }

    // variable_declaration : ID (COMMA ID)* COLON type_spec
    private List<VarDeclNode> GetVariableDeclaration()
    {
        var ids = new List<Token>() { _currentToken };
        Eat(TokenType.Id);

        while (_currentToken.Type is TokenType.Comma)
        {
            Eat(TokenType.Comma);
            ids.Add(_currentToken);
            Eat(TokenType.Id);
        }

        Eat(TokenType.Colon);
        var type = GetTypeSpec();

        return ids.Select(x => new VarDeclNode(new VariableNode(x), type)).ToList();
    }

    // type_spec : INTEGER | REAL
    private IAST GetTypeSpec()
    {
        var current = _currentToken;
        switch (_currentToken.Type)
        {
            case TokenType.Integer:
                Eat(TokenType.Integer);
                return new NumberNode(current);
            case TokenType.Real:
                Eat(TokenType.Real);
                return new NumberNode(current);
            default:
                throw new InvalidOperationException("Invalid syntax.");
        }
    }

    // compound_statement : BEGIN statement_list END
    private IAST GetCompoundStatement()
    {
        Eat(TokenType.Begin);
        var nodes = GetStatementList();
        Eat(TokenType.End);

        return new CompoundNode(nodes);
    }

    // statement_list : statement
    // | statement SEMI statement_list
    private List<IAST> GetStatementList()
    {
        var result = new List<IAST> { GetStatement() };

        while (_currentToken.Type == TokenType.Semi)
        {
            Eat(TokenType.Semi);
            result.Add(GetStatement());
        }

        if (_currentToken.Type == TokenType.Id) throw new InvalidOperationException("Invalid syntax");

        return result;
    }

    // statement : compound_statement
    // | assignment_statement
    // | empty
    private IAST GetStatement()
    {
        return _currentToken.Type switch
        {
            TokenType.Begin => GetCompoundStatement(),
            TokenType.Id => GetAssignmentStatement(),
            _ => GetEmpty()
        };
    }

    // assignment_statement : variable ASSIGN expr
    private IAST GetAssignmentStatement()
    {
        var left = GetVariable();
        var op = _currentToken;
        Eat(TokenType.Assign);

        return new AssignNode(left, op, GetExpr());
    }

    // empty :
    private IAST GetEmpty()
    {
        return NoOpNode.Instance;
    }

    // expr : term ( (PLUS | MINUS) term)*
    private IAST GetExpr()
    {
        var node = GetTerm();

        while (_currentToken.Type is TokenType.Plus or TokenType.Minus)
        {
            var op = _currentToken;
            switch (_currentToken.Type)
            {
                case TokenType.Plus:
                    Eat(TokenType.Plus);
                    break;
                case TokenType.Minus:
                    Eat(TokenType.Minus);
                    break;
            }

            node = new BinaryOperator(node, op, GetTerm());
        }

        return node;
    }

    // term : factor ((MUL | INTEGER_DIV | FLOAT_DIV) factor)*
    private IAST GetTerm()
    {
        var node = GetFactor();

        while (_currentToken.Type is TokenType.IntegerDivide or TokenType.Multiply or TokenType.RealDivide)
        {
            var op = _currentToken;
            switch (_currentToken.Type)
            {
                case TokenType.IntegerDivide:
                    Eat(TokenType.IntegerDivide);
                    break;
                case TokenType.RealDivide:
                    Eat(TokenType.RealDivide);
                    break;
                case TokenType.Multiply:
                    Eat(TokenType.Multiply);
                    break;
            }

            node = new BinaryOperator(node, op, GetFactor());
        }

        return node;
    }

    // factor : PLUS factor
    // | MINUS factor
    // | INTEGER_CONST
    // | REAL_CONST
    // | LPAREN expr RPAREN
    // | variable
    private IAST GetFactor()
    {
        switch (_currentToken.Type)
        {
            case TokenType.Plus:
            {
                Eat(TokenType.Plus);
                return new UnaryOperator(new Token(TokenType.Plus), GetFactor());
            }
            case TokenType.Minus:
            {
                Eat(TokenType.Minus);
                return new UnaryOperator(new Token(TokenType.Minus), GetFactor());
            }
            case TokenType.IntegerConst:
            {
                var current = _currentToken;
                Eat(TokenType.IntegerConst);
                return new NumberNode(current);
            }
            case TokenType.RealConst:
            {
                var current = _currentToken;
                Eat(TokenType.RealConst);
                return new NumberNode(current);
            }
            case TokenType.LParen:
            {
                Eat(TokenType.LParen);
                var result = GetExpr();
                Eat(TokenType.RParen);
                return result;
            }
            case TokenType.Id:
                return GetVariable();
            default:
                throw new InvalidOperationException($"{_currentToken.Type} is not a valid Factor");
        }
    }

    // variable: ID
    private IAST GetVariable()
    {
        var result = new VariableNode(_currentToken);
        Eat(TokenType.Id);

        return result;
    }

    private void Eat(TokenType tokenType)
    {
        if (_currentToken.Type != tokenType) throw new InvalidOperationException("Invalid syntax.");

        _currentToken = _lexer.NextToken();
    }
}