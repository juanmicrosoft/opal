namespace SignatureExamples;

public class Functions
{
    // Method to add a parameter to
    public string Greet(string name)
    {
        return name;
    }

    // Method to change return type
    public int TryParse(string input)
    {
        return 0;
    }

    // Method to reorder parameters
    // Precondition: start <= end
    // Precondition: step > 0
    public int CreateRange(int start, int end, int step)
    {
        return (end - start) / step;
    }

    // Method that calls CreateRange - for testing call site updates
    // Precondition: max > 0
    public int CountSteps(int max)
    {
        return CreateRange(0, max, 1);
    }
}
