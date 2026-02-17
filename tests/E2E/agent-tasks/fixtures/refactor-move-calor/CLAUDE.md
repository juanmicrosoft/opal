## Calor Syntax Reference

This is a Calor project with multiple modules. Files are in `src/`.

### Module Structure
```
§M{moduleId:ModuleName}
  // functions here
§/M{moduleId}
```

### Function Syntax
```
§F{id:Name:pub}
  §I{type:name}      // Input parameter
  §O{type}           // Output/return type
  §Q (condition)     // Precondition
  §S (condition)     // Postcondition
  §E{effects}        // Effects
  §R expression      // Return
§/F{id}
```

### Moving Functions Between Modules
When moving a function to another module:
1. PRESERVE the function's unique ID - this is critical
2. Update any callers to use qualified name if needed
3. Contracts (§Q, §S) should move with the function
4. Effects (§E) should move with the function
5. The ID enables stable references even after the move

### Cross-Module Calls
After moving, callers may need to reference the new module:
```
§C{ModuleName.FunctionName}
  §A arg
§/C
```

### Type Mappings
| C# | Calor |
|----|-------|
| `int` | `i32` |
| `string` | `str` |
| `void` | `void` |
