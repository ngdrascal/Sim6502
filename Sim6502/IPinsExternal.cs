using System.Diagnostics.CodeAnalysis;
using UInt8 = Sim6502.types.UInt8;

namespace Sim6502;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public interface IPinsExternal
{
    DataBusMode DataBusMode { get; }

    byte VPB { get; }

    RdyPinMode RdyPinMode { get; }

    byte RDY { set; }

    byte PHI1O { get; }

    byte IRQB { set; }

    byte MLB { get; }

    byte NMIB { set; }

    byte SYNC { get; }

    AddrBusMode AddrBusMode { get; }

    byte A0 { get; }

    byte A1 { get; }

    byte A2 { get; }

    byte A3 { get; }

    byte A4 { get; }

    byte A5 { get; }

    byte A6 { get; }

    byte A7 { get; }

    byte A8 { get; }

    byte A9 { get; }

    byte A10 { get; }

    byte A11 { get; }

    byte A12 { get; }

    byte A13 { get; }

    byte A14 { get; }

    byte A15 { get; }

    byte D7 { get; set; }

    byte D6 { get; set; }

    byte D5 { get; set; }

    byte D4 { get; set; }

    byte D3 { get; set; }

    byte D2 { get; set; }

    byte D1 { get; set; }

    byte D0 { get; set; }

    byte RWB { get; }

    byte BE { set; }

    byte PHI2 { set; }

    byte SOB { set; }

    byte PHI2O { get; }

    byte RESB { set; }

    byte DBGSTATE { get; }

    byte DBGSUBSTEP { get; }

    UInt8 DBGINST { get; }
}