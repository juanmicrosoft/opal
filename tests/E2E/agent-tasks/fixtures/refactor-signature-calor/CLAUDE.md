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

### Signature Changes
When changing a function signature:
1. **Add parameter**: Add a new `§I{type:name}` line
2. **Remove parameter**: Remove the `§I{...}` line
3. **Change return type**: Update the `§O{type}` line
4. **Reorder parameters**: Reorder the `§I{...}` lines
5. IMPORTANT: Keep the function ID unchanged!

### Updating Call Sites
After signature changes, update all callers:
```
§C{FunctionName}
  §A newArg1
  §A newArg2
  §A newArg3
§/C
```

The `§A` arguments must match the new parameter order.

### Type Mappings
| C# | Calor |
|----|-------|
| `int` | `i32` |
| `string` | `str` |
| `bool` | `bool` |
| `int?` / `Option<int>` | `Option<i32>` |

### Contracts
Update contracts when signature changes:
- If new parameter added, may need new preconditions
- If return type changes, update postconditions
