## Calor Syntax Reference

This is a Calor project. Write code in `.calr` files.

### Function Syntax
```
§F{id:Name:pub}
  §I{type:name}      // Input parameter
  §O{type}           // Output/return type
  §Q (condition)     // Precondition (requires)
  §S (condition)     // Postcondition (ensures)
  §E{effects}        // Effects declaration
  §R expression      // Return
§/F{id}
```

### Type Mappings
| C# | Calor |
|----|-------|
| `int` | `i32` |
| `long` | `i64` |
| `string` | `str` |
| `bool` | `bool` |
| `void` | `void` |

### Expression Syntax (Lisp-style, prefix notation)
- Arithmetic: `(+ a b)`, `(- a b)`, `(* a b)`, `(/ a b)`, `(% a b)`
- Comparison: `(== a b)`, `(!= a b)`, `(< a b)`, `(> a b)`, `(<= a b)`, `(>= a b)`
- Logical: `(and a b)`, `(or a b)`, `(not a)`
- Ternary/Conditional: `(? condition then-expr else-expr)`

### Contracts
- Precondition: `§Q (>= x 0)` - use `§Q` for requires
- Postcondition: `§S (>= result 0)` - use `§S` for ensures, `result` refers to return value

### Effects Declaration
- `cw` = console write
- `cr` = console read
- `fs` = file system access
- `net` = network access

### Method Calls
```
§C{MethodName}
  §A argument
§/C
```

### Unique IDs
Each function has a unique ID (e.g., f001, f002). When extracting new functions:
- Original functions keep their IDs
- New extracted functions should use the next available ID (f005, f006, etc.)
- IDs enable stable references across refactorings
