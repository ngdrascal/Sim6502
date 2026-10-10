using Digisim.Components;
using Digisim.Engine;
using Digisim.Logging;
using Digisim.Shared;
using Microsoft.Extensions.Logging;
using W65C22.Engine;

namespace W65C22.DigisimPlugin;

/// <summary>
/// WDC W65C22 versatile interface adapter as a Digisim component, driven by <see cref="W65C22Engine"/>.
/// </summary>
/// <remarks>
/// The component only marshals pins; every timing rule lives in the engine, which is evaluated on
/// each scheduler pass. D is driven from the PHI2 rise to the fall of a selected read. Each Port
/// line drives its Output Register bit when its Data Direction Register bit is 1 (PB7 also when
/// T1 owns it) and is high-Z otherwise. IRQB is a totem-pole output (W65C22S), always driven.
/// Port and control lines model the W65C22S bus-holding devices: a floating line reads its last
/// resolved level (1 at start), but the VIA never drives a floating net itself. Floating bus-side
/// inputs (CS1, CS2B, RS0-RS3, RWB, RESB, PHI2) read high.
/// </remarks>
public class W65C22Via : LogicNode, IRegisterComponent
{
    public const string TypeUid = "W65C22";

    private new const string PartName = "W65C22";

    private const int DataBits = 8;

    private const long ByteMask = 0xFF;

    private readonly ILogger _logger;

    private W65C22Engine _engine = null!;

    // Bus-hold levels: the last resolved level of each port and control line.
    private long _paHeld;
    private long _pbHeld;
    private bool _ca1Held;
    private long _ca2Held;
    private long _cb1Held;
    private long _cb2Held;

    // The last read of each input, reused while its pin's Version and the VIA's own drive are unchanged
    private LevelCache _phi2Cache, _resbCache, _cs1Cache, _cs2bCache;
    private LevelCache _rs0Cache, _rs1Cache, _rs2Cache, _rs3Cache, _rwbCache, _ca1Cache;
    private NetCache _dCache, _paCache, _pbCache, _ca2Cache, _cb1Cache, _cb2Cache;

    public static ComponentDescriptor BuildDescriptor()
    {
        return new ComponentDescriptor
            {
                TypeId = TypeUid,
                PartName = PartName,
                Description = "WDC W65C22 versatile interface adapter",
                Category = "Peripherals",
                ClassFullName = typeof(W65C22Via).FullName!,
                Presentation = new ComponentPresentation { Shape = "simple", Height = 10, Width = 4 }
            }
            .AddPin(PinTypes.ClockIn, "PHI2", 1, PinLocations.Left, 1)
            .AddPin(PinTypes.DataIn, "RESB", 2, PinLocations.Left, 2)
            .AddPin(PinTypes.DataIn, "CS1", 3, PinLocations.Left, 3)
            .AddPin(PinTypes.DataIn, "CS2B", 4, PinLocations.Left, 4)
            .AddPin(PinTypes.DataIn, "RS0", 5, PinLocations.Left, 5)
            .AddPin(PinTypes.DataIn, "RS1", 6, PinLocations.Left, 6)
            .AddPin(PinTypes.DataIn, "RS2", 7, PinLocations.Left, 7)
            .AddPin(PinTypes.DataIn, "RS3", 8, PinLocations.Left, 8)
            .AddPin(PinTypes.DataIn, "RWB", 9, PinLocations.Left, 9)
            .AddPin(PinTypes.DataBi, "D", 10, PinLocations.Right, 1, DataBits)
            .AddPin(PinTypes.DataOut, "IRQB", 11, PinLocations.Right, 2)
            .AddPin(PinTypes.DataBi, "PA", 12, PinLocations.Right, 3, DataBits)
            .AddPin(PinTypes.DataIn, "CA1", 13, PinLocations.Right, 4)
            .AddPin(PinTypes.DataBi, "CA2", 14, PinLocations.Right, 5)
            .AddPin(PinTypes.DataBi, "PB", 15, PinLocations.Right, 6, DataBits)
            .AddPin(PinTypes.DataBi, "CB1", 16, PinLocations.Right, 7)
            .AddPin(PinTypes.DataBi, "CB2", 17, PinLocations.Right, 8)
            .AddStringProperty(ComponentDescriptor.LabelProperty);
    }

    public static W65C22Via Create(Guid instanceId, PropertyValueDictionary properties)
    {
        return new W65C22Via(instanceId, properties.GetString(ComponentDescriptor.LabelProperty));
    }

    public W65C22Via(Guid instanceId, string? label)
        : this(instanceId, label, LogManager.GetLogger<W65C22Via>())
    {
    }

    internal W65C22Via(Guid instanceId, string? label, ILogger logger)
        : base(instanceId, PartName, label)
    {
        _logger = logger;

        PHI2 = AddInputPin("PHI2", 1, 1);
        RESB = AddInputPin("RESB", 2, 1);
        CS1 = AddInputPin("CS1", 3, 1);
        CS2B = AddInputPin("CS2B", 4, 1);
        RS0 = AddInputPin("RS0", 5, 1);
        RS1 = AddInputPin("RS1", 6, 1);
        RS2 = AddInputPin("RS2", 7, 1);
        RS3 = AddInputPin("RS3", 8, 1);
        RWB = AddInputPin("RWB", 9, 1);
        D = AddInOutPin("D", 10, DataBits);
        IRQB = AddOutputPin("IRQB", 11, 1);
        PA = AddInOutPin("PA", 12, DataBits);
        CA1 = AddInputPin("CA1", 13, 1);
        CA2 = AddInOutPin("CA2", 14, 1);
        PB = AddInOutPin("PB", 15, DataBits);
        CB1 = AddInOutPin("CB1", 16, 1);
        CB2 = AddInOutPin("CB2", 17, 1);

        CreateEngine();
    }

    public InputPin PHI2 { get; }

    public InputPin RESB { get; }

    public InputPin CS1 { get; }

    public InputPin CS2B { get; }

    public InputPin RS0 { get; }

    public InputPin RS1 { get; }

    public InputPin RS2 { get; }

    public InputPin RS3 { get; }

    public InputPin RWB { get; }

    public InOutPin D { get; }

    public OutputPin IRQB { get; }

    public InOutPin PA { get; }

    public InputPin CA1 { get; }

    public InOutPin CA2 { get; }

    public InOutPin PB { get; }

    public InOutPin CB1 { get; }

    public InOutPin CB2 { get; }

    public override bool IsSequential => true;

    /// <summary>
    /// Test-only: resolve every input on every read instead of reusing the last read.
    /// </summary>
    internal bool DisableReadCache { get; set; }

    public override void Initialize()
    {
        CreateEngine();
    }

    public override void ReadInputs()
    {
        var pins = _engine.Pins;

        pins.PHI2 = Level(PHI2, ref _phi2Cache);
        pins.RESB = Level(RESB, ref _resbCache);
        pins.CS1 = Level(CS1, ref _cs1Cache);
        pins.CS2B = Level(CS2B, ref _cs2bCache);
        pins.RS0 = Level(RS0, ref _rs0Cache);
        pins.RS1 = Level(RS1, ref _rs1Cache);
        pins.RS2 = Level(RS2, ref _rs2Cache);
        pins.RS3 = Level(RS3, ref _rs3Cache);
        pins.RWB = Level(RWB, ref _rwbCache);

        pins.DataIn = (byte)NetLevels(D, ref _dCache, 0, 0, ByteMask);

        _paHeld = NetLevels(PA, ref _paCache, pins.PAOut, pins.PADrive, _paHeld);
        _pbHeld = NetLevels(PB, ref _pbCache, pins.PBOut, pins.PBDrive, _pbHeld);
        _ca1Held = HeldLevel(CA1, ref _ca1Cache, _ca1Held);
        _ca2Held = NetLevels(CA2, ref _ca2Cache, pins.CA2Out ? 1 : 0, pins.CA2Drive ? 1 : 0, _ca2Held);
        _cb1Held = NetLevels(CB1, ref _cb1Cache, pins.CB1Out ? 1 : 0, pins.CB1Drive ? 1 : 0, _cb1Held);
        _cb2Held = NetLevels(CB2, ref _cb2Cache, pins.CB2Out ? 1 : 0, pins.CB2Drive ? 1 : 0, _cb2Held);

        pins.PAIn = (byte)_paHeld;
        pins.PBIn = (byte)_pbHeld;
        pins.CA1 = _ca1Held;
        pins.CA2In = _ca2Held != 0;
        pins.CB1In = _cb1Held != 0;
        pins.CB2In = _cb2Held != 0;
    }

    public override void WriteOutputs()
    {
        _engine.Evaluate();

        var pins = _engine.Pins;
        DriveBus(D, pins.DataOut, pins.DataDrive);
        DriveBus(PA, pins.PAOut, pins.PADrive);
        DriveBus(PB, pins.PBOut, pins.PBDrive);
        DriveBus(CA2, pins.CA2Out ? 1 : 0, pins.CA2Drive ? 1 : 0);
        DriveBus(CB1, pins.CB1Out ? 1 : 0, pins.CB1Drive ? 1 : 0);
        DriveBus(CB2, pins.CB2Out ? 1 : 0, pins.CB2Drive ? 1 : 0);
        IRQB.DriveValue(pins.IRQB ? 1 : 0);
    }

    private void CreateEngine()
    {
        _engine = new W65C22Engine();
        _engine.RegisterAccessed += TraceAccess;
        _paHeld = ByteMask;
        _pbHeld = ByteMask;
        _ca1Held = true;
        _ca2Held = 1;
        _cb1Held = 1;
        _cb2Held = 1;

        _phi2Cache = _resbCache = _cs1Cache = _cs2bCache = LevelCache.Empty;
        _rs0Cache = _rs1Cache = _rs2Cache = _rs3Cache = _rwbCache = _ca1Cache = LevelCache.Empty;
        _dCache = _paCache = _pbCache = _ca2Cache = _cb1Cache = _cb2Cache = NetCache.Empty;
    }

    private void TraceAccess(RegisterAccess access)
    {
        if (!_logger.IsEnabled(LogLevel.Debug))
            return;

        _logger.LogDebug("{Via} {Direction} {Register} ${Value:X2}", Label ?? PartName,
            access.IsWrite ? "write" : "read", access.Register, access.Value);
    }

    // drives the bits in driveMask and floats the rest; a pin driving nothing listens
    private static void DriveBus(InOutPin pin, long value, long driveMask)
    {
        if (driveMask == 0)
            pin.Mode = PinModes.Input;
        else
            pin.Drive(value, ~driveMask & ByteMask);
    }

    // a floating (unconnected or high-Z) bus-side input reads as high, its inactive level
    private bool Level(InputPin pin, ref LevelCache cache)
    {
        return HeldLevel(pin, ref cache, true);
    }

    // a floating control input keeps its last level (bus-holding device)
    private bool HeldLevel(InputPin pin, ref LevelCache cache, bool held)
    {
        // An unchanged Version means the pin is not stale, so resolving again would change nothing
        if (!DisableReadCache && cache.Version == pin.Version && cache.Held == held)
            return cache.Level;

        pin.UpdateFromDrivers();

        var level = pin.ConnectedOutputs.Count == 0 || pin.IsHighZ ? held : pin.GetBool();
        cache = new LevelCache(pin.Version, held, level);
        return level;
    }

    // NetLevels, reused while the pin's Version and every argument are unchanged
    private long NetLevels(InOutPin pin, ref NetCache cache, long ownValue, long ownDrive, long undriven)
    {
        if (!DisableReadCache && cache.Version == pin.Version && cache.OwnValue == ownValue
            && cache.OwnDrive == ownDrive && cache.Undriven == undriven)
            return cache.Levels;

        var levels = NetLevels(pin, ownValue, ownDrive, undriven);
        cache = new NetCache(pin.Version, ownValue, ownDrive, undriven, levels);
        return levels;
    }

    /// <summary>
    /// The level of each bit of the net on <paramref name="pin"/>: this pin's own value on the bits
    /// it drives, otherwise the other drivers on the net (normal strength over weak), and
    /// <paramref name="undriven"/> where nothing drives. Resolved here because an
    /// <see cref="InOutPin"/> that drives some bits does not receive the others.
    /// </summary>
    private static long NetLevels(InOutPin pin, long ownValue, long ownDrive, long undriven)
    {
        long normalValue = 0, normalDriven = 0, weakValue = 0, weakDriven = 0;

        // Indexed: foreach over the IReadOnlyList would box an enumerator on every read.
        var drivers = pin.ConnectedOutputs;
        for (var i = 0; i < drivers.Count; i++)
        {
            var driver = drivers[i];
            var driven = ~driver.DrivenHighZMask;
            if (driver.Strength == DriveStrength.Weak)
            {
                weakValue |= driver.DrivenValue & driven;
                weakDriven |= driven;
            }
            else
            {
                normalValue |= driver.DrivenValue & driven;
                normalDriven |= driven;
            }
        }

        var others = (normalValue & normalDriven)
                     | (weakValue & weakDriven & ~normalDriven)
                     | (undriven & ~(normalDriven | weakDriven));
        var mask = (1L << pin.Bits) - 1;
        return ((ownValue & ownDrive) | (others & ~ownDrive)) & mask;
    }

    // the key and result of the last Level/HeldLevel read of one pin; Version -1 matches no pin
    private readonly record struct LevelCache(long Version, bool Held, bool Level)
    {
        public static readonly LevelCache Empty = new(-1, false, false);
    }

    // the key and result of the last NetLevels read of one pin; Version -1 matches no pin
    private readonly record struct NetCache(long Version, long OwnValue, long OwnDrive, long Undriven, long Levels)
    {
        public static readonly NetCache Empty = new(-1, 0, 0, 0, 0);
    }
}
