#pragma warning disable IDE0051
// ReSharper disable InconsistentNaming
using UInt8 = Sim6502.types.UInt8;
using UInt16 = Sim6502.types.UInt16;


namespace Sim6502;

public interface IPinsInternal
{
    byte VPB { set; }

    byte RDY { get; }

    byte PHI1O { set; }

    byte IRQB { get; }

    byte MLB { set; }

    byte NMIB { get; }

    byte SYNC { set; }

    AddrBusMode AddrBusMode { set; }

    UInt16 AddrBus { get; set; }

    DataBusMode DataBusMode { set; }

    UInt8 DataBus { get; set; }

    byte RWB { set; }

    byte BE { get; }

    byte PHI2 { get; }

    byte SOB { get; }

    byte PHI2O { set; }

    byte RESB { get; }

    byte DBGSTATE { set; }

    byte DBGSUBSTEP { set; }

    UInt8 DBGINST { set; }
}