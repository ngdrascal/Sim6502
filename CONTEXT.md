# Sim6502

Pin-level simulation of WDC 65xx parts - the W65C02S CPU (cycle-accurate) and the W65C22 VIA - driven one clock edge at a time.

## Language

### Clocking

**PHI2**:
The bus clock input. One PHI2 period is one CPU cycle; the VIA uses it to time every bus transfer.

**Substep**:
One of six fixed phases of a PHI2 cycle; each PHI2 toggle advances the CPU engine one substep.
_Avoid_: microstep, tick

### W65C22 VIA

**VIA**:
The W65C22 Versatile Interface Adapter: two 8-bit peripheral ports with two control lines each, two 16-bit timers and a shift register, behind sixteen CPU-addressable registers. Modelled with W65C22S behavior.
_Avoid_: PIA (that is the W65C21), PIO

**Port**:
The eight peripheral I/O lines PA0-PA7 or PB0-PB7.
_Avoid_: I/O bus

**Output Register**:
The register (ORA, ORB) holding the levels driven on a Port's output lines.

**Input Register**:
What a CPU read of a Port returns (IRA, IRB): pin levels, ORB on Port B output lines, or the value captured by Input latching.

**Data Direction Register**:
The register (DDRA, DDRB) whose bits make each Port line an output (1) or input (0).
_Avoid_: DDR as a standalone word in prose

**Input latching**:
ACR bit 0 (Port A) or 1 (Port B): the CA1/CB1 Active transition captures the Port into the Input Register, held until read.

**Control lines**:
CA1, CA2 (Port A) and CB1, CB2 (Port B), configured by the PCR. CA1 is an input; CA2/CB2 are inputs or outputs; CB1 is also the Shift Register clock.

**Peripheral Control Register (PCR)**:
Configures the Control lines: active edges, input/independent-interrupt modes and the CA2/CB2 output modes.

**Auxiliary Control Register (ACR)**:
Configures the Timers, the Shift Register mode and Input latching.

**Active transition**:
The edge (rising or falling, chosen in the PCR) on a control line input that sets its Interrupt Flag.

**Interrupt Flag**:
One of IFR bits 0-6 (CA2, CA1, SR, CB2, CB1, T2, T1). Set by hardware; cleared by the documented register access or by writing 1 to it in the IFR.
_Avoid_: IRQ bit, status bit

**Interrupt Enable**:
The IER bit that lets the matching Interrupt Flag pull IRQB low. Written with bit 7 choosing set (1) or clear (0).

**Independent interrupt**:
A CA2/CB2 input mode whose flag is not cleared by ORA/ORB accesses, only by writing the IFR.

**Handshake mode**:
A CA2/CB2 output mode where the line goes low after an ORA read (CA2 only) or an ORA/ORB write, and high again on the CA1/CB1 Active transition.

**Pulse mode**:
A CA2/CB2 output mode where the line goes low for one PHI2 cycle after the same accesses.

**Manual output**:
A CA2/CB2 output mode where the line is held low or high by the PCR.

**Timer**:
T1 or T2: a 16-bit counter decrementing at PHI2 falls, with Latches. T1 can run one-shot or free-run and drive PB7; T2 is one-shot or counts PB6 pulses.

**Latch**:
The register a Timer counter is loaded from (T1: 16 bits; T2: low 8 bits only).

**Time-out**:
A Timer counter passing 0 to $FFFF, which sets the Timer's Interrupt Flag at the next PHI2 rise; in T2 pulse counting, reaching 0, which sets it at once.

**Shift Register (SR)**:
An 8-bit register shifting serial data on CB2, clocked on CB1 by PHI2, by T2's low byte, or externally, per ACR bits 4-2.

**Bus hold**:
The W65C22S keeper on Port and control lines: a floating line reads its last level.
