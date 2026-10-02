# W65C22 VIA - Digisim Component Design

Status: agreed and implemented 2026-10-01; replaces the earlier W65C21 PIA. Datasheet:
`Docs/DataSheets/w65c22.pdf`. Vocabulary: see `CONTEXT.md` (W65C22 VIA section). Template to
follow: `W65C02S.DigisimPlugin/W65C02SCpu.cs`.

## Goal

A pin-waveform-accurate W65C22 VIA usable as a Digisim plugin component. Names use `W65C22` (no
suffix) but behavior follows the W65C22S (totem-pole IRQB, bus-holding pins). Digisim has no
propagation delays, so "waveform accurate" means every output changes on the correct PHI2 edge or
input transition as shown in the datasheet figures.

## Digisim facts that constrain the design

- Pins support per-bit high-Z via `Drive(value, highZMask)`, so one 8-bit `InOutPin` per port can
  mix input and output bits.
- `InOutPin` is always `DriveStrength.Normal`; an `InOutPin` that drives any bit stops receiving,
  so input levels are resolved from `ConnectedOutputs` (normal over weak).
- `Digisim:PluginPaths` is a list, so a second plugin output folder can be added.
- Plugins reference `Digisim.Sdk` with `ExcludeAssets="runtime"`; tests reference `Digisim.Engine`
  from the local feed `C:\nuget-local`.

## Projects (all in `Sim6502.slnx`)

| Project | Purpose |
|---|---|
| `W65C22.Engine` | VIA logic. No dependencies (plain `byte`/`bool`). |
| `W65C22.Engine.Tests` | xUnit v3 unit tests; base class `ViaTestBase`. |
| `W65C22.DigisimPlugin` | `W65C22Via` component; pin marshalling and bus hold only. |
| `W65C22.DigisimPlugin.Tests` | Tests through the real Digisim scheduler (`ViaBench`), plus CPU+memory+VIA tests (`SystemBench`). |
| `DigisimPlugin.TestHelpers` | `SignalSource`, `CapturingLogger`, `FakeMemory`, shared with the CPU plugin tests. |

Engine and plugin grant `InternalsVisibleTo` to their test projects. Coverage >= 90% per project.

## Engine structure

- `W65C22Engine` owns `W65C22Pins` (inputs set by the host; outputs are value + drive flag/mask)
  and coordinates: bus cycles, IFR/IER/IRQB, ports and input latching.
- `ControlLines` - one PCR nibble: CA1/CA2 or CB1/CB2 active transitions and the C2 output level.
- `Timer1`, `Timer2` - counters, latches, PB7 level, time-out timing.
- `ShiftRegister` - SR, bit counter, internal CB1 clock and CB2 data.
- Host calls `Evaluate()` after any input change; the engine detects PHI2 edges, control-line
  transitions and RESB itself.

## Component (`W65C22Via`)

- TypeUid `"W65C22"`, PartName `W65C22`, category `"Peripherals"`.
- Pins - left: PHI2 (ClockIn), RESB, CS1, CS2B, RS0-RS3, RWB. Right: D[8] (bi), IRQB, PA[8] (bi),
  CA1 (in), CA2 (bi), PB[8] (bi), CB1 (bi, shift clock out), CB2 (bi).
- Floating bus-side inputs (CS1, CS2B, RS0-RS3, RWB, RESB, PHI2) read high. A floating CS2B
  therefore deselects the part.
- Port and control lines model the W65C22S bus-holding devices: a floating line reads its last
  resolved level (1 after `Initialize`). The VIA never drives a floating net itself.
- IRQB is totem-pole: always driven, low while any enabled interrupt is flagged.
- Debug-level logging of register accesses via `LogManager.GetLogger<W65C22Via>()`.

## Registers (Table 2-1)

| RS | Write | Read |
|---|---|---|
| 0 | ORB | IRB |
| 1 | ORA (handshake) | IRA (handshake) |
| 2 | DDRB | DDRB |
| 3 | DDRA | DDRA |
| 4 | T1 low latch | T1 counter low (clears IFR6) |
| 5 | T1 high latch + load counter (clears IFR6) | T1 counter high |
| 6 | T1 low latch | T1 low latch |
| 7 | T1 high latch (clears IFR6) | T1 high latch |
| 8 | T2 low latch | T2 counter low (clears IFR5) |
| 9 | T2 counter high + load (clears IFR5) | T2 counter high |
| A | SR | SR |
| B | ACR | ACR |
| C | PCR | PCR |
| D | IFR (1 bits clear flags) | IFR (bit 7 = any enabled flag) |
| E | IER (bit 7 = set/clear) | IER (bit 7 reads 1) |
| F | ORA, no handshake | IRA, no handshake |

Selected when CS1=1 and CS2B=0.

Ports: IRA returns the PA pin levels (all bits); IRB returns ORB on output bits and pin levels on
input bits. With input latching (ACR0/ACR1) the CA1/CB1 active transition captures the pins; the
captured value is read once, then the register is transparent again (section 2.1).

## Timing (edge mapping)

PHI2 rise:
- Sample CS1/CS2B, RS0-RS3, RWB; a selected read drives D from rise to fall with the value
  captured at the rise and applies read side effects (flag clears, SR access) there.
- Timer time-outs due from the previous fall set IFR6/IFR5 and change PB7.
- T2 pulse counting samples PB6; shift-in samples CB2.
- Write handshake / pulse: CA2 (after ORA write) and CB2 (after ORB write) go low; a pulse returns
  high at the next rise.

PHI2 fall:
- Timers and the SR clock count, then a selected write latches D into its register (so a counter
  load at this fall is not decremented).
- Read handshake / pulse: CA2 goes low at the fall ending an ORA read; a pulse returns high at the
  next fall.
- Shift out puts SR7 on CB2 at the first fall after a CB1 fall.

Asynchronous:
- CA1/CB1, and CA2/CB2 in input modes: the active transition sets the flag (IRQB follows at once)
  and, with latching enabled, captures the port. In handshake mode CA1/CB1 sets CA2/CB2 high.
- External shift clock edges on CB1 are noticed at once and acted on at the PHI2 edges above.
- RESB low (level): ORA, ORB, DDRA, DDRB, ACR, PCR, IFR, IER cleared; all pins inputs; IRQB high;
  timers and SR stop (counters, latches and SR contents kept); bus cycles ignored while low.

## Timers

- T1: a T1C-H write loads the counter at that fall and takes PB7 low. The counter decrements at
  every later fall; on 0 to $FFFF the time-out lands at the next rise (N+1.5 cycles, Figure 2-3)
  and the counter reloads from the latch at the following fall (free-run period N+2, Figure 2-4).
  One-shot sets IFR6 once per load and returns PB7 high; free-run sets it every time-out and
  toggles PB7. ACR7=1 drives PB7 from T1 regardless of DDRB7.
- T2 interval: like T1 but rolls over without reload and sets IFR5 once per T2C-H write.
- T2 pulse counting (ACR5): PB6 sampled at each rise; a high to low change decrements.
- Timers do not count after power-on or RESB until their C-H register is written.

## Shift Register

| ACR 4-2 | CB1 | CB2 | Clock | Stops after 8 |
|---|---|---|---|---|
| 000 | PCR | PCR | - | - |
| 001 | out | in | T2 low byte | yes |
| 010 | out | in | PHI2 | yes |
| 011 | in | in | external CB1 | no (IFR2 every 8) |
| 100 | out | out | T2 low byte | never (no IFR2) |
| 101 | out | out | T2 low byte | yes |
| 110 | out | out | PHI2 | yes |
| 111 | in | out | external CB1 | no (IFR2 every 8) |

- Any SR read (at its rise) or write (at its fall) clears IFR2, resets the bit counter and starts a
  transfer in the internal clock modes.
- PHI2 modes toggle CB1 at every fall (one bit per 2 cycles). T2 modes reload T2's low byte from
  T2L-L at the SR access; it decrements each fall and on 0 to $FF toggles CB1 and reloads on the
  next fall (half-period N+2).
- Shift in: CB2 sampled at the first rise after a CB1 rise into bit 0. Shift out: SR7 to CB2 at the
  first fall after a CB1 fall, rotated into bit 0.
- The bit counter advances on CB1 rises; IFR2 at the 8th (shift in: at the sampling rise).
- Internal modes ignore the CB1 net (section 5.2); the CB1 flag (IFR4) still follows the net,
  including the VIA's own clock. Mode 000 clears and holds IFR2 low.

## Ambiguities resolved

- Register $F reads/writes ORA without handshake and without clearing the CA1/CA2 flags.
- Independent interrupt C2 modes: ORx accesses leave the C2 flag set.
- Entering handshake or pulse mode leaves CA2/CB2 high; CA2/CB2 in output mode set no flag.
- CA2 read handshake is triggered by an ORA read, write handshake by an ORA write; CB2 has only
  the write handshake.
- Register 7 read returns the T1 high latch (Table 2-1; Table 2-7 says counter), as the MOS 6522.
- T1 one-shot reloads from the latch after its time-out (Figure 2-3) but sets no further flag.
- T2 pulse counting sets IFR5 on reaching 0 (section 2.10), interval mode on 0 to $FFFF.
- A T1C-H write on the same fall as an underflow wins and cancels that time-out.
- IRB bit 7 reads the T1 PB7 level while ACR7=1.
- While a shift mode uses T2 (001, 100, 101), T2's high byte is held and T2 sets no IFR5.
- In T2 shift modes the first CB1 edge comes N+1 cycles after the SR access, later edges every N+2.
- CB2 rests at its previous level (high after power-on) until the first bit shifts out.

## Tests

- Engine tests: registers and addressing, ports and latching, every PCR mode, IFR/IER/IRQB,
  timers, every shift mode, reset, edge timing.
- Plugin tests via `SimulationModel` with `SignalSource` drivers: descriptor, pin directions,
  per-bit high-Z, bus hold, totem-pole IRQB, D drive window, CB1 as shift clock output and input.
- CPU integration (`CpuViaIntegrationTests`): DDRB/ORB write and IRB read; T1 free-run interrupt
  to an ISR acknowledging through T1C-L; CA1 interrupt to an ISR reading IRA.
- Test comment format and Arrange/Act/Assert per `CLAUDE.md`.

## Out of scope / decided against

- No N/S variant property; the S behavior is modelled.
- No internal port pull-ups (bus hold covers floating lines).
