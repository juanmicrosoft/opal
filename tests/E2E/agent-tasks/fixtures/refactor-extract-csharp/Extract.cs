namespace ExtractExamples;

public class Calculator
{
    // Simple calculation to extract
    public int Distance(int x, int y)
    {
        return x * x + y * y;
    }

    // Method with validation - extraction should preserve validation
    public int ProcessArray(int index, int length)
    {
        // Precondition: index >= 0
        // Precondition: index < length
        if (index < 0 || index >= length)
            return -1;
        // Postcondition: result >= 0 (when valid)
        return index;
    }

    // Method with effects - extraction should consider side effects
    public int LogAndCompute(int a, int b)
    {
        Console.WriteLine("Computing sum");
        return a + b;
    }

    // Nested calculations - complex extraction
    public int ComputeNested(int x, int y, int z)
    {
        return x * x + y * y + z * z;
    }
}
