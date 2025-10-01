# Sim6502

## Does the world really need another 6502 simulator?
No, but I have a deep affinity for simulations, particularly for ones that simulate hardware.  Think of robot simulations that include real world physics or old timey CPUs and their support chips.  I built this for my own edification.  I'm sharing it on the off chance someone finds it interesting or maybe even helpful.  Want to get a deep understanding of how something works?  Write a simulator for it.

## History
The original version was written in Java.  I intended to publish it as a plugin to the digital circuit simulator called (of all things) "Digital".  If you are into designing digital circuits then check it out here https://github.com/hneemann/Digital. It’s a very powerful tool.  And it’s open source.

So why port it to C#?  Well, I'm a C# programmer by profession and I was curious about 1) could I take advantage of some of the C# language features to make the design and code more succinct and 2) how long could it possibly take?  After all the two languages are cousins.  Even with GitHub Copilot who came along for the ride it still took a month of evening and weekends.

## Key Design Goals

### 1. Cycle Accuracy
Every instruction executes in the exact number of clock cycles as real hardware, including:
- Variable timing based on addressing modes
- Page boundary crossing penalties
- Interrupt processing delays

### 2. Hardware Fidelity
The simulator models real hardware behavior:
- Two-phase clock system (PHI1/PHI2)
- Separate address and data buses
- Hardware stack in page 1 ($0100-$01FF)
- Pin-level signal simulation

### 3. Modular Architecture
- Instruction families implemented as separate classes
- State registration system promotes decoupling
- Interface-based design for testability

### 4. Comprehensive Testing
- Extensive unit testing
- Using notable test suites for validation

---

## Technical Highlights

### Custom Type System
The simulator uses hardware-accurate data types:
- **UInt8**: 8-bit values with overflow handling
- **UInt16**: 16-bit addresses with proper arithmetic
- **BitFlag**: Individual flag manipulation

### State Machine Implementation
Each instruction phase is modeled as a discrete state:
- **Fetch**: Get next instruction from memory
- **Execute**: Multi-cycle instruction execution
- **Interrupt**: Hardware interrupt processing

### Memory Interface
The memory system provides:
- 64KB address space (full 6502 memory map)
- Memory-mapped I/O support
- Hardware bus simulation with proper timing

---

## Conclusion

The Sim6502 project represents a highly sophisticated emulation of the 65C02S processor that strives to achieve accuracy through:
- Cycle-level timing precision
- Hardware-faithful implementation
- Comprehensive validation testing
- Clean, modular architecture
