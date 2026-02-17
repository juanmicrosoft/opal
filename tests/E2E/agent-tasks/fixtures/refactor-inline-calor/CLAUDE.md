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

### Inlining Guidelines
When inlining a function call:
1. Replace the call with the function body
2. Substitute parameters with actual arguments
3. IMPORTANT: Contracts from inlined function should be considered:
   - Preconditions become assertions at call site
   - Postconditions may strengthen the caller's postconditions
4. Keep unique IDs stable - don't change IDs of remaining functions
5. You may delete the inlined function if it's no longer called elsewhere

### Function Calls
```
§C{FunctionName}
  §A argument1
  §A argument2
§/C
```

### Expression Syntax
- Arithmetic: `(+ a b)`, `(- a b)`, `(* a b)`, `(/ a b)`
- Comparison: `(== a b)`, `(!= a b)`, `(< a b)`, `(> a b)`, `(<= a b)`, `(>= a b)`
- Ternary: `(? condition then else)`

### Contracts
- Precondition: `§Q (condition)`
- Postcondition: `§S (condition)` - use `result` for return value
