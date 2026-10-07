# Performance

Goal: an effective clock of 1 MHz for the W65C02S in Digisim, which means the engine alone
must run at >= 5 MHz (30M substeps/s) with close to zero steady-state allocation.

## Running the benchmarks

```
dotnet run -c Release --project W65C02S.Benchmarks -- --filter '*'
dotnet run -c Release --project W65C02S.Benchmarks -- --filter '*MixedLoop*'
dotnet run -c Release --project W65C02S.Benchmarks -- --verify
```

- `MixedLoopBenchmark.Run1MCycles` - 1M CPU cycles of a loop that mixes zp,X / abs,X /
  (zp),Y addressing, ADC, INC, stack, JSR/RTS and branches. MHz = 1 / mean seconds.
- `DormannBenchmark.RunToSuccess` - Klaus Dormann functional test, reset to the success
  trap at `$3469` (96,521,371 cycles), tracing off. Throws if it traps anywhere else.
- `--verify` - runs the Dormann workload once (no BenchmarkDotNet) as a correctness check.

## Results

Machine: Intel Core i7-13700HX, Windows 11, .NET 10.0.12, BenchmarkDotNet 0.15.8.

| Milestone | Mixed loop (1M cycles) | MHz  | Alloc/1M cycles | Dormann | MHz  | Dormann alloc |
|-----------|-----------------------:|-----:|----------------:|--------:|-----:|--------------:|
| Baseline  | 548.2 ms               | 1.82 | 1.2 GB          | 57.9 s  | 1.67 | 131.2 GB      |
