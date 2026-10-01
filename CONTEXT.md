# Sim6502

Pin-level simulation of WDC 65xx parts - the W65C02S CPU (cycle-accurate) and the W65C21 PIA - driven one clock edge at a time.

## Language

### Clocking

**PHI2**:
The bus clock input. One PHI2 period is one CPU cycle; the PIA uses it to time every bus transfer.

**Substep**:
One of six fixed phases of a PHI2 cycle; each PHI2 toggle advances the CPU engine one substep.
_Avoid_: microstep, tick

### W65C21 PIA

**PIA**:
The W65C21 Peripheral Interface Adapter: two 8-bit peripheral ports, each with two control lines, behind four CPU-addressable locations.
_Avoid_: VIA (that is the W65C22), PIO

**Side**:
One of the two independent halves of the PIA (A or B), each with its own Port, Control Register, Data Direction Register, Output Register and IRQ line.

**Port**:
The eight peripheral I/O lines of a Side (PA0-PA7 or PB0-PB7).
_Avoid_: Peripheral Interface (the datasheet's term for the register selection), I/O bus

**Output Register**:
The register (ORA, ORB) holding the levels driven on a Port's output lines.
_Avoid_: Peripheral Register, IRA/IRB

**Data Direction Register**:
The register (DDRA, DDRB) whose bits make each Port line an output (1) or input (0).
_Avoid_: DDR as a standalone word in prose

**Control Register**:
The register (CRA, CRB) configuring a Side's control lines and holding its two Interrupt Flags.

**DDR Access bit**:
Bit 2 of a Control Register; it chooses whether that Side's data location reaches the Output Register/Port (1) or the Data Direction Register (0).

**Control lines**:
CA1, CA2 (Side A) and CB1, CB2 (Side B). CA1/CB1 are inputs only; CA2/CB2 are inputs or outputs per the Control Register.

**Active transition**:
The edge (rising or falling, chosen in the Control Register) on a control line input that sets its Interrupt Flag.

**Interrupt Flag**:
Control Register bit 7 (set by CA1/CB1) or bit 6 (set by CA2/CB2 as input); read-only to the CPU.
_Avoid_: IRQ bit, status bit

**Read A Data**:
A CPU read of Side A's data location with the DDR Access bit set; it clears Side A's Interrupt Flags and drives CA2 handshake/pulse.

**Write B Data**:
A CPU write of Side B's data location with the DDR Access bit set; it drives CB2 handshake/pulse.

**Handshake mode**:
A CA2/CB2 output mode where the line is cleared by Read A Data / Write B Data and set again by the active transition on CA1/CB1.

**Pulse mode**:
A CA2/CB2 output mode where the line goes low for one PHI2 cycle after Read A Data / Write B Data.

**Manual output**:
A CA2/CB2 output mode where the line follows Control Register bit 3.
