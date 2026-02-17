namespace InlineExamples;

public class Calculator
{
    // Simple helper method to inline
    public int Double(int x)
    {
        return x * 2;
    }

    // Method that calls Double - target for inlining
    public int Calculate(int a, int b)
    {
        return Double(a) + Double(b);
    }

    // Method with validation to inline
    // Precondition: min <= max
    // Postcondition: result >= min
    // Postcondition: result <= max
    public int Clamp(int value, int min, int max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }

    // Method that uses Clamp - test validation propagation
    // Postcondition: result >= 0
    // Postcondition: result <= 100
    public int NormalizeScore(int score)
    {
        return Clamp(score, 0, 100);
    }

    // Method called at multiple sites
    // Postcondition: result >= 0
    public int Square(int n)
    {
        return n * n;
    }

    // Uses Square multiple times - test multi-site inlining
    // Postcondition: result >= 0
    public int SumOfSquares(int x, int y)
    {
        return Square(x) + Square(y);
    }
}
