using System.Runtime.InteropServices;
using System.Text;

namespace kittywork.Wc3ReplayParser.Business;

// Raw struct representations for replay actions. These match the binary layout
// used in the replay file. StructLayout is sequential with packing of 1 to
// ensure there is no padding between fields.

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SetGameSpeedData
{
    public byte GameSpeed;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct UnitAbilityNoTargetData
{
    public ushort AbilityFlags;
    public uint OrderId;
    public ulong Unknown;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct UnitAbilityTargetPositionData
{
    public ushort AbilityFlags;
    public uint OrderId;
    public ulong Unknown;
    public float X;
    public float Y;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct UnitAbilityTargetPositionObjectData
{
    public ushort AbilityFlags;
    public uint OrderId;
    public ulong Unknown;
    public float X;
    public float Y;
    public NetTagData Object;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct GiveItemToUnitData
{
    public ushort AbilityFlags;
    public uint OrderId;
    public ulong Unknown;
    public float X;
    public float Y;
    public NetTagData Unit;
    public NetTagData Item;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct NetTagData
{
    public uint A;
    public uint B;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct ChangeSelectionDataPrefix
{
    public byte SelectMode;
    public byte Count;
    // followed by Count NetTagData entries
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct AssignGroupHotkeyDataPrefix
{
    public byte GroupNumber;
    public byte Count;
    // followed by Count NetTagData entries
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SelectGroupHotkeyData
{
    public byte GroupNumber;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SelectSubgroupData
{
    public uint ItemId;
    public NetTagData Object;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SelectUnitData
{
    public NetTagData Object;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SelectGroundItemData
{
    public NetTagData Item;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct CancelHeroRevivalData
{
    public NetTagData Hero;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct RemoveUnitFromQueueData
{
    public byte SlotNumber;
    public uint ItemId;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct TransferResourcesData
{
    public byte Slot;
    public uint Gold;
    public uint Lumber;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct ArrowKeyData
{
    public byte ArrowKey;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct MouseActionData
{
    public byte EventId;
    public float X;
    public float Y;
    public byte Button;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct W3ApiDataPrefix
{
    public uint CommandId;
    public uint Data;
    public uint Length;
    // followed by Length bytes of buffer
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct BlzSyncDataPrefix
{
    // zero terminated strings follow, then a uint which is ignored
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct CommandFrameDataPrefix
{
    public ulong Unknown;
    public uint EventId;
    public float Val;
    // followed by zero terminated UTF8 string
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct ChatMessageDataPrefix
{
    public uint UnknownA;
    public uint UnknownB;
    // followed by zero terminated UTF8 string
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct MinimapPingData
{
    public uint X;
    public uint Y;
    public uint Flags;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct MmdMessageDataPrefix
{
    // zero terminated strings Tag, Value, Text then uint Data
}

public static partial class ReplayActionParserExtensions
{
    private static bool TryReadStruct<T>(ref ReadOnlySpan<byte> span, out T value) where T : struct
    {
        if (span.Length < Marshal.SizeOf<T>())
        {
            value = default;
            return false;
        }
        value = MemoryMarshal.Read<T>(span);
        span = span.Slice(Marshal.SizeOf<T>());
        return true;
    }

    public static bool TryParseAction(ref ReadOnlySpan<byte> span, out ReplayAction action)
    {
        if (span.IsEmpty)
        {
            action = null!;
            return false;
        }

        return span[0] switch
        {
            0x03 => span.TryParseSetGameSpeed(out action),
            0x10 => span.TryParseUnitAbilityNoTarget(out action),
            0x11 => span.TryParseUnitAbilityTargetPosition(out action),
            0x12 => span.TryParseUnitAbilityTargetPositionObject(out action),
            0x13 => span.TryParseGiveItemToUnit(out action),
            0x16 => span.TryParseChangeSelection(out action),
            0x17 => span.TryParseAssignGroupHotkey(out action),
            0x18 => span.TryParseSelectGroupHotkey(out action),
            0x19 => span.TryParseSelectSubgroup(out action),
            0x1B => span.TryParseSelectUnit(out action),
            0x1C => span.TryParseSelectGroundItem(out action),
            0x1D => span.TryParseCancelHeroRevival(out action),
            0x1E or 0x1F => span.TryParseRemoveUnitFromQueue(out action),
            0x51 => span.TryParseTransferResources(out action),
            0x60 => span.TryParseChatMessage(out action),
            0x62 => span.TryParseMinimapPing(out action),
            0x75 => span.TryParseArrowKey(out action),
            0x76 => span.TryParseMouseAction(out action),
            0x77 => span.TryParseW3Api(out action),
            0x78 => span.TryParseBlzSync(out action),
            0x79 => span.TryParseCommandFrame(out action),
            0x6B => span.TryParseMmdMessage(out action),
            _ => span.TryParseUnknown(out action)
        };
    }

    public static bool TryParseSetGameSpeed(this ref ReadOnlySpan<byte> span, out ReplayAction action)
    {
        action = null!;
        if (span.Length < 1 + Marshal.SizeOf<SetGameSpeedData>() || span[0] != 0x03)
            return false;
        var work = span.Slice(1);
        if (!TryReadStruct(ref work, out SetGameSpeedData data))
            return false;
        span = work;
        action = new SetGameSpeedAction(data.GameSpeed);
        return true;
    }

    public static bool TryParseUnitAbilityNoTarget(this ref ReadOnlySpan<byte> span, out ReplayAction action)
    {
        action = null!;
        if (span.Length < 1 + Marshal.SizeOf<UnitAbilityNoTargetData>() || span[0] != 0x10)
            return false;
        var work = span.Slice(1);
        if (!TryReadStruct(ref work, out UnitAbilityNoTargetData data))
            return false;
        span = work;
        action = new UnitAbilityNoTargetAction(data.AbilityFlags, data.OrderId);
        return true;
    }

    public static bool TryParseUnitAbilityTargetPosition(this ref ReadOnlySpan<byte> span, out ReplayAction action)
    {
        action = null!;
        if (span.Length < 1 + Marshal.SizeOf<UnitAbilityTargetPositionData>() || span[0] != 0x11)
            return false;
        var work = span.Slice(1);
        if (!TryReadStruct(ref work, out UnitAbilityTargetPositionData data))
            return false;
        span = work;
        action = new UnitAbilityTargetPositionAction(data.AbilityFlags, data.OrderId, data.X, data.Y);
        return true;
    }

    public static bool TryParseUnitAbilityTargetPositionObject(this ref ReadOnlySpan<byte> span, out ReplayAction action)
    {
        action = null!;
        if (span.Length < 1 + Marshal.SizeOf<UnitAbilityTargetPositionObjectData>() || span[0] != 0x12)
            return false;
        var work = span.Slice(1);
        if (!TryReadStruct(ref work, out UnitAbilityTargetPositionObjectData data))
            return false;
        span = work;
        var obj = new NetTag(data.Object.A, data.Object.B);
        action = new UnitAbilityTargetPositionObjectAction(data.AbilityFlags, data.OrderId, data.X, data.Y, obj);
        return true;
    }

    public static bool TryParseGiveItemToUnit(this ref ReadOnlySpan<byte> span, out ReplayAction action)
    {
        action = null!;
        if (span.Length < 1 + Marshal.SizeOf<GiveItemToUnitData>() || span[0] != 0x13)
            return false;
        var work = span.Slice(1);
        if (!TryReadStruct(ref work, out GiveItemToUnitData data))
            return false;
        span = work;
        action = new GiveItemToUnitAction(data.AbilityFlags, data.OrderId, data.X, data.Y,
            new NetTag(data.Unit.A, data.Unit.B), new NetTag(data.Item.A, data.Item.B));
        return true;
    }

    public static bool TryParseChangeSelection(this ref ReadOnlySpan<byte> span, out ReplayAction action)
    {
        action = null!;
        if (span.Length < 1 + Marshal.SizeOf<ChangeSelectionDataPrefix>() || span[0] != 0x16)
            return false;
        var work = span.Slice(1);
        if (!TryReadStruct(ref work, out ChangeSelectionDataPrefix prefix))
            return false;
        int total = Marshal.SizeOf<ChangeSelectionDataPrefix>() + prefix.Count * Marshal.SizeOf<NetTagData>();
        if (span.Length < 1 + total)
            return false;
        var unitsWork = work;
        var units = new NetTag[prefix.Count];
        for (int i = 0; i < units.Length; i++)
        {
            var nt = MemoryMarshal.Read<NetTagData>(unitsWork);
            unitsWork = unitsWork.Slice(Marshal.SizeOf<NetTagData>());
            units[i] = new NetTag(nt.A, nt.B);
        }
        work = work.Slice(prefix.Count * Marshal.SizeOf<NetTagData>());
        span = work;
        action = new ChangeSelectionAction(prefix.SelectMode, units);
        return true;
    }

    public static bool TryParseAssignGroupHotkey(this ref ReadOnlySpan<byte> span, out ReplayAction action)
    {
        action = null!;
        if (span.Length < 1 + Marshal.SizeOf<AssignGroupHotkeyDataPrefix>() || span[0] != 0x17)
            return false;
        var work = span.Slice(1);
        if (!TryReadStruct(ref work, out AssignGroupHotkeyDataPrefix prefix))
            return false;
        int total = Marshal.SizeOf<AssignGroupHotkeyDataPrefix>() + prefix.Count * Marshal.SizeOf<NetTagData>();
        if (span.Length < 1 + total)
            return false;
        var unitsWork = work;
        var units = new NetTag[prefix.Count];
        for (int i = 0; i < units.Length; i++)
        {
            var nt = MemoryMarshal.Read<NetTagData>(unitsWork);
            unitsWork = unitsWork.Slice(Marshal.SizeOf<NetTagData>());
            units[i] = new NetTag(nt.A, nt.B);
        }
        work = work.Slice(prefix.Count * Marshal.SizeOf<NetTagData>());
        span = work;
        action = new AssignGroupHotkeyAction(prefix.GroupNumber, units);
        return true;
    }

    public static bool TryParseSelectGroupHotkey(this ref ReadOnlySpan<byte> span, out ReplayAction action)
    {
        action = null!;
        if (span.Length < 1 + Marshal.SizeOf<SelectGroupHotkeyData>() || span[0] != 0x18)
            return false;
        var work = span.Slice(1);
        if (!TryReadStruct(ref work, out SelectGroupHotkeyData data))
            return false;
        span = work;
        action = new SelectGroupHotkeyAction(data.GroupNumber);
        return true;
    }

    public static bool TryParseSelectSubgroup(this ref ReadOnlySpan<byte> span, out ReplayAction action)
    {
        action = null!;
        if (span.Length < 1 + Marshal.SizeOf<SelectSubgroupData>() || span[0] != 0x19)
            return false;
        var work = span.Slice(1);
        if (!TryReadStruct(ref work, out SelectSubgroupData data))
            return false;
        span = work;
        action = new SelectSubgroupAction(data.ItemId, new NetTag(data.Object.A, data.Object.B));
        return true;
    }

    public static bool TryParseSelectUnit(this ref ReadOnlySpan<byte> span, out ReplayAction action)
    {
        action = null!;
        if (span.Length < 1 + Marshal.SizeOf<SelectUnitData>() || span[0] != 0x1B)
            return false;
        var work = span.Slice(1);
        if (!TryReadStruct(ref work, out SelectUnitData data))
            return false;
        span = work;
        action = new SelectUnitAction(new NetTag(data.Object.A, data.Object.B));
        return true;
    }

    public static bool TryParseSelectGroundItem(this ref ReadOnlySpan<byte> span, out ReplayAction action)
    {
        action = null!;
        if (span.Length < 1 + Marshal.SizeOf<SelectGroundItemData>() || span[0] != 0x1C)
            return false;
        var work = span.Slice(1);
        if (!TryReadStruct(ref work, out SelectGroundItemData data))
            return false;
        span = work;
        action = new SelectGroundItemAction(new NetTag(data.Item.A, data.Item.B));
        return true;
    }

    public static bool TryParseCancelHeroRevival(this ref ReadOnlySpan<byte> span, out ReplayAction action)
    {
        action = null!;
        if (span.Length < 1 + Marshal.SizeOf<CancelHeroRevivalData>() || span[0] != 0x1D)
            return false;
        var work = span.Slice(1);
        if (!TryReadStruct(ref work, out CancelHeroRevivalData data))
            return false;
        span = work;
        action = new CancelHeroRevivalAction(new NetTag(data.Hero.A, data.Hero.B));
        return true;
    }

    public static bool TryParseRemoveUnitFromQueue(this ref ReadOnlySpan<byte> span, out ReplayAction action)
    {
        action = null!;
        if (span.Length < 1 + Marshal.SizeOf<RemoveUnitFromQueueData>())
            return false;
        byte id = span[0];
        if (id != 0x1E && id != 0x1F)
            return false;
        var work = span.Slice(1);
        if (!TryReadStruct(ref work, out RemoveUnitFromQueueData data))
            return false;
        span = work;
        action = new RemoveUnitFromQueueAction(id, data.SlotNumber, data.ItemId);
        return true;
    }

    public static bool TryParseTransferResources(this ref ReadOnlySpan<byte> span, out ReplayAction action)
    {
        action = null!;
        if (span.Length < 1 + Marshal.SizeOf<TransferResourcesData>() || span[0] != 0x51)
            return false;
        var work = span.Slice(1);
        if (!TryReadStruct(ref work, out TransferResourcesData data))
            return false;
        span = work;
        action = new TransferResourcesAction(data.Slot, data.Gold, data.Lumber);
        return true;
    }

    public static bool TryParseChatMessage(this ref ReadOnlySpan<byte> span, out ReplayAction action)
    {
        action = null!;
        if (span.Length < 1 + Marshal.SizeOf<ChatMessageDataPrefix>() || span[0] != 0x60)
            return false;
        var work = span.Slice(1);
        if (!TryReadStruct(ref work, out ChatMessageDataPrefix data))
            return false;
        var msgSpan = work;
        int zeroIdx = msgSpan.IndexOf((byte)0);
        if (zeroIdx < 0)
            return false;
        string msg = Encoding.UTF8.GetString(msgSpan.Slice(0, zeroIdx));
        work = msgSpan.Slice(zeroIdx + 1);
        span = work;
        action = new ChatMessageAction(data.UnknownA, data.UnknownB, msg);
        return true;
    }

    public static bool TryParseMinimapPing(this ref ReadOnlySpan<byte> span, out ReplayAction action)
    {
        action = null!;
        if (span.Length < 1 + Marshal.SizeOf<MinimapPingData>() || span[0] != 0x62)
            return false;
        var work = span.Slice(1);
        if (!TryReadStruct(ref work, out MinimapPingData data))
            return false;
        span = work;
        action = new MinimapPingAction(data.X, data.Y, data.Flags);
        return true;
    }

    public static bool TryParseArrowKey(this ref ReadOnlySpan<byte> span, out ReplayAction action)
    {
        action = null!;
        if (span.Length < 1 + Marshal.SizeOf<ArrowKeyData>() || span[0] != 0x75)
            return false;
        var work = span.Slice(1);
        if (!TryReadStruct(ref work, out ArrowKeyData data))
            return false;
        span = work;
        action = new ArrowKeyAction(data.ArrowKey);
        return true;
    }

    public static bool TryParseMouseAction(this ref ReadOnlySpan<byte> span, out ReplayAction action)
    {
        action = null!;
        if (span.Length < 1 + Marshal.SizeOf<MouseActionData>() || span[0] != 0x76)
            return false;
        var work = span.Slice(1);
        if (!TryReadStruct(ref work, out MouseActionData data))
            return false;
        span = work;
        action = new MouseAction(data.EventId, data.X, data.Y, data.Button);
        return true;
    }

    public static bool TryParseW3Api(this ref ReadOnlySpan<byte> span, out ReplayAction action)
    {
        action = null!;
        if (span.Length < 1 + Marshal.SizeOf<W3ApiDataPrefix>() || span[0] != 0x77)
            return false;
        var work = span.Slice(1);
        if (!TryReadStruct(ref work, out W3ApiDataPrefix prefix))
            return false;
        if (work.Length < prefix.Length)
            return false;
        var text = Encoding.UTF8.GetString(work.Slice(0, (int)prefix.Length));
        work = work.Slice((int)prefix.Length);
        span = work;
        action = new W3ApiAction(prefix.CommandId, prefix.Data, text);
        return true;
    }

    public static bool TryParseBlzSync(this ref ReadOnlySpan<byte> span, out ReplayAction action)
    {
        action = null!;
        if (span.Length < 1 || span[0] != 0x78)
            return false;
        var work = span.Slice(1);
        int idEnd = work.IndexOf((byte)0);
        if (idEnd < 0)
            return false;
        var idStr = Encoding.UTF8.GetString(work.Slice(0, idEnd));
        work = work.Slice(idEnd + 1);
        int valEnd = work.IndexOf((byte)0);
        if (valEnd < 0)
            return false;
        var valStr = Encoding.UTF8.GetString(work.Slice(0, valEnd));
        work = work.Slice(valEnd + 1);
        if (work.Length < 4)
            return false;
        work = work.Slice(4); // ignore uint
        span = work;
        action = new BlzSyncAction(idStr, valStr);
        return true;
    }

    public static bool TryParseCommandFrame(this ref ReadOnlySpan<byte> span, out ReplayAction action)
    {
        action = null!;
        if (span.Length < 1 + Marshal.SizeOf<CommandFrameDataPrefix>() || span[0] != 0x79)
            return false;
        var work = span.Slice(1);
        if (!TryReadStruct(ref work, out CommandFrameDataPrefix prefix))
            return false;
        int txtEnd = work.IndexOf((byte)0);
        if (txtEnd < 0)
            return false;
        var txt = Encoding.UTF8.GetString(work.Slice(0, txtEnd));
        work = work.Slice(txtEnd + 1);
        span = work;
        action = new CommandFrameAction(prefix.EventId, prefix.Val, txt);
        return true;
    }

    public static bool TryParseMmdMessage(this ref ReadOnlySpan<byte> span, out ReplayAction action)
    {
        action = null!;
        if (span.Length < 1 || span[0] != 0x6B)
            return false;
        var work = span.Slice(1);
        int tagEnd = work.IndexOf((byte)0);
        if (tagEnd < 0)
            return false;
        var tag = Encoding.UTF8.GetString(work.Slice(0, tagEnd));
        work = work.Slice(tagEnd + 1);
        int valEnd = work.IndexOf((byte)0);
        if (valEnd < 0)
            return false;
        var val = Encoding.UTF8.GetString(work.Slice(0, valEnd));
        work = work.Slice(valEnd + 1);
        int textEnd = work.IndexOf((byte)0);
        if (textEnd < 0)
            return false;
        var text = Encoding.UTF8.GetString(work.Slice(0, textEnd));
        work = work.Slice(textEnd + 1);
        if (work.Length < 4)
            return false;
        uint data = MemoryMarshal.Read<uint>(work);
        work = work.Slice(4);
        span = work;
        action = new MmdMessageAction(tag, val, text, data);
        return true;
    }

    public static bool TryParseUnknown(this ref ReadOnlySpan<byte> span, out ReplayAction action)
    {
        action = null!;
        if (span.IsEmpty)
            return false;
        byte id = span[0];
        var unknown = span.Slice(1).ToArray();
        span = ReadOnlySpan<byte>.Empty;
        action = new UnknownAction(id, unknown);
        return true;
    }
}

