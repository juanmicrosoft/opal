namespace ContractExamples;

public class Calculator
{
    // Method needing a precondition
    // Note: x should be non-negative for square root
    public int Sqrt(int x)
    {
        return x; // Simplified - should add validation for x >= 0
    }

    // Method needing a postcondition
    // Note: result should always be >= 0
    public int Abs(int x)
    {
        return x < 0 ? -x : x;
    }

    // Method needing array bounds validation
    // Existing: has runtime check but needs explicit contract comments
    public int GetElement(int index, int length)
    {
        if (index >= 0 && index < length)
            return index;
        return -1;
    }

    // Method needing effect documentation
    // Note: writes to console
    public void PrintValue(int x)
    {
        Console.WriteLine(x);
    }

    // Method with existing contracts - new contracts should preserve these
    // Precondition: a >= 0
    // Precondition: b >= 0
    // Postcondition: result >= 0
    public int Add(int a, int b)
    {
        return a + b;
    }
}
