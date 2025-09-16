namespace Sim6502;

public interface IT2Registry
{
    void Map(OpCodes opCode, States state);
}
