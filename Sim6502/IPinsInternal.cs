// ReSharper disable InconsistentNaming
using UInt8 = Sim6502.types.UInt8;
using UInt16 = Sim6502.types.UInt16;

namespace Sim6502;

public interface IPinsInternal
{
    void SetVPB(byte value);

    byte GetRDY();

    void SetRDY(byte value);

    void SetPHI1O(byte value);

    byte GetIRQB();

    void SetMLB(byte value);

    byte GetNMIB();

    void SetSYNC(byte value);

    void SetAddrBusMode(AddrBusMode mode);

    void SetAddrBusPins(UInt16 value);

    void SetDataBusMode(DataBusMode mode);

    UInt8 GetDataBusPins();

    void SetDataBusPins(UInt8 value);

    void SetRWB(byte value);

    byte GetBE();

    byte GetPHI2();

    byte GetSOB();

    void SetPHI2O(byte value);

    byte GetRESB();

    void SetDBGSTATE(byte value);

    void SetDBGSUBSTEP(byte value);

    void SetDBGINST(UInt8 value);
}