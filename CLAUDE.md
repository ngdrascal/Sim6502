# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

Cycle-accurate, pin-level simulator of the WDC W65C02S CPU, plus a pin-waveform-accurate W65C21 PIA, in C# (.NET 10, `net10.0`). Ported from an earlier Java version. Solution file is `Sim6502.slnx` (XML format).

Projects:
- `W65C02S.Engine` - the simulator library.
- `W65C02S.Engine.Tests` - xUnit v3 unit tests (namespace is still `Sim6502.Tests`).
- `W65C02S.ValidationSuite` - console exe that runs Klaus Dormann functional, 65C02 extended opcode, and Bruce Clark BCD test binaries (`Tests/*.bin`). Not an xUnit project; pick which suite runs by editing `Program.cs`.
- `W65C02S.DigisimPlugin` - wraps the engine as a component (`W65C02SCpu`) for the Digisim circuit simulator.
- `W65C02S.DigisimPlugin.Tests` - xUnit v3 tests driving the plugin through the real Digisim scheduler.
- `W65C21.Engine` - the PIA simulator library (no dependencies). Design: `Docs/W65C21-Design.md`; vocabulary: `CONTEXT.md`.
- `W65C21.Engine.Tests` - xUnit v3 unit tests for the PIA engine.
- `W65C21.DigisimPlugin` - wraps the PIA engine as a component (`W65C21Pia`) for Digisim.
- `W65C21.DigisimPlugin.Tests` - plugin tests through the real Digisim scheduler, plus CPU+memory+PIA integration tests (`SystemBench`).
- `DigisimPlugin.TestHelpers` - test doubles shared by both plugin test projects (`SignalSource`, `FakeMemory`, `CapturingLogger`).

## Commands

```
dotnet build Sim6502.slnx
dotnet test Sim6502.slnx
dotnet test W65C02S.Engine.Tests --filter "FullyQualifiedName~LDATests"
dotnet test W65C02S.Engine.Tests --filter "FullyQualifiedName~LDATests.TestLDAimmWith0"
dotnet run --project W65C02S.ValidationSuite
```

The Digisim projects depend on `Digisim.Sdk` / `Digisim.Engine` 1.0.0 from a local NuGet feed at `C:\nuget-local` (see `nuget.config`), produced by Digisim's `scripts/pack-sdk.ps1`. If restore fails for those, the feed is missing; the engine projects build without it. The plugin references `Digisim.Sdk` with `ExcludeAssets="runtime"` because the host supplies it.

Note: CI (`.github/workflows`) sets up .NET 10 but cannot restore the Digisim packages, which exist only in the local feed.

## Engine architecture

Key files: `W65c02sEngine.cs`, `Context.cs`, `States.cs`, `Instructions/`, `Pins.cs`, `Constants.cs`. More detail in `Docs/ARCHITECTURE.md` (partly outdated on substep numbering and project names). Datasheet: `Docs/DataSheets/w65c02s.pdf`.

**Clocking.** The caller toggles `Pins.PHI2` and calls `engine.Step()`; `Step()` does nothing unless PHI2 changed. Each toggle advances one substep. A CPU cycle is substeps 1..6 (`Constants.P1MiddleStep`=2, `P1LastStep`=3, `P2FirstStep`=4, `P2MiddleStep`=5, `P2LastSubstep`=6). Comments elsewhere naming other substep values (4, 8, 10) are stale.
- Substep 1: RDY check (enters `NotReady`), pending NMI/IRQ latched into `Context` flags, cycle count.
- Substep 2 (`P1MiddleStep`): states typically drive address bus / RWB; status pins SYNC/VPB/MLB updated from `BusStatusSignals`.
- Substep 6 (`P2LastSubstep`): states latch read data from `Pins.DataBus` and call `AdvanceState`; engine then samples RESB (low >= 2 cycles -> `Boot1`), NMIB (falling edge), IRQB (level, masked by I), SOB (falling edge sets V, after the instruction's own flag writes).

**State machine.** `States` enum holds one value per instruction cycle (e.g. `InstLDAabsx4`) plus system states (`WarmUp0-2`, `Boot1-2`, `Fetch`, `NotReady`, `Stop`, `Interrupt*`). Two lookup tables in the engine:
- `_t2Map[opcode] -> States` - the state entered after `Fetch` (cycle T2).
- `_stateMethodMap[state] -> Action<Context>` - the method run on every substep while in that state.

Each instruction family class in `Instructions/` (derives `InstBase`, implements `IInstruction`) fills both tables via `RegisterT2State(IT2Registry)` and `RegisterStates(IStateRegistry)`. The engine constructor instantiates every family; a new family must be added there. State methods branch on `ctx.GetSubStep()` and end with `ctx.AdvanceState(next)`, which only takes effect at the last substep. Page-cross penalties use `ctx.CrossedPageBoundary` to choose an extra state.

At `Fetch`, a pending NMI/IRQ flag diverts to `Interrupt1` (`Instructions/Interrupts.cs`, shared with BRK sequence).

**Types.** Registers and buses use custom reference types `Types.UInt8`, `Types.UInt16`, `Types.BitFlag` (not the BCL types); files alias them with `using UInt8 = W65C02S.Engine.Types.UInt8;`. ADC/SBC (incl. decimal mode) live on `UInt8` returning `MathResult`.

**Pins.** `Pins` implements both `IPinsExternal` (what a host drives/reads) and `IPinsInternal` (what the engine uses). `DBG*` pins (e.g. `DBGINST`, `DBGSUBSTEP`) expose internals for tests.

**Debug output.** `OnInstructionComplete` (with `Disassembler.Disassemble`) and `OnSubstepChanging` events are used for tracing.

## Unit testing

Tests use **xUnit v3** with **NSubstitute** for mocking and coverlet for coverage. Every test
project mirrors a source project 1:1 (e.g. `Digisim.Engine` ↔ `Digisim.Engine.Tests`), and
production assemblies grant `InternalsVisibleTo` to their matching test project. Shared test
helpers (base classes, prebuilt simulation models, fakes) live in `Digisim.Tests.Shared`, which
every test project references.

Every unit test has comment that preceeds it.  Use the following format:
```
   /*
      TITLE: <clear text description of the test>
      GIVEN: <precondition before running the test>
      WHEN: <action trigging the code under test to execute>
      THEN: <expect post-execution states>
    */
```
An example:
```
   /*
      TITLE: Writing to THR invokes TxSink with the written byte
      GIVEN: an Uart16550 with TxSink capturing output
      WHEN: 0x41 is written to offset 0 (THR, DLAB clear by default)
      THEN: TxSink is invoked once with 0x41
    */
```
The body of each unit test follow the arrange, act, assert pattern.

An example:
```
   {
      // ARRANGE:

      // ACT:

      // ASSERT:
   }
```
The minimum code coverage for every project is 90%

## W65C02S Tests

Engine tests derive from `UnitTestBase`, which builds Pins/Registers/Context/Engine and provides `BootToAddress(addr)` (warm-up + reset vector fed through the data bus), `ExecuteClockCycles(n)`, and `ExecuteProgram(program, loadAddr, runAddr)` against a 64KB `Memory` array. Typical pattern: put an opcode/operand on `Pins.DataBus`, run one cycle, assert on `Pins.AddrBus`, `Regs`, and flags. ADC/SBC tests read `Types/adc.csv` and `sbc.csv`.

## Digisim plugin

One external PHI2 period = one CPU cycle; each external edge runs several engine substeps (falling: 6,1,2,3; rising: 4,5) by toggling a private engine clock. D is driven only while RWB low and PHI2 high; BE low floats A/D/RWB; unconnected control inputs read high. Build output folder is what Digisim's `Digisim:PluginPaths` points at.

## W65C21 PIA

`W65C21Engine` owns a `W65C21Pins` object. The host sets inputs and calls `Evaluate()` after any change; the engine detects PHI2 edges, control-line transitions and RESB (a level) itself. Outputs are a value plus a drive flag/mask; IRQAB/IRQBB are open drain (drive flag means low). Edge mapping: PHI2 rise samples select/RS/RWB/ports, starts driving D on reads, clears flags on Read A/B Data, and clocks CB2 strobes; PHI2 fall latches writes and clocks CA2 strobes. `PiaSide` holds one Side's registers and control-line logic. `RegisterAccessed` reports each selected bus cycle (used for Debug logging). Engine tests derive from `PiaTestBase` (`Write`, `Read`, `Rise`, `Fall`, `IdleCycle`).

The `W65C21Pia` component (TypeUid `W65C21`, category Peripherals) only marshals pins. Floating control inputs read high; floating Port lines read 1 but are never driven (no pull-ups). Because a Digisim `InOutPin` that drives any bit stops receiving, Port/CA2/CB2/D input levels are resolved in `NetLevels` from `ConnectedOutputs` (normal over weak). Add the plugin's build output folder as a second entry in `Digisim:PluginPaths`.

## Code style

- Remove unused usings; build must be warning-free.
- `return` statements on their own line.
- ASCII `-` only in comments (no em/en dashes).

** IMPORTANT **
When working in the context of a multi-milestone plan, aways stop between milestones for a code review.  After the review the human will either commit the changes or ask you to do that for them.