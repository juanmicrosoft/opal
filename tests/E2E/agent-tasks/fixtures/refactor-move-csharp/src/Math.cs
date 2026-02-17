namespace MoveExamples;

public class MathModule
{
    // Method to move to Utils class
    // Postcondition: result >= 0
    public int Abs(int x)
    {
        return x < 0 ? -x : x;
    }

    // Method with validation that will be moved
    // Precondition: b != 0
    // Postcondition: result == a / b
    public int SafeDivide(int a, int b)
    {
        if (b == 0)
            throw new DivideByZeroException();
        return a / b;
    }

    // Method with dependencies - move requires updating callers
    // Postcondition: result >= 0
    public int Distance(int x, int y)
    {
        return Abs(x * x + y * y);
    }
}
