## C# Refactoring Reference

This is a C# project. Write code in `.cs` files.

### Contract Comments
C# doesn't have built-in contracts, so use comments to document them:

**Preconditions**: What must be true BEFORE the method runs
```csharp
// Precondition: x >= 0
// Precondition: index < length
```

**Postconditions**: What is guaranteed AFTER the method runs
```csharp
// Postcondition: result >= 0
// Postcondition: result == x * x
```

### Adding Validation
For runtime enforcement, add guard clauses:
```csharp
public int Sqrt(int x)
{
    // Precondition: x >= 0
    if (x < 0)
        throw new ArgumentOutOfRangeException(nameof(x), "x must be non-negative");
    // Implementation
}
```

### Effect Documentation
Document side effects in comments:
```csharp
// Effect: Writes to console
// Effect: Reads from file system
public void PrintValue(int x)
{
    Console.WriteLine(x);
}
```

### Code Conventions
- Add contract comments above the method
- Add guard clauses at the start of the method body
- Use `ArgumentOutOfRangeException` for range violations
- Use `ArgumentNullException` for null violations
- Use `ArgumentException` for other validation failures
