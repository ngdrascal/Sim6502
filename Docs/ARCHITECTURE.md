# Sim6502 Architecture Documentation

## Overview

Sim6502 is a highly accurate, cycle-level simulator for the WDC 65C02S microprocessor. It aims to achieve hardware-level fidelity by modeling not just the instruction execution but also the actual timing, pin states, and internal operations of the real processor.

## Key Design Principles

### 1. Cycle-Level Accuracy
Every instruction executes in the exact number of clock cycles as the real hardware, including:
- Variable timing based on addressing modes
- Page boundary crossing penalties  
- Interrupt processing delays
- Two-phase clock system (PHI1/PHI2) with proper substep timing

### 2. Hardware Fidelity
The simulator models real hardware behavior including:
- Pin-level signal simulation
- Separate address and data buses
- Hardware stack in page 1 ($0100-$01FF)
- Proper read/write timing
- Reset and interrupt handling

### 3. Modular Architecture
- Instruction families implemented as separate classes
- State machine-based execution model
- Interface-based design for testability and extensibility
- Decoupled components with event-driven communication

## Core Architecture

### System Components

```
???????????????????    ???????????????????    ???????????????????
?   W65C02SEngine ??????     Context     ??????   Instructions  ?
???????????????????    ???????????????????    ???????????????????
         ?                       ?                       ?
         ?                       ?                       ?
???????????????????    ???????????????????    ???????????????????
?      Pins       ?    ?   Registers     ?    ?     States      ?
???????????????????    ???????????????????    ???????????????????
```

### Core Classes

#### W65C02SEngine
The main simulation engine that orchestrates the entire system:
- **Clock Management**: Handles the two-phase clock system (PHI1/PHI2)
- **State Machine**: Executes the current processor state
- **Instruction Dispatch**: Maps opcodes to instruction implementations
- **Interrupt Handling**: Processes NMI, IRQ, and RESET signals
- **Cycle Counting**: Tracks instruction and cycle counts

```csharp
public class W65C02SEngine : IT2Registry, IStateRegistry
{
    private readonly Context _ctx;
    private readonly States[] _t2Map;           // Opcode ? State mapping
    private readonly Action<Context>[] _stateMethodMap;  // State ? Method mapping
}
```

#### Context
The central data container that provides access to all system components:
- **State Management**: Current execution state and substep tracking
- **Component Access**: Unified access to pins, registers, and flags
- **Event Coordination**: State change notifications
- **Debugging Support**: Debug information tracking

```csharp
public class Context
{
    public IPinsInternal Pins { get; }
    public Registers Regs { get; }
    public States State { get; private set; }
    public bool CrossedPageBoundary { get; set; }
}
```

## State Machine Architecture

### Execution States

The simulator uses a comprehensive state machine with different categories of states:

#### System States
- **WarmUp0-2**: Initial synchronization with clock
- **Boot1-2**: Reset sequence and vector loading
- **Fetch**: Instruction fetch from memory
- **NotReady**: RDY pin handling
- **Stop**: Processor halt state

#### Instruction States
Each instruction family has its own set of states representing the individual cycles of execution. For example, LDA (Load Accumulator) has states like:
- `InstLDAimm2`: LDA immediate addressing, cycle 2
- `InstLDAzpg2-3`: LDA zero page addressing, cycles 2-3
- `InstLDAabs2-4`: LDA absolute addressing, cycles 2-4

### State Transition Flow

```
Power On ? WarmUp0 ? WarmUp1 ? WarmUp2 ? Boot1 ? Boot2 ? Fetch ?
                                                            ?
                                                            ?
                                                   Instruction States
                                                            ?
                                                            ?
                                                         Fetch ?
```

### Substep System

Each processor state is divided into substeps that correspond to the two-phase clock:

1. **Substep 1**: PHI1 rising edge - Internal operations
2. **Substep P1MiddleStep**: PHI1 middle - Address setup
3. **Substep P2MiddleStep**: PHI2 middle - Data setup  
4. **Substep P2LastSubstep**: PHI2 falling edge - Data capture, state advance

## Custom Type System

### Hardware-Accurate Data Types

The simulator uses custom types that model the behavior of actual hardware registers:

#### UInt8 - 8-bit Values
```csharp
public class UInt8
{
    // Arithmetic with proper overflow/underflow
    public UInt8 Inc()  // Wraps 255 ? 0
    public UInt8 Dec()  // Wraps 0 ? 255
    
    // Bitwise operations
    public bool IsBitSet(int bitIndex)
    public void SetBit(int bitIndex)
    public void ClearBit(int bitIndex)
    
    // 6502-specific operations
    public MathResult Adc(UInt8 operand, BitFlag carryIn, BitFlag decimalMode)
    public MathResult Sbc(UInt8 operand, BitFlag carryIn, BitFlag decimalMode)
}
```

#### UInt16 - 16-bit Addresses
```csharp
public class UInt16
{
    public UInt8 Lsb()  // Access low byte
    public UInt8 Msb()  // Access high byte
    public UInt16 AddUnsigned(UInt8 operand)  // Page boundary detection
}
```

#### BitFlag - Individual Flag States
```csharp
public class BitFlag
{
    public bool IsSet()
    public bool IsCleared()
    public int ToInt()
}
```

## Instruction Implementation

### Modular Instruction Classes

Each instruction family is implemented as a separate class inheriting from `InstBase`:

```csharp
internal class InstLDA : InstBase, IInstruction
{
    // Register opcode mappings
    public IInstruction RegisterT2State(IT2Registry registry)
    
    // Register state method mappings  
    public IInstruction RegisterStates(IStateRegistry stateRegistry)
    
    // Individual addressing mode implementations
    private void Imm2(Context ctx)      // Immediate
    private void Zpg2(Context ctx)      // Zero Page
    private void Abs2(Context ctx)      // Absolute
    // ... etc
}
```

### Addressing Mode Implementation

Each addressing mode is implemented as a series of states that model the actual hardware timing:

#### Example: LDA Immediate (`LDA #$42`)
```csharp
private void Imm2(Context ctx)
{
    Imm2SetAddrBus(ctx);  // Set PC on address bus
    if (ctx.GetSubStep() == P2LastSubstep)
    {
        var data = ctx.Pins.DataBus;  // Read operand
        ctx.Regs.UpdateAUpdateFlags(data);  // Load A, update N,Z flags
        ctx.DbgOperand1 = data;  // Debug tracking
    }
    ctx.AdvanceState(States.Fetch);  // Return to fetch
}
```

#### Example: LDA Absolute,X with Page Crossing (`LDA $1234,X`)
```csharp
private void Absx4(Context ctx)
{
    var nextState = States.Fetch;
    if (ctx.GetSubStep() == P1MiddleStep)
    {
        var beforePage = ctx.Regs.EA.Msb().Copy();
        ctx.Regs.IncEAWithX();  // Add X to effective address
        var afterPage = ctx.Regs.EA.Msb().Copy();
        ctx.CrossedPageBoundary = !afterPage.Equals(beforePage);
        ctx.Pins.AddrBus = ctx.Regs.EA;
        ctx.Pins.RWB = Read;
    }
    else if (ctx.GetSubStep() == P2LastSubstep)
    {
        var data = ctx.Pins.DataBus;
        if (!ctx.CrossedPageBoundary)
            ctx.Regs.UpdateAUpdateFlags(data);  // 4 cycles if no page cross
        else
            nextState = States.InstLDAabsx5;    // 5 cycles if page crossed
    }
    ctx.AdvanceState(nextState);
}
```

## Memory Interface

### Bus Simulation

The simulator models the actual 6502 bus system:

```csharp
public interface IPinsInternal
{
    // Address bus (16-bit)
    UInt16 AddrBus { get; set; }
    AddrBusMode AddrBusMode { get; set; }
    
    // Data bus (8-bit)  
    UInt8 DataBus { get; set; }
    DataBusMode DataBusMode { get; set; }
    
    // Control signals
    byte RWB { get; set; }      // Read/Write
    byte PHI2 { get; set; }     // Clock
    byte RDY { get; set; }      // Ready
    byte RESB { get; set; }     // Reset
    byte IRQB { get; set; }     // IRQ
    byte NMIB { get; set; }     // NMI
}
```

### Memory Access Timing

All memory accesses follow the proper 6502 timing:
1. **PHI1**: Address setup and internal operations
2. **PHI1 Middle**: Address bus driven
3. **PHI2 Middle**: Data bus setup (write) 
4. **PHI2 End**: Data capture (read) or latch (write)

## Register System

### 6502 Registers

```csharp
public class Registers
{
    public UInt8 A { get; }      // Accumulator
    public UInt8 X { get; }      // X Index
    public UInt8 Y { get; }      // Y Index  
    public UInt16 PC { get; }    // Program Counter
    public UInt8 S { get; }      // Stack Pointer
    public StatusRegister P { get; } // Processor Status
}
```

### Status Register (P)
```csharp
public class StatusRegister
{
    public BitFlag Negative { get; }     // N - Bit 7
    public BitFlag Overflow { get; }     // V - Bit 6  
    public BitFlag Decimal { get; }      // D - Bit 3
    public BitFlag IRQDisabled { get; }  // I - Bit 2
    public BitFlag Zero { get; }         // Z - Bit 1
    public BitFlag Carry { get; }        // C - Bit 0
}
```

### Working Registers
```csharp
// Internal working registers
public UInt16 EA { get; }    // Effective Address
public UInt16 EA2 { get; }   // Secondary Effective Address (for indirect)
public UInt8 Temp { get; }   // Temporary storage
public UInt8 Inst { get; }   // Current instruction
```

## Interrupt System

### Interrupt Types

1. **RESET**: Hardware reset
   - Detected by RESB pin low for 2+ cycles
   - Triggers Boot1 ? Boot2 sequence
   - Loads PC from $FFFC/$FFFD

2. **NMI**: Non-Maskable Interrupt  
   - Edge-triggered on NMIB pin
   - Cannot be disabled by I flag
   - Vectors through $FFFA/$FFFB

3. **IRQ**: Maskable Interrupt
   - Level-triggered on IRQB pin  
   - Disabled when I flag set
   - Vectors through $FFFE/$FFFF

### Interrupt Processing

All interrupts follow the same 7-cycle sequence:
1. **Cycle 1**: Finish current instruction
2. **Cycle 2**: Push PCH to stack
3. **Cycle 3**: Push PCL to stack  
4. **Cycle 4**: Push P to stack (B flag handling differs)
5. **Cycle 5**: Fetch vector low byte
6. **Cycle 6**: Fetch vector high byte  
7. **Cycle 7**: Continue at interrupt handler

## Testing and Validation

### Comprehensive Test Suite

The simulator includes multiple validation approaches:

#### Unit Tests (`Sim6502.Tests`)
- Individual instruction testing
- Addressing mode verification
- Flag behavior validation
- Edge case coverage

#### Integration Tests (`Sim6502.ValidationSuites`) 
- **Klaus Dormann Functional Test**: Comprehensive 6502 instruction test
- **65C02 Extended Opcodes Test**: Validation of 65C02-specific instructions  
- **Bruce Clark BCD Test**: Decimal mode arithmetic verification

### Test Infrastructure

```csharp
public class ValidationTests
{
    public void RunKlausDormannTest()
    {
        var program = LoadBinaryFromFile(@".\Tests\6502_functional_test.bin");
        var simulator = new Simulator();
        simulator.LoadAndRunProgram(program, 0x0000, 0x0400);
    }
}
```

## Performance Characteristics

### Execution Speed
- Simulates at approximately 1-2 MHz on modern hardware
- Cycle-accurate timing maintained at all speeds
- Memory access patterns match real hardware

### Memory Usage  
- 64KB simulated memory space
- Minimal overhead for register and state tracking
- Efficient state machine implementation

### Accuracy Metrics
- **Cycle Accuracy**: 100% - Every instruction takes exact hardware cycles
- **Timing Accuracy**: Sub-cycle precision with PHI1/PHI2 modeling
- **Behavioral Accuracy**: Passes comprehensive test suites

## Usage Examples

### Basic Program Execution

```csharp
// Create simulator instance
var simulator = new Simulator();

// Load program binary
byte[] program = LoadBinaryFromFile("program.bin");

// Execute starting at address $0400
simulator.LoadAndRunProgram(program, 0x0000, 0x0400);

// Check results
var accumulator = simulator.Peek(0x00);  // Read memory location
var instructionCount = simulator.InstCount;
```

### Custom Hardware Integration

```csharp
// Access pin states for hardware simulation
var pins = simulator.Pins;
pins.IRQB = 0;  // Assert IRQ
pins.RDY = 0;   // Halt processor

// Monitor bus activity  
simulator.StateChanging += (sender, args) => {
    Console.WriteLine($"State: {args.NewState}");
    Console.WriteLine($"Address Bus: ${pins.AddrBus:X4}");
    Console.WriteLine($"Data Bus: ${pins.DataBus:X2}");
};
```

## Future Enhancements

### Planned Features
- **Disassembler Integration**: Real-time instruction disassembly
- **Breakpoint System**: Debugging and analysis tools
- **Performance Profiling**: Instruction frequency analysis
- **Hardware Peripherals**: Simulation of common 6502 system chips

### Extensibility Points
- **Custom Instructions**: Plugin system for undocumented opcodes
- **Memory Mapping**: Configurable memory and I/O regions  
- **Bus Monitoring**: External device simulation interface
- **Timing Analysis**: Detailed performance measurement tools

## Conclusion

Sim6502 represents a sophisticated approach to processor simulation that prioritizes accuracy over speed. By modeling the actual hardware behavior at the cycle level, it provides an invaluable tool for understanding the 6502 architecture, validating software behavior, and exploring the intricacies of one of computing history's most influential processors.

The modular design, comprehensive test coverage, and hardware-faithful implementation make it suitable for both educational purposes and serious development work requiring precise 6502 behavior simulation.