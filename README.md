[![Build Status](https://github.com/sj-distributor/CoR/actions/workflows/build.yml/badge.svg?branch=main)](https://github.com/sj-distributor/CoR/actions?query=branch%3Amain)
[![codecov](https://codecov.io/gh/sj-distributor/FastCache/branch/master/graph/badge.svg?token=XV3W873RGV)](https://codecov.io/gh/sj-distributor/CoR)
[![NuGet version (CoRProcessor)](https://img.shields.io/nuget/v/CoRProcessor.svg?style=flat-square)](https://www.nuget.org/packages/CoRProcessor/)
![](https://img.shields.io/badge/license-MIT-green)


# CoRProcessor Framework

### Overview 🌟
The CoRProcessor framework provides a way to implement the Chain of Responsibility (CoR) pattern in .NET applications. It allows you to define a chain of processors that can handle a request in sequence, with support for adding before, after, and finally actions, as well as exception handling.

### Getting Started 🚀
#### Installation 📦
To use the CoRProcessor framework, simply add the `CoRProcessor` package to your project.
```csharp
dotnet add package CoRProcessor
```

### Define a Processor 🛠️
Processors must implement the `IChainProcessor<T>` interface. Here's an example of a simple processor:

```csharp
public class SampleProcessor : IChainProcessor<MyData>
{
    public IChainProcessor<MyData> Next { get; set; }

    public async Task<MyData> Handle(MyData data, CancellationToken token = default)
    {
        // Process the data
        Console.WriteLine("Processing in SampleProcessor");

        // Call the next processor in the chain
        return await Next.Handle(data, token);
    }
}

```

### Create and Execute the Processor Chain 🏗️
You can create and execute a processor chain using the `CoRProcessor<T>` class. Here's how to do it:
```csharp
class Program
{
    public class MyData
    {
        public string Data { get; set; }
    }
    
    static async Task Main(string[] args)
    {
        var processors = new List<IChainProcessor<MyData>>
        {
            new SampleProcessor(),
            new AnotherProcessor() // Another processor implementing IChainProcessor<MyData>
        };

        var processor = CoRProcessor<MyData>.New()
            .AddRange(processors)
            .Before(async (data, token) =>
            {
                Console.WriteLine("Before action");
                await Task.CompletedTask;
            })
            .After(async (data, token) =>
            {
                Console.WriteLine("After action");
                await Task.CompletedTask;
            })
            .Finally(async (data, token) =>
            {
                Console.WriteLine("Finally action");
                await Task.CompletedTask;
            })
            .OnException(async (data, token) =>
            {
                Console.WriteLine("Exception occurred");
                await Task.CompletedTask;
            });

        var result = await processor.Execute(new MyData(), CancellationToken.None);
    }
```
### Methods 📚
* **New()**: Creates a new instance of the `CoRProcessor<T>`. 
* **AddRange(IEnumerable<IChainProcessor<T>> processors)**: Adds a range of processors to the chain.
* **Execute(T t, CancellationToken token = default)**: Executes the processor chain with the provided data and cancellation token.
* **Before(FuncDelegate<T> action)**: Adds an action to be executed before the main processing.
* **After(FuncDelegate<T> action)**: Adds an action to be executed after the main processing.
* **Finally(FuncDelegate<T> action)**: Adds an action to be executed after all processing is complete.
* **OnException(FuncDelegate<T> action)**: Adds an action to be executed when an exception occurs.

### Exception Handling ⚠️
To handle exceptions, you can use the OnException method. This allows you to specify an action that should be taken when an exception occurs during processing.
```csharp
processor.OnException(async (data, token) =>
{
    Console.WriteLine("Exception occurred");
    await Task.CompletedTask;
});
```

### Dependency Injection (DI) Usage☀️
### Microsoft Dependency Injection (DI) 🏢
You can integrate the CoRProcessor with Microsoft's built-in Dependency Injection (DI) system. Here's an example of how to do it:
As long as IChainProcessor<T>is implemented, the AddCoR method will automatically register
#### Configure DI in your Console Application:
```csharp
class Program
{
    static async Task Main(string[] args)
    {
        var serviceProvider = new ServiceCollection()
            .AddCoR(typeof(Program).Assembly)
            .BuildServiceProvider();

        var additionProcessor = serviceProvider.GetRequiredService<AdditionProcessor>();

        var result = await CoRProcessor<NumberContext>
            .New()
            .AddRange(new[] { additionProcessor })
            .Execute(new NumberContext
            {
                Number1 = 1,
                Number2 = 1,
                Operation = Operation.Addition
            }, default);
        
        Console.WriteLine(result);
    }
}
```
### Autofac Integration 🛠️
You can also use Autofac for Dependency Injection. Here’s how you can integrate Autofac with the CoRProcessor framework:
As long as IChainProcessor<T>is implemented, the AddCoR method will automatically register.
#### Configure Autofac in your Console Application:
```csharp
class Program
{
    static async Task Main(string[] args)
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
        
        Console.WriteLine(result);
    }
}
```

### License 📄
This project is licensed under the MIT License. See the LICENSE file for details.