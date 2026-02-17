namespace RenameExamples;

public class Calculator
{
    // Method with parameter to rename
    // Postcondition: result == val * 2
    public int Calculate(int val)
    {
        return val * 2;
    }

    // Method with validation referencing parameter
    // Precondition: num >= 0
    // Precondition: num <= max
    // Postcondition: result <= max
    public int ValidatedOp(int num, int max)
    {
        if (num < 0 || num > max)
            throw new ArgumentOutOfRangeException(nameof(num));
        return num;
    }

    // Method with local variable shadowing considerations
    public int OuterCalc(int x)
    {
        var inner = x * 2;
        return x + inner;
    }
}
