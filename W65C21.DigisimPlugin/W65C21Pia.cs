using Digisim.Components;
using Digisim.Engine;
using Digisim.Logging;
using Digisim.Shared;
using Microsoft.Extensions.Logging;
using W65C21.Engine;

namespace W65C21.DigisimPlugin;

/// <summary>
/// WDC W65C21 peripheral interface adapter as a Digisim component, driven by <see cref="W65C21Engine"/>.
/// </summary>
/// <remarks>
/// The component only marshals pins; every timing rule lives in the engine, which is evaluated on
/// each scheduler pass. D is driven from the PHI2 rise to the fall of a selected read. Each Port
/// line drives its Output Register bit when its Data Direction Register bit is 1 and is high-Z
/// otherwise. IRQAB/IRQBB are open drain (low or high-Z). A floating control input reads as high
/// and a floating Port line reads as 1, but the PIA never drives a floating net itself (no internal
/// pull-ups).
/// </remarks>
public class W65C21Pia : LogicNode, IRegisterComponent
{
    public const string TypeUid = "W65C21";

    private new const string PartName = "W65C21";

    private const int DataBits = 8;

    private const long ByteMask = 0xFF;

    private readonly ILogger _logger;

    private W65C21Engine _engine = null!;

    public static ComponentDescriptor BuildDescriptor()
    {
        return new ComponentDescriptor
            {
                TypeId = TypeUid,
                PartName = PartName,
                Description = "WDC W65C21 peripheral interface adapter",
                Category = "Peripherals",
                ClassFullName = typeof(W65C21Pia).FullName!,
                Presentation = new ComponentPresentation { Shape = "simple", Height = 10, Width = 4 }
            }
            .AddPin(PinTypes.ClockIn, "PHI2", 1, PinLocations.Left, 1)
            .AddPin(PinTypes.DataIn, "RESB", 2, PinLocations.Left, 2)
            .AddPin(PinTypes.DataIn, "CS0", 3, PinLocations.Left, 3)
            .AddPin(PinTypes.DataIn, "CS1", 4, PinLocations.Left, 4)
            .AddPin(PinTypes.DataIn, "CS2B", 5, PinLocations.Left, 5)
            .AddPin(PinTypes.DataIn, "RS0", 6, PinLocations.Left, 6)
            .AddPin(PinTypes.DataIn, "RS1", 7, PinLocations.Left, 7)
            .AddPin(PinTypes.DataIn, "RWB", 8, PinLocations.Left, 8)
            .AddPin(PinTypes.DataBi, "D", 9, PinLocations.Right, 1, DataBits)
            .AddPin(PinTypes.DataOut, "IRQAB", 10, PinLocations.Right, 2)
            .AddPin(PinTypes.DataOut, "IRQBB", 11, PinLocations.Right, 3)
            .AddPin(PinTypes.DataBi, "PA", 12, PinLocations.Right, 4, DataBits)
            .AddPin(PinTypes.DataIn, "CA1", 13, PinLocations.Right, 5)
            .AddPin(PinTypes.DataBi, "CA2", 14, PinLocations.Right, 6)
            .AddPin(PinTypes.DataBi, "PB", 15, PinLocations.Right, 7, DataBits)
            .AddPin(PinTypes.DataIn, "CB1", 16, PinLocations.Right, 8)
            .AddPin(PinTypes.DataBi, "CB2", 17, PinLocations.Right, 9)
            .AddStringProperty(ComponentDescriptor.LabelProperty);
    }

    public static W65C21Pia Create(Guid instanceId, PropertyValueDictionary properties)
    {
        return new W65C21Pia(instanceId, properties.GetString(ComponentDescriptor.LabelProperty));
    }

    public W65C21Pia(Guid instanceId, string? label)
        : this(instanceId, label, LogManager.GetLogger<W65C21Pia>())
    {
    }

    internal W65C21Pia(Guid instanceId, string? label, ILogger logger)
        : base(instanceId, PartName, label)
    {
        _logger = logger;

        PHI2 = AddInputPin("PHI2", 1, 1);
        RESB = AddInputPin("RESB", 2, 1);
        CS0 = AddInputPin("CS0", 3, 1);
        CS1 = AddInputPin("CS1", 4, 1);
        CS2B = AddInputPin("CS2B", 5, 1);
        RS0 = AddInputPin("RS0", 6, 1);
        RS1 = AddInputPin("RS1", 7, 1);
        RWB = AddInputPin("RWB", 8, 1);
        D = AddInOutPin("D", 9, DataBits);
        IRQAB = AddOutputPin("IRQAB", 10, 1);
        IRQBB = AddOutputPin("IRQBB", 11, 1);
        PA = AddInOutPin("PA", 12, DataBits);
        CA1 = AddInputPin("CA1", 13, 1);
        CA2 = AddInOutPin("CA2", 14, 1);
        PB = AddInOutPin("PB", 15, DataBits);
        CB1 = AddInputPin("CB1", 16, 1);
        CB2 = AddInOutPin("CB2", 17, 1);

        CreateEngine();
    }

    public InputPin PHI2 { get; }

    public InputPin RESB { get; }

    public InputPin CS0 { get; }

    public InputPin CS1 { get; }

    public InputPin CS2B { get; }

    public InputPin RS0 { get; }

    public InputPin RS1 { get; }

    public InputPin RWB { get; }

    public InOutPin D { get; }

    public OutputPin IRQAB { get; }

    public OutputPin IRQBB { get; }

    public InOutPin PA { get; }

    public InputPin CA1 { get; }

    public InOutPin CA2 { get; }

    public InOutPin PB { get; }

    public InputPin CB1 { get; }

    public InOutPin CB2 { get; }

    public override bool IsSequential => true;

    public override void Initialize()
    {
        CreateEngine();
    }

    public override void ReadInputs()
    {
        var pins = _engine.Pins;

        pins.PHI2 = Level(PHI2);
        pins.RESB = Level(RESB);
        pins.CS0 = Level(CS0);
        pins.CS1 = Level(CS1);
        pins.CS2B = Level(CS2B);
        pins.RS0 = Level(RS0);
        pins.RS1 = Level(RS1);
        pins.RWB = Level(RWB);
        pins.CA1 = Level(CA1);
        pins.CB1 = Level(CB1);

        pins.DataIn = (byte)NetLevels(D, 0, 0);
        pins.PAIn = (byte)NetLevels(PA, pins.PAOut, pins.PADrive);
        pins.PBIn = (byte)NetLevels(PB, pins.PBOut, pins.PBDrive);
        pins.CA2In = NetLevels(CA2, pins.CA2Out ? 1 : 0, pins.CA2Drive ? 1 : 0) != 0;
        pins.CB2In = NetLevels(CB2, pins.CB2Out ? 1 : 0, pins.CB2Drive ? 1 : 0) != 0;
    }

    public override void WriteOutputs()
    {
        _engine.Evaluate();

        var pins = _engine.Pins;
        DriveBus(D, pins.DataOut, pins.DataDrive);
        DriveBus(PA, pins.PAOut, pins.PADrive);
        DriveBus(PB, pins.PBOut, pins.PBDrive);
        DriveBus(CA2, pins.CA2Out ? 1 : 0, pins.CA2Drive ? 1 : 0);
        DriveBus(CB2, pins.CB2Out ? 1 : 0, pins.CB2Drive ? 1 : 0);
        DriveOpenDrain(IRQAB, pins.IRQABDrive);
        DriveOpenDrain(IRQBB, pins.IRQBBDrive);
    }

    private void CreateEngine()
    {
        _engine = new W65C21Engine();
        _engine.RegisterAccessed += TraceAccess;
    }

    private void TraceAccess(RegisterAccess access)
    {
        if (!_logger.IsEnabled(LogLevel.Debug))
            return;

        _logger.LogDebug("{Pia} {Direction} {Register} ${Value:X2}", Label ?? PartName,
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

    private static void DriveOpenDrain(OutputPin pin, bool asserted)
    {
        if (asserted)
            pin.DriveValue(0);
        else
            pin.SetToHighZ();
    }

    // a floating (unconnected or high-Z) control input reads as high, its inactive level
    private static bool Level(InputPin pin)
    {
        pin.UpdateFromDrivers();

        if (pin.ConnectedOutputs.Count == 0 || pin.IsHighZ)
            return true;

        return pin.GetBool();
    }

    /// <summary>
    /// The level of each bit of the net on <paramref name="pin"/>: this pin's own value on the bits
    /// it drives, otherwise the other drivers on the net (normal strength over weak), and 1 where
    /// nothing drives. Resolved here because an <see cref="InOutPin"/> that drives some bits does not
    /// receive the others.
    /// </summary>
    private static long NetLevels(InOutPin pin, long ownValue, long ownDrive)
    {
        long normalValue = 0, normalDriven = 0, weakValue = 0, weakDriven = 0;

        foreach (var driver in pin.ConnectedOutputs)
        {
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
                     | ~(normalDriven | weakDriven);
        var mask = (1L << pin.Bits) - 1;
        return ((ownValue & ownDrive) | (others & ~ownDrive)) & mask;
    }
}
