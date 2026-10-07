namespace W65C02S.Engine;

/// <summary>
/// Classifies processor states for the SYNC, VPB and MLB status outputs.
/// </summary>
internal static class BusStatusSignals
{
    private static readonly bool[] OpcodeFetchStates = ToTable(
    [
        States.Fetch,
        // the discarded opcode fetch that starts a hardware interrupt sequence
        States.Interrupt1
    ]);

    private static readonly bool[] VectorPullStates = ToTable(
    [
        States.Boot1, States.Boot2,
        States.InstBRKimp6, States.InstBRKimp7
    ]);

    // the read, modify and write cycles (the last three) of every read-modify-write instruction
    private static readonly bool[] ReadModifyWriteStates = ToTable(
    [
        States.InstASLzpg3, States.InstASLzpg4, States.InstASLzpg5,
        States.InstASLzpgx4, States.InstASLzpgx5, States.InstASLzpgx6,
        States.InstASLabs4, States.InstASLabs5, States.InstASLabs6,
        States.InstASLabsx5, States.InstASLabsx6, States.InstASLabsx7,

        States.InstLSRzpg3, States.InstLSRzpg4, States.InstLSRzpg5,
        States.InstLSRzpgx4, States.InstLSRzpgx5, States.InstLSRzpgx6,
        States.InstLSRabs4, States.InstLSRabs5, States.InstLSRabs6,
        States.InstLSRabsx5, States.InstLSRabsx6, States.InstLSRabsx7,

        States.InstROLzpg3, States.InstROLzpg4, States.InstROLzpg5,
        States.InstROLzpgx4, States.InstROLzpgx5, States.InstROLzpgx6,
        States.InstROLabs4, States.InstROLabs5, States.InstROLabs6,
        States.InstROLabsx5, States.InstROLabsx6, States.InstROLabsx7,

        States.InstRORzpg3, States.InstRORzpg4, States.InstRORzpg5,
        States.InstRORzpgx4, States.InstRORzpgx5, States.InstRORzpgx6,
        States.InstRORabs4, States.InstRORabs5, States.InstRORabs6,
        States.InstRORabsx5, States.InstRORabsx6, States.InstRORabsx7,

        States.InstINCzpg3, States.InstINCzpg4, States.InstINCzpg5,
        States.InstINCzpgx4, States.InstINCzpgx5, States.InstINCzpgx6,
        States.InstINCabs4, States.InstINCabs5, States.InstINCabs6,
        States.InstINCabsx5, States.InstINCabsx6, States.InstINCabsx7,

        States.InstDECzpg3, States.InstDECzpg4, States.InstDECzpg5,
        States.InstDECzpgx4, States.InstDECzpgx5, States.InstDECzpgx6,
        States.InstDECabs4, States.InstDECabs5, States.InstDECabs6,
        States.InstDECabsx5, States.InstDECabsx6, States.InstDECabsx7,

        States.InstTRBzpg3, States.InstTRBzpg4, States.InstTRBzpg5,
        States.InstTRBabs4, States.InstTRBabs5, States.InstTRBabs6,

        States.InstTSBzpg3, States.InstTSBzpg4, States.InstTSBzpg5,
        States.InstTSBabs4, States.InstTSBabs5, States.InstTSBabs6
    ]);

    public static bool IsOpcodeFetch(States state) => OpcodeFetchStates[(int)state];

    public static bool IsVectorPull(States state) => VectorPullStates[(int)state];

    public static bool IsReadModifyWrite(States state) => ReadModifyWriteStates[(int)state];

    // one flag per state value, so each per-cycle status-pin lookup is an array index
    private static bool[] ToTable(States[] states)
    {
        var table = new bool[Enum.GetValues<States>().Length];
        foreach (var state in states)
            table[(int)state] = true;

        return table;
    }
}
