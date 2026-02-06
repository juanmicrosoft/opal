namespace Calor.Compiler.IR;

/// <summary>
/// Calor Normal Form (CNF) type enumeration.
/// All CNF nodes carry explicit type information.
/// </summary>
public enum CnfType
{
    Void,
    Bool,
    Int,      // i32
    Long,     // i64
    Float,    // f32
    Double,   // f64
    String,
    Object,   // Reference types
    Array,    // Array types
    Option,   // Option<T>
    Result,   // Result<T,E>
}

/// <summary>
/// Base class for all CNF nodes.
/// </summary>
public abstract class CnfNode
{
    public abstract T Accept<T>(ICnfVisitor<T> visitor);
}

/// <summary>
/// Base class for CNF expressions (always atomic).
/// </summary>
public abstract class CnfExpression : CnfNode
{
    public abstract CnfType Type { get; }
}

/// <summary>
/// Base class for CNF statements.
/// </summary>
public abstract class CnfStatement : CnfNode
{
}

// ============================================================
// EXPRESSIONS (Atomic)
// ============================================================

/// <summary>
/// Represents a constant literal value.
/// </summary>
public sealed class CnfLiteral : CnfExpression
{
    public object Value { get; }
    public override CnfType Type { get; }

    public CnfLiteral(object value, CnfType type)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
        Type = type;
    }

    public override T Accept<T>(ICnfVisitor<T> visitor) => visitor.Visit(this);

    public override string ToString() => Type switch
    {
        CnfType.String => $"\"{Value}\"",
        CnfType.Bool => Value.ToString()?.ToLowerInvariant() ?? "false",
        _ => Value.ToString() ?? "null"
    };
}

/// <summary>
/// References a variable by name.
/// </summary>
public sealed class CnfVariableRef : CnfExpression
{
    public string Name { get; }
    public override CnfType Type { get; }

    public CnfVariableRef(string name, CnfType type)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Type = type;
    }

    public override T Accept<T>(ICnfVisitor<T> visitor) => visitor.Visit(this);

    public override string ToString() => Name;
}

/// <summary>
/// Binary operators in CNF (excludes && and || which are control flow).
/// </summary>
public enum CnfBinaryOperator
{
    // Arithmetic
    Add,
    Subtract,
    Multiply,
    Divide,
    Modulo,
    Power,

    // Comparison
    Equal,
    NotEqual,
    LessThan,
    LessOrEqual,
    GreaterThan,
    GreaterOrEqual,

    // Bitwise
    BitwiseAnd,
    BitwiseOr,
    BitwiseXor,
    LeftShift,
    RightShift,
}

/// <summary>
/// Binary operation on two atomic operands.
/// Note: Logical AND/OR are NOT binary ops - they're lowered to control flow.
/// </summary>
public sealed class CnfBinaryOp : CnfExpression
{
    public CnfBinaryOperator Operator { get; }
    public CnfExpression Left { get; }
    public CnfExpression Right { get; }
    public override CnfType Type { get; }

    public CnfBinaryOp(CnfBinaryOperator op, CnfExpression left, CnfExpression right, CnfType resultType)
    {
        Operator = op;
        Left = left ?? throw new ArgumentNullException(nameof(left));
        Right = right ?? throw new ArgumentNullException(nameof(right));
        Type = resultType;
    }

    public override T Accept<T>(ICnfVisitor<T> visitor) => visitor.Visit(this);

    public override string ToString() => $"({Left} {OperatorSymbol} {Right})";

    private string OperatorSymbol => Operator switch
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
}

/// <summary>
/// Unary operators in CNF.
/// </summary>
public enum CnfUnaryOperator
{
    Negate,      // -
    Not,         // !
    BitwiseNot,  // ~
}

/// <summary>
/// Unary operation on an atomic operand.
/// </summary>
public sealed class CnfUnaryOp : CnfExpression
{
    public CnfUnaryOperator Operator { get; }
    public CnfExpression Operand { get; }
    public override CnfType Type { get; }

    public CnfUnaryOp(CnfUnaryOperator op, CnfExpression operand, CnfType resultType)
    {
        Operator = op;
        Operand = operand ?? throw new ArgumentNullException(nameof(operand));
        Type = resultType;
    }

    public override T Accept<T>(ICnfVisitor<T> visitor) => visitor.Visit(this);

    public override string ToString() => $"{OperatorSymbol}{Operand}";

    private string OperatorSymbol => Operator switch
    {
        CnfUnaryOperator.Negate => "-",
        CnfUnaryOperator.Not => "!",
        CnfUnaryOperator.BitwiseNot => "~",
        _ => "?"
    };
}

/// <summary>
/// Function call with atomic arguments.
/// </summary>
public sealed class CnfCall : CnfExpression
{
    public string FunctionName { get; }
    public IReadOnlyList<CnfExpression> Arguments { get; }
    public override CnfType Type { get; }

    public CnfCall(string functionName, IReadOnlyList<CnfExpression> arguments, CnfType returnType)
    {
        FunctionName = functionName ?? throw new ArgumentNullException(nameof(functionName));
        Arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
        Type = returnType;
    }

    public override T Accept<T>(ICnfVisitor<T> visitor) => visitor.Visit(this);

    public override string ToString() => $"{FunctionName}({string.Join(", ", Arguments)})";
}

/// <summary>
/// Kind of type conversion.
/// </summary>
public enum ConversionKind
{
    Implicit,  // Safe widening conversions
    Explicit,  // May lose precision
    Checked,   // Throws on overflow
}

/// <summary>
/// Explicit type conversion.
/// </summary>
public sealed class CnfConversion : CnfExpression
{
    public CnfExpression Operand { get; }
    public CnfType FromType { get; }
    public override CnfType Type { get; }
    public ConversionKind Kind { get; }

    public CnfConversion(CnfExpression operand, CnfType fromType, CnfType toType, ConversionKind kind)
    {
        Operand = operand ?? throw new ArgumentNullException(nameof(operand));
        FromType = fromType;
        Type = toType;
        Kind = kind;
    }

    public override T Accept<T>(ICnfVisitor<T> visitor) => visitor.Visit(this);

    public override string ToString() => $"({Type}){Operand}";
}

// ============================================================
// STATEMENTS
// ============================================================

/// <summary>
/// Assigns a value to a variable.
/// </summary>
public sealed class CnfAssign : CnfStatement
{
    public string Target { get; }
    public CnfExpression Value { get; }
    public CnfType TargetType { get; }

    public CnfAssign(string target, CnfExpression value, CnfType targetType)
    {
        Target = target ?? throw new ArgumentNullException(nameof(target));
        Value = value ?? throw new ArgumentNullException(nameof(value));
        TargetType = targetType;
    }

    public override T Accept<T>(ICnfVisitor<T> visitor) => visitor.Visit(this);

    public override string ToString() => $"{Target} = {Value}";
}

/// <summary>
/// A sequence of statements executed in order.
/// </summary>
public sealed class CnfSequence : CnfStatement
{
    public IReadOnlyList<CnfStatement> Statements { get; }

    public CnfSequence(IReadOnlyList<CnfStatement> statements)
    {
        Statements = statements ?? throw new ArgumentNullException(nameof(statements));
    }

    public CnfSequence(params CnfStatement[] statements)
        : this((IReadOnlyList<CnfStatement>)statements)
    {
    }

    public override T Accept<T>(ICnfVisitor<T> visitor) => visitor.Visit(this);
}

/// <summary>
/// Conditional branch to one of two labels.
/// </summary>
public sealed class CnfBranch : CnfStatement
{
    public CnfExpression Condition { get; }
    public string TrueLabel { get; }
    public string FalseLabel { get; }

    public CnfBranch(CnfExpression condition, string trueLabel, string falseLabel)
    {
        Condition = condition ?? throw new ArgumentNullException(nameof(condition));
        TrueLabel = trueLabel ?? throw new ArgumentNullException(nameof(trueLabel));
        FalseLabel = falseLabel ?? throw new ArgumentNullException(nameof(falseLabel));
    }

    public override T Accept<T>(ICnfVisitor<T> visitor) => visitor.Visit(this);

    public override string ToString() => $"branch {Condition} -> {TrueLabel}, {FalseLabel}";
}

/// <summary>
/// Returns a value from a function.
/// </summary>
public sealed class CnfReturn : CnfStatement
{
    public CnfExpression? Value { get; }

    public CnfReturn(CnfExpression? value = null)
    {
        Value = value;
    }

    public override T Accept<T>(ICnfVisitor<T> visitor) => visitor.Visit(this);

    public override string ToString() => Value != null ? $"return {Value}" : "return";
}

/// <summary>
/// Throws an exception.
/// </summary>
public sealed class CnfThrow : CnfStatement
{
    public CnfExpression Exception { get; }

    public CnfThrow(CnfExpression exception)
    {
        Exception = exception ?? throw new ArgumentNullException(nameof(exception));
    }

    public override T Accept<T>(ICnfVisitor<T> visitor) => visitor.Visit(this);

    public override string ToString() => $"throw {Exception}";
}

/// <summary>
/// A labeled point in the code.
/// </summary>
public sealed class CnfLabel : CnfStatement
{
    public string Name { get; }

    public CnfLabel(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    public override T Accept<T>(ICnfVisitor<T> visitor) => visitor.Visit(this);

    public override string ToString() => $"{Name}:";
}

/// <summary>
/// Unconditional jump to a label.
/// </summary>
public sealed class CnfGoto : CnfStatement
{
    public string Target { get; }

    public CnfGoto(string target)
    {
        Target = target ?? throw new ArgumentNullException(nameof(target));
    }

    public override T Accept<T>(ICnfVisitor<T> visitor) => visitor.Visit(this);

    public override string ToString() => $"goto {Target}";
}

/// <summary>
/// Try/catch/finally block.
/// </summary>
public sealed class CnfTry : CnfStatement
{
    public CnfSequence TryBody { get; }
    public IReadOnlyList<CnfCatchClause> CatchClauses { get; }
    public CnfSequence? FinallyBody { get; }

    public CnfTry(
        CnfSequence tryBody,
        IReadOnlyList<CnfCatchClause> catchClauses,
        CnfSequence? finallyBody = null)
    {
        TryBody = tryBody ?? throw new ArgumentNullException(nameof(tryBody));
        CatchClauses = catchClauses ?? throw new ArgumentNullException(nameof(catchClauses));
        FinallyBody = finallyBody;
    }

    public override T Accept<T>(ICnfVisitor<T> visitor) => visitor.Visit(this);
}

/// <summary>
/// A catch clause within a try statement.
/// </summary>
public sealed class CnfCatchClause
{
    public string? ExceptionType { get; }
    public string? VariableName { get; }
    public CnfSequence Body { get; }

    public CnfCatchClause(string? exceptionType, string? variableName, CnfSequence body)
    {
        ExceptionType = exceptionType;
        VariableName = variableName;
        Body = body ?? throw new ArgumentNullException(nameof(body));
    }
}

// ============================================================
// TOP-LEVEL CONSTRUCTS
// ============================================================

/// <summary>
/// A function in CNF form.
/// </summary>
public sealed class CnfFunction
{
    public string Id { get; }
    public string Name { get; }
    public IReadOnlyList<CnfParameter> Parameters { get; }
    public CnfType ReturnType { get; }
    public CnfSequence Body { get; }

    public CnfFunction(
        string id,
        string name,
        IReadOnlyList<CnfParameter> parameters,
        CnfType returnType,
        CnfSequence body)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        ReturnType = returnType;
        Body = body ?? throw new ArgumentNullException(nameof(body));
    }
}

/// <summary>
/// A parameter in CNF form.
/// </summary>
public sealed class CnfParameter
{
    public string Name { get; }
    public CnfType Type { get; }

    public CnfParameter(string name, CnfType type)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Type = type;
    }
}

/// <summary>
/// A complete module in CNF form.
/// </summary>
public sealed class CnfModule
{
    public string Id { get; }
    public string Name { get; }
    public Version? SemanticsVersion { get; }
    public IReadOnlyList<CnfFunction> Functions { get; }

    public CnfModule(
        string id,
        string name,
        Version? semanticsVersion,
        IReadOnlyList<CnfFunction> functions)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        SemanticsVersion = semanticsVersion;
        Functions = functions ?? throw new ArgumentNullException(nameof(functions));
    }
}
