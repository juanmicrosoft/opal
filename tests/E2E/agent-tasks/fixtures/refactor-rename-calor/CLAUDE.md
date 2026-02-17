## Calor Syntax Reference

This is a Calor project. Write code in `.calr` files.

### Function Syntax
```
§F{id:Name:pub}
  §I{type:name}      // Input parameter
  §O{type}           // Output/return type
  §Q (condition)     // Precondition (requires)
  §S (condition)     // Postcondition (ensures)
  §R expression      // Return
§/F{id}
```

### Variable Declaration
```
§V{id:name:type} initialValue
```

### Renaming Guidelines
- When renaming a parameter, update ALL references to it:
  - In the function body
  - In preconditions (§Q)
  - In postconditions (§S)
- Unique IDs (f001, v001, etc.) should NOT change during rename
- Only the human-readable name changes

### Type Mappings
| C# | Calor |
|----|-------|
| `int` | `i32` |
| `long` | `i64` |
| `string` | `str` |
| `bool` | `bool` |

### Expression Syntax (Lisp-style, prefix notation)
- Arithmetic: `(+ a b)`, `(- a b)`, `(* a b)`, `(/ a b)`
- Comparison: `(== a b)`, `(!= a b)`, `(< a b)`, `(> a b)`, `(<= a b)`, `(>= a b)`
- Logical: `(and a b)`, `(or a b)`, `(not a)`

### Contracts
- Precondition: `§Q (condition)` - refers to parameters by name
- Postcondition: `§S (condition)` - use `result` for return value
