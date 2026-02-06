namespace Calor.Compiler.IR;

/// <summary>
/// Visitor interface for CNF nodes with return values.
/// </summary>
public interface ICnfVisitor<T>
{
    // Expressions
    T Visit(CnfLiteral node);
    T Visit(CnfVariableRef node);
    T Visit(CnfBinaryOp node);
    T Visit(CnfUnaryOp node);
    T Visit(CnfCall node);
    T Visit(CnfConversion node);

    // Statements
    T Visit(CnfAssign node);
    T Visit(CnfSequence node);
    T Visit(CnfBranch node);
    T Visit(CnfReturn node);
    T Visit(CnfThrow node);
    T Visit(CnfLabel node);
    T Visit(CnfGoto node);
    T Visit(CnfTry node);
}

/// <summary>
/// Base class for CNF visitors with default implementations that visit children.
/// </summary>
public abstract class CnfVisitorBase<T> : ICnfVisitor<T>
{
    public virtual T Visit(CnfLiteral node) => default!;
    public virtual T Visit(CnfVariableRef node) => default!;

    public virtual T Visit(CnfBinaryOp node)
    {
        node.Left.Accept(this);
        node.Right.Accept(this);
        return default!;
    }

    public virtual T Visit(CnfUnaryOp node)
    {
        node.Operand.Accept(this);
        return default!;
    }

    public virtual T Visit(CnfCall node)
    {
        foreach (var arg in node.Arguments)
        {
            arg.Accept(this);
        }
        return default!;
    }

    public virtual T Visit(CnfConversion node)
    {
        node.Operand.Accept(this);
        return default!;
    }

    public virtual T Visit(CnfAssign node)
    {
        node.Value.Accept(this);
        return default!;
    }

    public virtual T Visit(CnfSequence node)
    {
        foreach (var stmt in node.Statements)
        {
            stmt.Accept(this);
        }
        return default!;
    }

    public virtual T Visit(CnfBranch node)
    {
        node.Condition.Accept(this);
        return default!;
    }

    public virtual T Visit(CnfReturn node)
    {
        node.Value?.Accept(this);
        return default!;
    }

    public virtual T Visit(CnfThrow node)
    {
        node.Exception.Accept(this);
        return default!;
    }

    public virtual T Visit(CnfLabel node) => default!;
    public virtual T Visit(CnfGoto node) => default!;

    public virtual T Visit(CnfTry node)
    {
        node.TryBody.Accept(this);
        foreach (var clause in node.CatchClauses)
        {
            clause.Body.Accept(this);
        }
        node.FinallyBody?.Accept(this);
        return default!;
    }
}

/// <summary>
/// Visitor that validates CNF invariants.
/// </summary>
public sealed class CnfValidator : CnfVisitorBase<bool>
{
    private readonly HashSet<string> _labels = new();
    private readonly HashSet<string> _gotos = new();
    private readonly HashSet<string> _variables = new();
    private readonly List<string> _errors = new();

    public IReadOnlyList<string> Errors => _errors;
    public bool IsValid => _errors.Count == 0;

    public void ValidateFunction(CnfFunction function)
    {
        _labels.Clear();
        _gotos.Clear();
        _variables.Clear();

        // Add parameters as defined variables
        foreach (var param in function.Parameters)
        {
            _variables.Add(param.Name);
        }

        // First pass: collect all labels
        CollectLabels(function.Body);

        // Second pass: validate
        function.Body.Accept(this);

        // Check all gotos have labels
        foreach (var gotoTarget in _gotos)
        {
            if (!_labels.Contains(gotoTarget))
            {
                _errors.Add($"Goto target '{gotoTarget}' has no corresponding label");
            }
        }
    }

    private void CollectLabels(CnfSequence sequence)
    {
        foreach (var stmt in sequence.Statements)
        {
            if (stmt is CnfLabel label)
            {
                _labels.Add(label.Name);
            }
            else if (stmt is CnfTry tryStmt)
            {
                CollectLabels(tryStmt.TryBody);
                foreach (var clause in tryStmt.CatchClauses)
                {
                    CollectLabels(clause.Body);
                }
                if (tryStmt.FinallyBody != null)
                {
                    CollectLabels(tryStmt.FinallyBody);
                }
            }
        }
    }

    public override bool Visit(CnfBinaryOp node)
    {
        // Validate operands are atomic
        if (!IsAtomic(node.Left))
        {
            _errors.Add($"Binary operation left operand must be atomic: {node.Left}");
        }
        if (!IsAtomic(node.Right))
        {
            _errors.Add($"Binary operation right operand must be atomic: {node.Right}");
        }
        return base.Visit(node);
    }

    public override bool Visit(CnfUnaryOp node)
    {
        if (!IsAtomic(node.Operand))
        {
            _errors.Add($"Unary operation operand must be atomic: {node.Operand}");
        }
        return base.Visit(node);
    }

    public override bool Visit(CnfCall node)
    {
        foreach (var arg in node.Arguments)
        {
            if (!IsAtomic(arg))
            {
                _errors.Add($"Call argument must be atomic: {arg}");
            }
        }
        return base.Visit(node);
    }

    public override bool Visit(CnfAssign node)
    {
        _variables.Add(node.Target);
        return base.Visit(node);
    }

    public override bool Visit(CnfVariableRef node)
    {
        if (!_variables.Contains(node.Name))
        {
            _errors.Add($"Variable '{node.Name}' used before definition");
        }
        return base.Visit(node);
    }

    public override bool Visit(CnfBranch node)
    {
        _gotos.Add(node.TrueLabel);
        _gotos.Add(node.FalseLabel);
        return base.Visit(node);
    }

    public override bool Visit(CnfGoto node)
    {
        _gotos.Add(node.Target);
        return base.Visit(node);
    }

    private static bool IsAtomic(CnfExpression expr)
    {
        return expr is CnfLiteral or CnfVariableRef;
    }
}

/// <summary>
/// Pretty-prints CNF for debugging.
/// </summary>
public sealed class CnfPrettyPrinter : ICnfVisitor<string>
{
    private int _indent = 0;

    private string Indent => new string(' ', _indent * 2);

    public string Print(CnfFunction function)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"{function.Name}({string.Join(", ", function.Parameters.Select(p => $"{p.Type} {p.Name}"))}) -> {function.ReturnType}:");
        _indent++;
        sb.Append(function.Body.Accept(this));
        _indent--;
        return sb.ToString();
    }

    public string Visit(CnfLiteral node) => node.ToString();
    public string Visit(CnfVariableRef node) => node.Name;
    public string Visit(CnfBinaryOp node) => $"({node.Left.Accept(this)} {GetOpSymbol(node.Operator)} {node.Right.Accept(this)})";
    public string Visit(CnfUnaryOp node) => $"{GetOpSymbol(node.Operator)}{node.Operand.Accept(this)}";
    public string Visit(CnfCall node) => $"{node.FunctionName}({string.Join(", ", node.Arguments.Select(a => a.Accept(this)))})";
    public string Visit(CnfConversion node) => $"({node.Type}){node.Operand.Accept(this)}";

    public string Visit(CnfAssign node) => $"{Indent}{node.Target} = {node.Value.Accept(this)}\n";

    public string Visit(CnfSequence node)
    {
        var sb = new System.Text.StringBuilder();
        foreach (var stmt in node.Statements)
        {
            sb.Append(stmt.Accept(this));
        }
        return sb.ToString();
    }

    public string Visit(CnfBranch node) => $"{Indent}branch {node.Condition.Accept(this)} -> {node.TrueLabel}, {node.FalseLabel}\n";
    public string Visit(CnfReturn node) => node.Value != null ? $"{Indent}return {node.Value.Accept(this)}\n" : $"{Indent}return\n";
    public string Visit(CnfThrow node) => $"{Indent}throw {node.Exception.Accept(this)}\n";
    public string Visit(CnfLabel node) => $"{node.Name}:\n";
    public string Visit(CnfGoto node) => $"{Indent}goto {node.Target}\n";

    public string Visit(CnfTry node)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"{Indent}try:");
        _indent++;
        sb.Append(node.TryBody.Accept(this));
        _indent--;
        foreach (var clause in node.CatchClauses)
        {
            var catchLine = clause.ExceptionType != null
                ? $"catch ({clause.ExceptionType} {clause.VariableName}):"
                : "catch:";
            sb.AppendLine($"{Indent}{catchLine}");
            _indent++;
            sb.Append(clause.Body.Accept(this));
            _indent--;
        }
        if (node.FinallyBody != null)
        {
            sb.AppendLine($"{Indent}finally:");
            _indent++;
            sb.Append(node.FinallyBody.Accept(this));
            _indent--;
        }
        return sb.ToString();
    }

    private static string GetOpSymbol(CnfBinaryOperator op) => op switch
    {
        CnfBinaryOperator.Add => "+",
        CnfBinaryOperator.Subtract => "-",
        CnfBinaryOperator.Multiply => "*",
        CnfBinaryOperator.Divide => "/",
        CnfBinaryOperator.Modulo => "%",
        CnfBinaryOperator.Power => "**",
        CnfBinaryOperator.Equal => "==",
        CnfBinaryOperator.NotEqual => "!=",
        CnfBinaryOperator.LessThan => "<",
        CnfBinaryOperator.LessOrEqual => "<=",
        CnfBinaryOperator.GreaterThan => ">",
        CnfBinaryOperator.GreaterOrEqual => ">=",
        CnfBinaryOperator.BitwiseAnd => "&",
        CnfBinaryOperator.BitwiseOr => "|",
        CnfBinaryOperator.BitwiseXor => "^",
        CnfBinaryOperator.LeftShift => "<<",
        CnfBinaryOperator.RightShift => ">>",
        _ => "?"
    };

    private static string GetOpSymbol(CnfUnaryOperator op) => op switch
    {
        CnfUnaryOperator.Negate => "-",
        CnfUnaryOperator.Not => "!",
        CnfUnaryOperator.BitwiseNot => "~",
        _ => "?"
    };
}
