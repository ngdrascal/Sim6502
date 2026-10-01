using Digisim.Components;
using Digisim.Engine;
using Digisim.Logging;
using Digisim.Shared;
using Microsoft.Extensions.Logging;
using W65C02S.Engine;
using Context = W65C02S.Engine.Context;
using EngineConstants = W65C02S.Engine.Constants;
using EnginePins = W65C02S.Engine.Pins;
using UInt8 = W65C02S.Engine.Types.UInt8;

namespace W65C02S.DigisimPlugin;

/// <summary>
/// WDC W65C02S microprocessor as a Digisim component, driven by <see cref="W65C02SEngine"/>.
/// </summary>
/// <remarks>
/// One external PHI2 period is one CPU cycle. The engine advances one substep per toggle of its own
/// private clock, six substeps per cycle, so each external edge runs several engine substeps:
/// PHI2 falling runs 6, 1, 2, 3 (latch the read data, then drive the next address and RWB) and PHI2
/// rising runs 4, 5 (drive the write data).
/// D is driven only while RWB is low (write) and PHI2 is high, as on the real part; that keeps the
/// CPU and a memory that is still decoding the previous read cycle from driving D at the same time.
/// BE low floats A, D and RWB. A control input that is not connected (high-Z) reads as high
/// (inactive). A falling SOB sets the V flag.
/// </remarks>
public class W65C02SCpu : LogicNode, IRegisterComponent
{
    public const string TypeUid = "W65C02S";

    private new const string PartName = "W65C02S";

    private const int AddressBits = 16;

    private const int DataBits = 8;

    private const byte Low = EngineConstants.Low;

    private const byte High = EngineConstants.High;

    // the engine substep that runs next once an external edge has been handled
    private const int NextSubstepAfterFall = EngineConstants.P2FirstStep;

    private const int NextSubstepAfterRise = EngineConstants.P2LastSubstep;

    // the first falling edge also runs the engine's warm-up resync, so allow two cycles' worth
    private const int MaxSubstepsPerEdge = 2 * EngineConstants.P2LastSubstep;

    private readonly ILogger _logger;

    private EnginePins _enginePins = null!;
    private Context _ctx = null!;
    private W65C02SEngine _engine = null!;
    private byte _enginePhi2;

    private bool _phi2;
    private bool _lastPhi2;
    private byte _resb = High, _irqb = High, _nmib = High, _rdy = High, _be = High, _sob = High;
    private long _data;

    public static ComponentDescriptor BuildDescriptor()
    {
        return new ComponentDescriptor
            {
                TypeId = TypeUid,
                PartName = PartName,
                Description = "WDC W65C02S 8-bit microprocessor",
                Category = "Processors",
                ClassFullName = typeof(W65C02SCpu).FullName!,
                Presentation = new ComponentPresentation { Shape = "simple", Height = 9, Width = 4 }
            }
            .AddPin(PinTypes.ClockIn, "PHI2", 1, PinLocations.Left, 1)
            .AddPin(PinTypes.DataIn, "RESB", 2, PinLocations.Left, 2)
            .AddPin(PinTypes.DataIn, "IRQB", 3, PinLocations.Left, 3)
            .AddPin(PinTypes.DataIn, "NMIB", 4, PinLocations.Left, 4)
            .AddPin(PinTypes.DataIn, "RDY", 5, PinLocations.Left, 5)
            .AddPin(PinTypes.DataIn, "BE", 6, PinLocations.Left, 6)
            .AddPin(PinTypes.DataIn, "SOB", 7, PinLocations.Left, 7)
            .AddPin(PinTypes.DataOut, "A", 8, PinLocations.Right, 1, AddressBits)
            .AddPin(PinTypes.DataBi, "D", 9, PinLocations.Right, 2, DataBits)
            .AddPin(PinTypes.DataOut, "RWB", 10, PinLocations.Right, 3)
            .AddPin(PinTypes.DataOut, "SYNC", 11, PinLocations.Right, 4)
            .AddPin(PinTypes.DataOut, "VPB", 12, PinLocations.Right, 5)
            .AddPin(PinTypes.DataOut, "MLB", 13, PinLocations.Right, 6)
            .AddPin(PinTypes.DataOut, "PHI1O", 14, PinLocations.Right, 7)
            .AddPin(PinTypes.DataOut, "PHI2O", 15, PinLocations.Right, 8)
            .AddStringProperty(ComponentDescriptor.LabelProperty);
    }

    public static W65C02SCpu Create(Guid instanceId, PropertyValueDictionary properties)
    {
        return new W65C02SCpu(instanceId, properties.GetString(ComponentDescriptor.LabelProperty));
    }

    public W65C02SCpu(Guid instanceId, string? label)
        : this(instanceId, label, LogManager.GetLogger<W65C02SCpu>())
    {
    }

    internal W65C02SCpu(Guid instanceId, string? label, ILogger logger)
        : base(instanceId, PartName, label)
    {
        _logger = logger;

        PHI2 = AddInputPin("PHI2", 1, 1);
        RESB = AddInputPin("RESB", 2, 1);
        IRQB = AddInputPin("IRQB", 3, 1);
        NMIB = AddInputPin("NMIB", 4, 1);
        RDY = AddInputPin("RDY", 5, 1);
        BE = AddInputPin("BE", 6, 1);
        SOB = AddInputPin("SOB", 7, 1);
        A = AddOutputPin("A", 8, AddressBits);
        D = AddInOutPin("D", 9, DataBits);
        RWB = AddOutputPin("RWB", 10, 1);
        SYNC = AddOutputPin("SYNC", 11, 1, supportsHighZ: false);
        VPB = AddOutputPin("VPB", 12, 1, supportsHighZ: false);
        MLB = AddOutputPin("MLB", 13, 1, supportsHighZ: false);
        PHI1O = AddOutputPin("PHI1O", 14, 1, supportsHighZ: false);
        PHI2O = AddOutputPin("PHI2O", 15, 1, supportsHighZ: false);

        D.Mode = PinModes.Input;
        CreateEngine();
    }

    public InputPin PHI2 { get; }

    public InputPin RESB { get; }

    public InputPin IRQB { get; }

    public InputPin NMIB { get; }

    public InputPin RDY { get; }

    public InputPin BE { get; }

    public InputPin SOB { get; }

    public OutputPin A { get; }

    public InOutPin D { get; }

    public OutputPin RWB { get; }

    public OutputPin SYNC { get; }

    public OutputPin VPB { get; }

    public OutputPin MLB { get; }

    public OutputPin PHI1O { get; }

    public OutputPin PHI2O { get; }

    public override bool IsSequential => true;

    public override void Initialize()
    {
        CreateEngine();
    }

    public override void ReadInputs()
    {
        PHI2.UpdateFromDrivers();
        RESB.UpdateFromDrivers();
        IRQB.UpdateFromDrivers();
        NMIB.UpdateFromDrivers();
        RDY.UpdateFromDrivers();
        BE.UpdateFromDrivers();
        SOB.UpdateFromDrivers();

        _phi2 = PHI2.GetBool();
        _resb = Level(RESB);
        _irqb = Level(IRQB);
        _nmib = Level(NMIB);
        _rdy = Level(RDY);
        _be = Level(BE);
        _sob = Level(SOB);

        if (D.Mode == PinModes.Input)
        {
            D.UpdateFromDrivers();
            _data = D.Value;
        }
    }

    public override void WriteOutputs()
    {
        if (_phi2 != _lastPhi2)
        {
            ApplyInputs(latchData: !_phi2);
            RunEngineUntil(_phi2 ? NextSubstepAfterRise : NextSubstepAfterFall);
            _lastPhi2 = _phi2;
        }

        DriveOutputs();
    }

    private void CreateEngine()
    {
        _enginePins = new EnginePins
        {
            RWB = High,
            RESB = High,
            IRQB = High,
            NMIB = High,
            RDY = High,
            BE = High,
            SOB = High
        };
        _ctx = new Context(_enginePins, new Registers(new StatusRegister()));
        _engine = new W65C02SEngine(_ctx);

        // the engine ignores its first clock transition unless it moves away from high
        _enginePhi2 = High;
        _lastPhi2 = false;

        if (_logger.IsEnabled(LogLevel.Debug))
            _engine.OnInstructionComplete += TraceInstruction;
    }

    private void ApplyInputs(bool latchData)
    {
        _enginePins.RESB = _resb;
        _enginePins.IRQB = _irqb;
        _enginePins.NMIB = _nmib;
        _enginePins.RDY = _rdy;
        _enginePins.BE = _be;
        _enginePins.SOB = _sob;

        if (latchData && D.Mode == PinModes.Input)
            _enginePins.DataBus = new UInt8((int)(_data & 0xFF));
    }

    private void RunEngineUntil(int nextSubstep)
    {
        for (var i = 0; i < MaxSubstepsPerEdge && _ctx.Substep != nextSubstep; i++)
        {
            _enginePhi2 = _enginePhi2 == High ? Low : High;
            _enginePins.PHI2 = _enginePhi2;
            _engine.Step();
        }
    }

    private void DriveOutputs()
    {
        var busEnabled = _be == High;

        if (busEnabled && _enginePins.AddrBusMode == AddrBusMode.Output)
            A.DriveValue(_enginePins.AddrBus.ToInt());
        else
            A.SetToHighZ();

        if (busEnabled)
            RWB.DriveValue(_enginePins.RWB);
        else
            RWB.SetToHighZ();

        if (busEnabled && _phi2 && _enginePins.RWB == EngineConstants.Write)
            D.DriveValue(_enginePins.DataBus.ToInt());
        else
            D.Mode = PinModes.Input;

        SYNC.DriveValue(_enginePins.SYNC);
        VPB.DriveValue(_enginePins.VPB);
        MLB.DriveValue(_enginePins.MLB);
        PHI1O.DriveValue(_phi2 ? Low : High);
        PHI2O.DriveValue(_phi2 ? High : Low);
    }

    private void TraceInstruction(object? sender, InstructionCompletedArgs args)
    {
        _logger.LogDebug("{Cpu} {Instruction,-24} {Registers}", Label ?? PartName,
            Disassembler.Disassemble(args.Address, args.OpCode, args.Operand1, args.Operand2), args.Registers);
    }

    // a floating (unconnected or high-Z) control input reads as high, its inactive level
    private static byte Level(InputPin pin)
    {
        if (pin.ConnectedOutputs.Count == 0 || pin.IsHighZ)
            return High;

        return pin.GetBool() ? High : Low;
    }
}
