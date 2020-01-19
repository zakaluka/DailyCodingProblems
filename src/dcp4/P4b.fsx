(**
# Conclusion

So, I think the place to begin is with a seemingly obvious statement - time and space complexity matters and should be balanced in conjunction with other considerations such as conceptual "simplicity" and maintainability.  For example, the set solver was one of the simplest solutions (for me) to understand.  However, the algorithm runtime was 521x slower and the total runtime was 165x slower than the fastest solution, with the total runtimes being 4 hours 42 minutes compared to less than 2 minutes on the same dataset.

The memory statistics present an interesting story, with the number of Gen 0 collections growing at a linear rate through the life of the program.  Now, most of the program's time was spent in the Set Solver's performance test.  At the end, there is a sharper uptick in Gen 0 collections, possibly due to the Sort Solver's performance test.  However, as I did not implement a method to correlate counter readings to the currently running code block, I can't say that with absolute certainty.

In general, I am extremely pleased with how [Hedgehog](https://github.com/hedgehogqa/fsharp-hedgehog) was able to take on a large percentage of the burdens related to property testing and random test data generation.  [Windows Performance Counters](https://docs.microsoft.com/en-us/dotnet/framework/debug-trace-profile/performance-counters) were also a pleasant find and a good alternative to BenchmarkDotNet.  I definitely plan to use these going forward, at least until FsLab comes to .NET 3.0 and I can use something like [dotnet-counters](https://devblogs.microsoft.com/dotnet/introducing-diagnostics-improvements-in-net-core-3-0/).
*)
