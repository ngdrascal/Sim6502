using System.Diagnostics.CodeAnalysis;
using UInt8 = Sim6502.types.UInt8;

namespace Sim6502;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public interface IPinsExternal
{
    DataBusMode GetDataBusMode();

    byte VPB { get; }

    RdyPinMode GetRdyPinMode();

    byte RDY { set; }

    byte PHI1O { get; }

    byte IRQB { set; }

    byte MLB { get; }

    byte NMIB { set; }

    byte SYNC { get; }

    AddrBusMode GetAddrBusMode();

    byte GetA0();

    byte GetA1();

    byte GetA2();

    byte GetA3();

    byte GetA4();

    byte GetA5();

    byte GetA6();

    byte GetA7();

    byte GetA8();

    byte GetA9();

    byte GetA10();

    byte GetA11();

    byte GetA12();

    byte GetA13();

    byte GetA14();

    byte GetA15();

    byte GetD7();

    void SetD7(byte value);

    byte GetD6();

    void SetD6(byte value);

    byte GetD5();

    void SetD5(byte value);

    byte GetD4();

    void SetD4(byte value);

    byte GetD3();

    void SetD3(byte value);

    byte GetD2();

    void SetD2(byte value);

    byte GetD1();

    void SetD1(byte value);

    byte GetD0();

    void SetD0(byte value);

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