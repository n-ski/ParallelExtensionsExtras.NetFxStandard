# Parallel Extensions Extras [![NuGet package](https://img.shields.io/nuget/v/ParallelExtensionsExtras.NetFxStandard)](https://www.nuget.org/packages/ParallelExtensionsExtras.NetFxStandard)

In 2010, Microsoft released [Samples for Parallel Programming with the .NET Framework](http://archive.is/vtcRb) (archive), along with the ParallelExtensionsExtras, a library of various extensions, which was based on .NET Framework 4. This is a port of sample ParallelExtensionsExtras source code, targeting .NET Framework 4.0, .NET Framework 4.5, .NET Standard 2.0, and .NET 6.0.

For examples of how to use this library, please refer to [the series of blog posts by Stephen Toub](https://devblogs.microsoft.com/pfxteam/tag/parallelextensionsextras/).

The following APIs are available only when targeting .NET Framework 4+ and .NET 6+ Windows Desktop:
- `System.Threading.Tasks.AsyncCall.CreateInTargetAppDomain<T>`
- `System.Threading.Tasks.TaskExtrasExtensions.WaitWithPumping`
