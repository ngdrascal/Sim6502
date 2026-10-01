# Sim6502

Pin-level, cycle-accurate simulation of the WDC W65C02S CPU, driven one clock edge at a time.

## Language

**PHI2**:
The bus clock input. One PHI2 period is one CPU cycle.

**Substep**:
One of six fixed phases of a PHI2 cycle; each PHI2 toggle advances the engine one substep.
_Avoid_: microstep, tick
