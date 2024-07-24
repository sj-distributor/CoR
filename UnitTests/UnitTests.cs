using Autofac;
using CoRProcessor;
using Microsoft.Extensions.DependencyInjection;
using UnitTests.Processors;

namespace UnitTests;

public class UnitTests
{
    [TestCase(1, 1, Operation.Addition, 2)]
    [TestCase(1, 1, Operation.Subtraction, 0)]
    [TestCase(1, 2, Operation.Multiplication, 2)]
    [TestCase(1, 2, Operation.Division, 0.5)]
    public async Task TestCorProcessorRunningIsOk(decimal number1, decimal number2, Operation operation, decimal result)
    {
        var resContext = await CoRProcessor<NumberContext>
            .New()
            .AddRange([
                new AdditionProcessor(),
                new SubtractionProcessor(),
                new MultiplicationProcessor(),
                new DivisionProcessor()
            ]).Execute(new NumberContext()
            {
                Number1 = number1,
                Number2 = number2,
                Operation = operation
            }, default);


        Assert.That(resContext.Result, Is.EqualTo(result));
    }


    [TestCase(1, 1, Operation.Addition, 1, 2, 3, 9)]
    public async Task TestCorProcessorRunningWithHook(
        decimal number1, decimal number2, Operation operation,
        decimal beforeChange, decimal afterChange, decimal finallyChange, decimal result)
    {
        var resContext = await CoRProcessor<NumberContext>
            .New()
            .GlobalPreExecute(async (t, token) =>
            {
                Assert.That(t.Number1, Is.EqualTo(number1));
                Assert.That(t.Number2, Is.EqualTo(number2));
                t.Number1 += beforeChange;
                t.Number2 += beforeChange;
                await Task.CompletedTask;
            })
            .GlobalExecuted(async (t, token) =>
            {
                t.Result += afterChange;
                await Task.CompletedTask;
            })
            .Finally(async (t, token) =>
            {
                t.Result += finallyChange;
                await Task.CompletedTask;
            })
            .AddRange([
                new AdditionProcessor(),
                new SubtractionProcessor(),
                new MultiplicationProcessor(),
                new DivisionProcessor()
            ])
            .Execute(new NumberContext()
            {
                Number1 = number1,
                Number2 = number2,
                Operation = operation
            }, default);


        Assert.That(resContext.Result, Is.EqualTo(result));
    }


    [TestCase(1, 1, Operation.Addition, 1, 2, 3, 9)]
    public async Task TestCorProcessorRunningFail(
        decimal number1, decimal number2, Operation operation,
        decimal beforeChange, decimal afterChange, decimal finallyChange, decimal result)
    {
        try
        {
            await CoRProcessor<NumberContext>
                .New()
                .GlobalPreExecute(async (t, token) =>
                {
                    Assert.That(t.Number1, Is.EqualTo(number1));
                    Assert.That(t.Number2, Is.EqualTo(number2));
                    t.Number1 += beforeChange;
                    t.Number2 += beforeChange;
                    await Task.CompletedTask;
                })
                .GlobalExecuted(async (t, token) =>
                {
                    t.Result += afterChange;
                    await Task.CompletedTask;
                })
                .Finally(async (t, token) =>
                {
                    t.Result += finallyChange;
                    await Task.CompletedTask;
                })
                .AddRange([
                    // new AdditionProcessor(),
                    // new SubtractionProcessor(),
                    // new MultiplicationProcessor(),
                    // new DivisionProcessor()
                ])
                .Execute(new NumberContext()
                {
                    Number1 = number1,
                    Number2 = number2,
                    Operation = operation
                }, default);
        }
        catch (Exception e)
        {
            Assert.That(e.Message, Is.EqualTo("No processors provided. At least one processor is required."));
        }
    }


    [TestCase(1, 1, Operation.Addition, 1, 2, 3, 9)]
    public async Task TestCorProcessorRunningOnException(
        decimal number1, decimal number2, Operation operation,
        decimal beforeChange, decimal afterChange, decimal finallyChange, decimal result)
    {
        try
        {
            await CoRProcessor<NumberContext>
                .New()
                .GlobalPreExecute(async (t, token) =>
                {
                    Assert.That(t.Number1, Is.EqualTo(number1));
                    Assert.That(t.Number2, Is.EqualTo(number2));
                    t.Number1 += beforeChange;
                    t.Number2 += beforeChange;
                    await Task.CompletedTask;
                })
                .GlobalExecuted(async (t, token) =>
                {
                    t.Result += afterChange;
                    await Task.CompletedTask;
                })
                .Finally(async (t, token) =>
                {
                    t.Result += finallyChange;
                    await Task.CompletedTask;
                })
                .OnException(async (t, e, token) =>
                {
                    t.Result += finallyChange;
                    return await Task.FromResult(true);
                })
                .AddRange([
                    new ExceptionProcessor(),
                ])
                .Execute(new NumberContext()
                {
                    Number1 = number1,
                    Number2 = number2,
                    Operation = operation
                }, default);
        }
        catch (Exception e)
        {
            Assert.That(e.Message, Is.EqualTo("Attempted to divide by zero."));
        }
    }

    [Test]
    public async Task TestCorProcessorRunningOnMSDI()
    {
        var serviceProvider = new ServiceCollection()
            .AddCoR(typeof(UnitTests).Assembly)
            .BuildServiceProvider();

        var additionProcessor = serviceProvider.GetRequiredService<AdditionProcessor>();

        var result = await CoRProcessor<NumberContext>
            .New()
            .AddRange([
                additionProcessor
            ])
            .Execute(new NumberContext()
            {
                Number1 = 1,
                Number2 = 1,
                Operation = Operation.Addition
            }, default);

        Assert.That(result.Result, Is.EqualTo(2));
    }

    [Test]
    public async Task TestCorProcessorRunningOnAutofac()
    {
        var builder = new ContainerBuilder();
        var container = builder.AddCoR(typeof(UnitTests).Assembly).Build();

        var additionProcessor = container.Resolve<AdditionProcessor>();

        var result = await CoRProcessor<NumberContext>
            .New()
            .AddRange([
                additionProcessor
            ])
            .Execute(new NumberContext()
            {
                Number1 = 1,
                Number2 = 1,
                Operation = Operation.Addition
            }, default);

        Assert.That(result.Result, Is.EqualTo(2));
    }


    [Test]
    public async Task TestEmptyProcessorRunning()
    {
        var emptyProcessor = new EmptyProcessor<NumberContext>();

        var result = await emptyProcessor.Handle(new NumberContext()
        {
            Number1 = 1,
            Number2 = 2,
            Operation = Operation.Addition
        }, default);

        Assert.That(result.Result, Is.EqualTo(0));
    }

    [Test]
    public async Task TestChainAbortCanStopExecute()
    {
        var result = await CoRProcessor<NumberContext>
            .New()
            .AddRange([
                new AbortProcessor(),
                new AdditionProcessor(),
                new AdditionProcessor(),
            ])
            .Execute(new NumberContext()
            {
                Number1 = 1,
                Number2 = 1,
                Operation = Operation.Addition
            }, default);


        Assert.That(result.Result, Is.EqualTo(0));

        result = await CoRProcessor<NumberContext>
            .New()
            .AddRange([
                new AdditionProcessor(),
                new AbortProcessor(),
                new AdditionProcessor(),
            ])
            .Execute(new NumberContext()
            {
                Number1 = 1,
                Number2 = 1,
                Operation = Operation.Addition
            }, default);

        Assert.That(result.Result, Is.EqualTo(2));
    }

    [Test]
    public async Task TestCompensateOnFailure()
    {
        var result = await CoRProcessor<NumberContext>
            .New()
            .AddRange([
                new AdditionProcessor(),
                new AdditionProcessor(),
                new ExceptionProcessor(),
                new AdditionProcessor(),
            ])
            .OnException(async (t, e, token) => await Task.FromResult(false))
            .Execute(new NumberContext()
            {
                Number1 = 1,
                Number2 = 1,
                Operation = Operation.Addition
            }, default);

        Assert.That(result.Result, Is.EqualTo(6));
        
         result = await CoRProcessor<NumberContext>
            .New()
            .AddRange([
                new AdditionProcessor(),
                new ExceptionProcessor(),
                new AdditionProcessor(),
                new AdditionProcessor(),
            ])
            .OnException(async (t, e, token) => await Task.FromResult(false))
            .Execute(new NumberContext()
            {
                Number1 = 1,
                Number2 = 1,
                Operation = Operation.Addition
            }, default);

        Assert.That(result.Result, Is.EqualTo(3));
        
        result = await CoRProcessor<NumberContext>
            .New()
            .AddRange([
                new AdditionProcessor(),
                new AdditionProcessor(),
                new AdditionProcessor(),
                new ExceptionProcessor(),
            ])
            .OnException(async (t, e, token) => await Task.FromResult(false))
            .Execute(new NumberContext()
            {
                Number1 = 1,
                Number2 = 1,
                Operation = Operation.Addition
            }, default);

        Assert.That(result.Result, Is.EqualTo(9));
    }
    
    [Test]
    public async Task TestNotImplementationCompensateOnFailure()
    {
        var result = await CoRProcessor<NumberContext>
            .New()
            .AddRange([
                new SubtractionProcessor(),
                new ExceptionProcessor(),
                new SubtractionProcessor(),
            ])
            .OnException(async (t, e, token) => await Task.FromResult(false))
            .Execute(new NumberContext()
            {
                Number1 = 1,
                Number2 = 1,
                Operation = Operation.Subtraction
            }, default);

        Assert.That(result.Result, Is.EqualTo(0));
    }
    
    [Test]
    public async Task TestSpecifyExecution()
    {
        var result = await CoRProcessor<NumberContext>
            .New()
            .AddRange([
                new AdditionProcessor(),
                new AdditionProcessor(),
                new AdditionProcessor(),
                new AdditionProcessor(),
            ])
            .Execute(new NumberContext()
            {
                Number1 = 1,
                Number2 = 1,
                Operation = Operation.Addition
            }, default, 3);


        Assert.That(result.Result, Is.EqualTo(2));
    }
    
    [Test]
    public async Task TestNotSpecifyExecution()
    {
        var result = await CoRProcessor<NumberContext>
            .New()
            .AddRange([
                new AdditionProcessor(),
                new AdditionProcessor(),
                new AdditionProcessor(),
                new AdditionProcessor(),
            ])
            .Execute(new NumberContext()
            {
                Number1 = 1,
                Number2 = 1,
                Operation = Operation.Addition
            }, default);


        Assert.That(result.Result, Is.EqualTo(8));
    }
}