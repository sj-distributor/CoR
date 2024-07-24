[![Build Status](https://github.com/sj-distributor/CoR/actions/workflows/build.yml/badge.svg?branch=main)](https://github.com/sj-distributor/CoR/actions?query=branch%3Amain)
[![codecov](https://codecov.io/gh/sj-distributor/CoR/graph/badge.svg?token=854D06RAR2)](https://codecov.io/gh/sj-distributor/CoR)
[![NuGet version (CoRProcessor)](https://img.shields.io/nuget/v/CoRProcessor.svg?style=flat-square)](https://www.nuget.org/packages/CoRProcessor/)
![](https://img.shields.io/badge/license-MIT-green)

# CoRProcessor Framework

### 概述 🌟
CoRProcessor 框架为在 .NET 应用中实现责任链（Chain of Responsibility, CoR）模式提供了一种方法。它允许您定义一系列处理器，以顺序处理请求，并支持添加前置、后置和最终操作，以及异常处理。

### 快速开始 🚀
#### 安装 📦
要使用 CoRProcessor 框架，只需将 `CoRProcessor` 添加到您的项目中。
```csharp
dotnet add package CoRProcessor
```

### 定义处理器 🛠️
处理器必须实现 `IChainProcessor<T>` 接口。以下是一个简单处理器的示例：

```csharp
public class SampleProcessor : IChainProcessor<MyData>
{
    public async Task<MyData> Handle(MyData data, CancellationToken token = default)
    {
        // Process the data
        Console.WriteLine("Processing in SampleProcessor");

        // Call the next processor in the chain
        return Task.FromResult(data);
    }
}

```

### 补偿机制 ↩️
CoRProcessor 框架支持补偿机制，允许在处理过程中发生异常时执行操作。
```csharp
public class SampleProcessor : IChainProcessor<MyData>
{
    public async Task<MyData> Handle(MyData data, CancellationToken token = default)
    {
        throw new Exception();          // 1. 发生异常
        return Task.FromResult(data);
    }
    
    public FuncDelegate<MyData> CompensateOnFailure { get; set; } = (context, token) =>
    {
        // 2. 只要执行的链路里面发生异常, 补偿机制的方法将会被依次执行
        return Task.FromResult(context);
    }; 
}

```

### 创建和执行处理器链 🏗️
您可以使用 `CoRProcessor<T>` 类创建和执行处理器链。如下所示：
```csharp
class Program
{
    public class MyData : IChainContext
    {
        public bool Abort { get; set; } // Abort = true, 可以跳过整个链路, 停止执行
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
            .GlobalPreExecute(async (data, token) =>
            {
                Console.WriteLine("Before action");
                await Task.CompletedTask;
            })
            .GlobalExecuted(async (data, token) =>
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
                await Task.FromResult(false); // Returning false will not throw an exception.
            });

        var result = await processor.Execute(new MyData(), CancellationToken.None);
    }
```
### Methods 📚
* **New()**: 创建一个新的 `CoRProcessor<T>` 实例。
* **AddRange(IEnumerable<IChainProcessor<T>> processors)**: 向链中添加一系列处理器。
* **Execute(T t, CancellationToken token = default)**: 使用提供的数据和取消令牌执行处理器链。
* **Before(FuncDelegate<T> action)**: 添加一个在主要处理之前执行的操作。
* **After(FuncDelegate<T> action)**: 添加一个在主要处理之后执行的操作。
* **Finally(FuncDelegate<T> action)**: 添加一个在所有处理完成后执行的操作(即使抛出异常, 依然会始终执行)。
* **OnException(FuncDelegate<T> action)**: 添加一个在发生异常时执行的操作。

### 异常处理 ⚠️
要处理异常，您可以使用 OnException 方法。这允许您在处理过程中发生异常时指定要执行的操作。
```csharp
processor.OnException(async (data, token) =>
{
    Console.WriteLine("Exception occurred");
    await Task.FromResult(false); // 返回 false 将不会抛出异常。
    await Task.FromResult(true);  // 返回 true 将抛出异常。
});
```

### 依赖注入（DI）使用 ☀️
### 微软依赖注入（DI） 🏢
您可以将 CoRProcessor 与微软内置的依赖注入（DI）系统集成。以下是一个示例：
只要实现了 IChainProcessor<T>,  AddCoR方法会自动注册
#### 在控制台应用中配置 DI (您也可以在 Web 程序中使用)： 
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
### Autofac 集成 🛠️
您也可以使用 Autofac 进行依赖注入。以下是如何将 Autofac 与 CoRProcessor 框架集成：
只要实现了 IChainProcessor<T>,  AddCoR方法会自动注册
#### 在控制台应用中配置 Autofac (您也可以在 Web 程序中使用)：
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
### 实际例子 ☀️
#### 以下是如何在实际场景中使用CoRProcessor框架的示例：
```csharp
 var result = await CoRProcessor<InsertOrUpdateOrderProcessorContext>
            .New()
            .AddRange([
                orderPreProcessor,
                orderValidaProcessor,
                orderCustomerProcessor,
                subTotalCalculationProcessor,
                discountAndChargeCalculationProcessor,
                subTotalBeforeDiscountCalculationProcessor,
                taxCalculationBeforeDiscountProcessor,
                taxCalculationAfterDiscountedProcessor,
                tipsCalculationProcessor,
                saveOrderRelationProcessor
            ])
            .Execute(new()
            {
                Merchant = merchant,
                Order = order,
            }, cancellationToken).ConfigureAwait(false);
```

### 许可证 📄
该项目根据 MIT 许可证授权。有关详细信息，请参阅 LICENSE 文件。