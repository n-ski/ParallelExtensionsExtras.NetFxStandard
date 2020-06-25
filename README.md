# Parallel Extensions Extras [![NuGet package](https://img.shields.io/nuget/v/ParallelExtensionsExtras.NetFxStandard)](https://www.nuget.org/packages/ParallelExtensionsExtras.NetFxStandard)

In 2010, Microsoft released [Samples for Parallel Programming with the .NET Framework](http://archive.is/vtcRb) (archive), along with the ParallelExtensionsExtras, a library of various extensions, which was based on .NET Framework 4. This is a port of sample ParallelExtensionsExtras source code, targeting both .NET Framework 4.0 and .NET Standard 2.0.

For examples of how to use this library, please refer to [the series of blog posts by Stephen Toub](https://devblogs.microsoft.com/pfxteam/tag/parallelextensionsextras/).

The following methods are not available when targeting .NET Standard:
- `CoordinationDataStructures\AsyncCoordination\AsyncCall.CreateInTargetAppDomain<T>` 
    - `AppDomain.CreateInstanceAndUnwrap` method is not available in .NET Standard
- `Extensions\TaskExtrasExtensions.WaitWithPumping`
     - Can't multitarget both .NET Standard and .NET Core Windows Desktop (requires WPF).

