using System.Text;

namespace kittywork.Wc3ReplayParser.Business;

public abstract record ReplayAction(byte Id)
{
    public abstract string Explain();
}

public record UnknownAction(byte Id, byte[] Data) : ReplayAction(Id)
{
    public override string Explain() => $"Unknown action 0x{Id:X2} ({BitConverter.ToString(Data)})";
}

public record SetGameSpeedAction(byte GameSpeed) : ReplayAction(0x03)
{
    public override string Explain() => $"Set game speed to {GameSpeed}";
}

public record UnitAbilityNoTargetAction(ushort AbilityFlags, uint OrderId) : ReplayAction(0x10)
{
    public override string Explain() => $"Order {ActionHelpers.FourCC(OrderId)} flags=0x{AbilityFlags:X}";
}

public record UnitAbilityTargetPositionAction(ushort AbilityFlags, uint OrderId, float X, float Y) : ReplayAction(0x11)
{
    public override string Explain() => $"Order {ActionHelpers.FourCC(OrderId)} to ({X},{Y}) flags=0x{AbilityFlags:X}";
}

public record UnitAbilityTargetPositionObjectAction(ushort AbilityFlags, uint OrderId, float X, float Y, NetTag Object) : ReplayAction(0x12)
{
    public override string Explain() => $"Order {ActionHelpers.FourCC(OrderId)} to ({X},{Y}) object={Object}";
}

public record GiveItemToUnitAction(ushort AbilityFlags, uint OrderId, float X, float Y, NetTag Unit, NetTag Item) : ReplayAction(0x13)
{
    public override string Explain() => $"Give item {Item} to {Unit} using {ActionHelpers.FourCC(OrderId)}";
}

public record ChangeSelectionAction(byte SelectMode, NetTag[] Units) : ReplayAction(0x16)
{
    public override string Explain() => $"Change selection mode {SelectMode} units {string.Join(',', Units.Select(u => u.ToString()))}";
}

public record AssignGroupHotkeyAction(byte GroupNumber, NetTag[] Units) : ReplayAction(0x17)
{
    public override string Explain() => $"Assign group {GroupNumber} units {string.Join(',', Units.Select(u => u.ToString()))}";
}

public record SelectGroupHotkeyAction(byte GroupNumber) : ReplayAction(0x18)
{
    public override string Explain() => $"Select group {GroupNumber}";
}

public record SelectSubgroupAction(uint ItemId, NetTag Object) : ReplayAction(0x19)
{
    public override string Explain() => $"Select subgroup item {ActionHelpers.FourCC(ItemId)} of {Object}";
}

public record SelectUnitAction(NetTag Object) : ReplayAction(0x1B)
{
    public override string Explain() => $"Select unit {Object}";
}

public record SelectGroundItemAction(NetTag Item) : ReplayAction(0x1C)
{
    public override string Explain() => $"Select ground item {Item}";
}

public record CancelHeroRevivalAction(NetTag Hero) : ReplayAction(0x1D)
{
    public override string Explain() => $"Cancel hero revival {Hero}";
}

public record RemoveUnitFromQueueAction(byte Id, byte SlotNumber, uint ItemId) : ReplayAction(Id) // Id 0x1E or 0x1F
{
    public override string Explain() => $"Remove unit {ActionHelpers.FourCC(ItemId)} from queue slot {SlotNumber}";
}

public record TransferResourcesAction(byte Slot, uint Gold, uint Lumber) : ReplayAction(0x51)
{
    public override string Explain() => $"Transfer resources to slot {Slot} gold={Gold} lumber={Lumber}";
}

public record ArrowKeyAction(byte ArrowKey) : ReplayAction(0x75)
{
    public override string Explain() => $"Arrow key {ArrowKey}";
}

public record MouseAction(byte EventId, float X, float Y, byte Button) : ReplayAction(0x76)
{
    public override string Explain() => $"Mouse event {EventId} at ({X},{Y}) button {Button}";
}

public record W3ApiAction(uint CommandId, uint Data, string Buffer) : ReplayAction(0x77)
{
    public override string Explain() => $"W3API command {CommandId} data={Data} text={Buffer}";
}

public record BlzSyncAction(string Identifier, string Value) : ReplayAction(0x78)
{
    public override string Explain() => $"BlzSync {Identifier}={Value}";
}

public record CommandFrameAction(uint EventId, float Val, string Text) : ReplayAction(0x79)
{
    public override string Explain() => $"CommandFrame event {EventId} val={Val} text={Text}";
}

public record NetTag(uint A, uint B)
{
    public override string ToString() => $"[{A:X8},{B:X8}]";
}

static class ActionHelpers
{
    public static string FourCC(uint v)
    {
        Span<byte> bytes = stackalloc byte[4];
        BitConverter.TryWriteBytes(bytes, v);
        return Encoding.ASCII.GetString(bytes);
    }
}
