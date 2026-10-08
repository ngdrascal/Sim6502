# Performance

Goal: an effective clock of 1 MHz for the W65C02S in Digisim, which means the engine alone
must run at >= 5 MHz (30M substeps/s) with close to zero steady-state allocation.

## Running the benchmarks

```
dotnet run -c Release --project W65C02S.Benchmarks -- --filter '*'
dotnet run -c Release --project W65C02S.Benchmarks -- --filter '*MixedLoop*'
dotnet run -c Release --project W65C02S.Benchmarks -- --verify
dotnet run -c Release --project W65C02S.Benchmarks -- --profile engine|digisim
```

- `MixedLoopBenchmark.Run1MCycles` - engine alone, 1M CPU cycles of a loop that mixes zp,X /
  abs,X / (zp),Y addressing, ADC, INC, stack, JSR/RTS and branches. MHz = 1 / mean seconds.
- `DormannBenchmark.RunToSuccess` - engine alone, Klaus Dormann functional test from reset to
  the success trap at `$3469` (96,561,325 cycles), tracing off. Throws if it traps anywhere else.
- `DigisimCpuBenchmark.Run100KCycles` - `W65C02SCpu` + `FakeMemory` + clock in a real Digisim
  `SimulationModel`, 100K cycles of the mixed loop. MHz = 0.1 / mean seconds. End to end:
  includes the Digisim scheduler and pin resolution.
- `--verify` - runs each workload once plus the Bruce Clark BCD test (no BenchmarkDotNet) as a
  correctness check.
- `--profile` - a long steady run to attach a profiler to, e.g.
  `dotnet-trace collect -- <exe> --profile digisim`.

## Results

Machine: Intel Core i7-13700HX, Windows 11, .NET 10.0.12, BenchmarkDotNet 0.15.8.

| Milestone | Mixed loop (1M) | MHz  | Alloc   | Dormann | MHz  | Alloc    | Digisim (100K) | MHz  | Alloc     |
|-----------|----------------:|-----:|--------:|--------:|-----:|---------:|---------------:|-----:|----------:|
| Baseline  | 548.2 ms        | 1.82 | 1.2 GB  | 57.9 s  | 1.67 | 131.2 GB | 215.5 ms       | 0.46 | 289.3 MB  |
| M2        | 132.6 ms        | 7.54 | 154.6 MB| 11.8 s  | 8.21 | 15.0 GB  | 145.9 ms       | 0.69 | 164.8 MB  |
| M3        | 42.3 ms         | 23.66| 0 B     | 4.93 s  | 19.57| 113 KB   | 72.6 ms        | 1.38 | 135.6 MB  |

M2: substep/state-change event args only built when subscribed; opcode decode is a table lookup
(was `Enum.GetValues` + linear scan per fetch); SYNC/VPB/MLB state lookups are arrays (were
`HashSet`); `Pins` holds plain fields and whole-bus values (was a bounds-checked `byte[40]` and
a 16-step loop per address read).

M3: `UInt8`, `UInt16`, `BitFlag` and `MathResult` are `readonly struct`s, so register, bus and flag
updates allocate nothing. The engine alone now runs with zero steady-state allocation; the Dormann
figure is the one-time harness setup (64K memory, engine tables). The 135.6 MB left in the Digisim
run is the `DriverResolution.Resolve` closure below.

## Findings outside the engine

Handoff with the exact Digisim changes and release steps: `Docs/Digisim-Allocation-Fix.md`.

Allocation profile of `DigisimCpuBenchmark` after M2 (dotnet-trace gc-verbose, AllocationTick):

- ~80% - closure (`<>c__DisplayClass1_0`) in Digisim `DriverResolution.Resolve`. The lambda for
  the 2+ driver path captures `bits`, so the closure is allocated on entry of every call, even
  the 0- and 1-driver fast paths. Every `UpdateFromDrivers` of every input pin pays it. Moving
  the 2+ driver path into its own method removes it. This is in the Digisim repo.
- ~16% - engine `UInt8` / `UInt16` objects (bus reads in the adapter, register copies); removed in M3.
- ~1% - Digisim `InOutPin.Mode` setter calls `LogTrace` without an `IsEnabled` guard, boxing
  its arguments on every D-bus direction change.

CPU profile: about 80% of the main thread's time in the Digisim run is spent stalled on GC,
so allocation, not compute, limits the end-to-end clock.
