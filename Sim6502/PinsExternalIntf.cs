// Converted from PinsExternalIntf.java
// External pin interface for W65c02s
using Us.Retrocpu.Shared;
using Us.Retrocpu.W65c02s;

public interface PinsExternalIntf
{
    DataBusMode GetDataBusMode();

    byte GetVPB();

    RdyPinMode GetRdyPinMode();

    byte GetRDY();

    void SetRDY(byte value);

    byte GetPHI1O();

    void SetIRQB(byte value);

    byte GetMLB();

    void SetNMIB(byte value);

    byte GetSYNC();

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

    byte GetRWB();

    void SetBE(byte value);

    void SetPHI2(byte value);

    void SetSOB(byte value);

    byte GetPHI2O();

    void SetRESB(byte value);

    byte GetDBGSTATE();

    byte GetDBGSUBSTEP();

    UInt8 GetDBGINST();
}
