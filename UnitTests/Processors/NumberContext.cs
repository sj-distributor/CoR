namespace UnitTests.Processors;

public class NumberContext
{
    public Operation Operation { get; set; } = Operation.Addition;
    public decimal Number1 { get; set; } = 0.00m;
    public decimal Number2 { get; set; } = 0.00m;
    public decimal Result { get; set; } = 0.00m;
}

public enum Operation
{
    Addition,
    Subtraction,
    Multiplication,
    Division
}