# C# Language Skills

You are writing code in **C#**, a modern, object-oriented programming language from Microsoft.

## Core Patterns

### Function Structure

For benchmark tasks, write public static methods in a static class:

```csharp
public static class Functions
{
    public static int FunctionName(int param1, int param2)
    {
        // implementation
        return result;
    }
}
```

### Handling Constraints

When a task specifies constraints like "must only accept positive values" or "must not allow zero", use **guard clauses** with exceptions:

```csharp
public static int SafeDivide(int a, int b)
{
    if (b == 0)
        throw new ArgumentException("Divisor cannot be zero", nameof(b));
    return a / b;
}
```

Common patterns:

| Constraint | C# Implementation |
|------------|-------------------|
| "must be positive" | `if (n <= 0) throw new ArgumentException(...)` |
| "must not be zero" | `if (n == 0) throw new ArgumentException(...)` |
| "must be in range" | `if (n < min \|\| n > max) throw new ArgumentOutOfRangeException(...)` |

### Types

| Type | Description |
|------|-------------|
| `int` | 32-bit signed integer |
| `long` | 64-bit signed integer |
| `bool` | Boolean (true/false) |
| `string` | Text |
| `void` | No return value |

### Control Flow

**If/Else:**
```csharp
if (condition)
    return value1;
else if (condition2)
    return value2;
else
    return value3;
```

**Ternary operator (for simple cases):**
```csharp
return condition ? valueIfTrue : valueIfFalse;
```

**For loop:**
```csharp
for (int i = start; i <= end; i++)
{
    // body
}
```

**While loop:**
```csharp
while (condition)
{
    // body
}
```

## Complete Examples

### Simple Function
```csharp
public static int Square(int n)
{
    return n * n;
}
```

### Function with Validation
```csharp
public static int SafeDivide(int a, int b)
{
    if (b == 0)
        throw new ArgumentException("Cannot divide by zero", nameof(b));
    return a / b;
}
```

### Function with Multiple Validations
```csharp
public static int Clamp(int value, int min, int max)
{
    if (min > max)
        throw new ArgumentException("min cannot exceed max");

    if (value < min)
        return min;
    if (value > max)
        return max;
    return value;
}
```

### Recursive Function
```csharp
public static int Factorial(int n)
{
    if (n < 0)
        throw new ArgumentException("n must be non-negative", nameof(n));

    if (n <= 1)
        return 1;
    return n * Factorial(n - 1);
}
```

### Loop-based Function
```csharp
public static int Power(int baseNum, int exp)
{
    if (exp < 0)
        throw new ArgumentException("Exponent must be non-negative", nameof(exp));

    int result = 1;
    for (int i = 0; i < exp; i++)
    {
        result *= baseNum;
    }
    return result;
}
```

### Boolean Function
```csharp
public static bool IsEven(int n)
{
    return n % 2 == 0;
}
```

### Comparison Function
```csharp
public static int Max(int a, int b)
{
    return a > b ? a : b;
}
```

## Guidelines for Writing C#

1. **Use guard clauses for validation**: Place validation at the start of the method, throw appropriate exceptions for invalid inputs.

2. **Choose appropriate exception types**:
   - `ArgumentException` for invalid argument values
   - `ArgumentNullException` for null arguments
   - `ArgumentOutOfRangeException` for out-of-range values

3. **Keep methods focused**: One method per task with clear parameters and return type.

4. **Use expression-bodied members for simple functions**:
   ```csharp
   public static int Square(int n) => n * n;
   ```

5. **Use meaningful parameter names**: Match the names specified in the task.
