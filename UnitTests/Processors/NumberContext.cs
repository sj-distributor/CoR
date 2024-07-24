using CoRProcessor;

namespace UnitTests.Processors;

public class NumberContext : IChainContext
{
    public Operation Operation { get; set; } = Operation.Addition;
    public decimal Number1 { get; set; } = 0.00m;
    public decimal Number2 { get; set; } = 0.00m;
    public decimal Result { get; set; } = 0.00m;
    public bool Abort { get; set; }
}

public enum Operation
{
    Addition,
    Subtraction,
    Multiplication,
    Division
}