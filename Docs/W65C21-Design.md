# W65C21 PIA - Digisim Component Design

Status: agreed and implemented 2026-10-01. Datasheet: `Docs/DataSheets/w65c21.pdf`.
Vocabulary: see `CONTEXT.md` (W65C21 PIA section). Template to follow: `W65C02S.DigisimPlugin/W65C02SCpu.cs`.

## Goal

A pin-waveform-accurate W65C21 (W65C21N/S) PIA usable as a Digisim plugin component.
Digisim has no propagation delays, so "waveform accurate" means every output changes on the
correct PHI2 edge or input transition as shown in datasheet Figures 5-8.

## Digisim facts that constrain the design

- Pins support per-bit high-Z via `Drive(value, highZMask)`, so one 8-bit `InOutPin` per port can
  mix input and output bits.
- `InOutPin` is always `DriveStrength.Normal`; only `OutputPin` can be `Weak`. A net allows only
  one weak driver (else `BusConflictException`). Hence no internal port pull-ups (see below).
- `Digisim:PluginPaths` is a list, so a second plugin output folder can be added.
- Plugins reference `Digisim.Sdk` with `ExcludeAssets="runtime"`; tests reference `Digisim.Engine`
  from the local feed `C:\nuget-local`.

## Projects (all added to `Sim6502.slnx`)

| Project | Purpose |
|---|---|
| `W65C21.Engine` | PIA logic. No dependencies (plain `byte`/`bool`, not `W65C02S.Engine.Types`). |
| `W65C21.Engine.Tests` | xUnit v3 unit tests for the engine. Namespace `W65C21.Engine.Tests`. |
| `W65C21.DigisimPlugin` | `W65C21Pia` component; pin marshalling only. |
| `W65C21.DigisimPlugin.Tests` | Tests through the real Digisim scheduler. Namespace `W65C21.DigisimPlugin.Tests`. |
| `DigisimPlugin.TestHelpers` | `SignalSource`, `CapturingLogger`, `FakeMemory` moved out of `W65C02S.DigisimPlugin.Tests`; referenced by both plugin test projects. |

Engine and plugin grant `InternalsVisibleTo` to their test projects. Coverage >= 90% per project.

## Engine API

- `W65C21Engine` owns a pins object: the host sets inputs; outputs are exposed as value + per-bit
  drive mask for D, PA, PB, CA2, CB2, IRQAB, IRQBB.
- Host calls `Evaluate()` after any input change. The engine detects PHI2 edges, control-line
  transitions and RESB itself. All timing rules live in the engine.
- Internal registers (CRA, CRB, DDRA, DDRB, ORA, ORB) exposed as `internal` properties for tests.

## Component (`W65C21Pia`)

- TypeUid `"W65C21"`, PartName `W65C21`, category `"Peripherals"`. One generic part (no N/S
  variant property; the difference is invisible at logic level).
- Pins - left: PHI2 (ClockIn), RESB, CS0, CS1, CS2B, RS0, RS1, RWB. Right: D[8] (bi), IRQAB, IRQBB,
  PA[8] (bi), CA1 (in), CA2 (bi), PB[8] (bi), CB1 (in), CB2 (bi).
- Floating (unconnected or high-Z) control inputs read high, like the CPU's `Level()`. A floating
  CS2B therefore deselects the part.
- Floating port input bits (PA and PB) read as 1 internally; the net itself stays high-Z (no weak
  driver, avoiding pull-up conflicts).
- Port bit with DDR=1 drives ORx bit; DDR=0 bit is high-Z.
- IRQAB/IRQBB are open drain: drive 0 or high-Z. User adds one pull-up per net if needed; the CPU
  already reads high-Z IRQB as high.
- Debug-level logging of register reads/writes via `LogManager.GetLogger<W65C21Pia>()`.

## Register addressing (Table 2)

| RS1 | RS0 | CRx bit 2 | Register |
|---|---|---|---|
| 0 | 0 | 1 | Port A read / ORA write |
| 0 | 0 | 0 | DDRA |
| 0 | 1 | - | CRA |
| 1 | 0 | 1 | Port B read / ORB write |
| 1 | 0 | 0 | DDRB |
| 1 | 1 | - | CRB |

Selected when CS0=1, CS1=1, CS2B=0.

Reading Port A returns the pin levels (floating input = 1). Reading Port B returns ORB for output
bits and pin levels for input bits.

## Timing (edge mapping)

PHI2 rise:
- Sample CS0/CS1/CS2B, RS0/RS1, RWB and port pins (tACR, tPCR measured to the rise).
- Selected read: drive D from rise to fall with the value latched at the rise.
- Read A Data / Read B Data: clear that Side's Interrupt Flags and release IRQ at the rise
  (Figure 8). A control-line active transition later in the same cycle sets the flag again.
- CB2 (handshake and pulse) goes low at the first PHI2 rise after a Write B Data (Figure 5); in
  pulse mode it returns high at the next rise.

PHI2 fall:
- Selected write: latch D and transfer into the target register; port outputs change here.
- CA2 (handshake and pulse) goes low at the fall ending a Read A Data (Figure 6); in pulse mode it
  returns high at the next fall.

Asynchronous (no clock involved):
- CA1/CB1, and CA2/CB2 in input mode: on the active transition set the Interrupt Flag; if enabled,
  IRQ goes low immediately (tRS3).
- Handshake mode: active transition on CA1/CB1 sets CA2/CB2 high immediately (tRS2).
- RESB low (level): all registers 0, ports and CA2/CB2 are inputs, D floats, IRQs released, bus
  cycles ignored while low.

## Control Register semantics (Table 1, Table 3)

- Bit 7: IRQx1 flag (CA1/CB1). Bit 6: IRQx2 flag (CA2/CB2). Both read-only.
- Bits 1-0: CA1/CB1 control. Bit 1 = active edge (0 falling, 1 rising); bit 0 = IRQ enable.
- Bit 2: DDR Access bit.
- Bits 5-3: CA2/CB2 control.
  - 0 e x: input; bit 4 = active edge, bit 3 = IRQ enable.
  - 1 0 0: handshake. 1 0 1: pulse. 1 1 0: manual low. 1 1 1: manual high.

Ambiguities resolved per MC6821 behavior:
- IRQx2 flag stays 0 while CA2/CB2 is in output mode.
- Enabling an IRQ enable bit while its flag is already set pulls IRQ low immediately.
- Entering handshake or pulse mode leaves CA2/CB2 high.
- CA2 pulse triggers only on Read A Data (Table 3), not "whenever selected" (p14 text).
- CB2 handshake triggers on Write B Data (Table 3 mislabels the row "Handshake on Read").

## Tests

- Engine tests: registers, addressing, every control-line mode, IRQ set/clear/enable, reset,
  edge timing per the mapping above.
- Plugin tests via `SimulationModel` with `SignalSource` drivers: pin directions, per-bit high-Z,
  floating reads as 1, open-drain IRQ, D drive window.
- One or two end-to-end tests with a real `W65C02SCpu` + `FakeMemory` + `W65C21Pia` (e.g. DDRB/ORB
  write, CB2 pulse, CA1 interrupt to CPU IRQB to ISR, read ORA). `W65C21.DigisimPlugin.Tests`
  therefore references `W65C02S.DigisimPlugin`.
- Test comment format and Arrange/Act/Assert per `CLAUDE.md`.

## Milestones (stop for code review after each; user commits or asks for commit)

1. `W65C21.Engine` + `W65C21.Engine.Tests`.
2. `W65C21.DigisimPlugin` + tests + shared plugin test helpers project (move helpers out of
   `W65C02S.DigisimPlugin.Tests`).
3. CPU+PIA integration test; docs (CLAUDE.md project list, Digisim PluginPaths note); CI workflow
   .NET version bumped to 10.

## Out of scope / decided against

- No ADR: floating-reads-1 and the edge mapping are cheap to reverse.
- No internal port pull-ups (needs a weak `InOutPin` in the SDK).
- No N/S variant property.
