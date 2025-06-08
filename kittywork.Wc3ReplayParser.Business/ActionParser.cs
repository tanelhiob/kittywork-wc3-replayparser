using System.Text;

namespace kittywork.Wc3ReplayParser.Business;

internal static class ActionParser
{
    public static ReplayAction Parse(byte id, BinaryReader br)
    {
        switch (id)
        {
            case 0x03:
                return new SetGameSpeedAction(br.ReadByte());
            case 0x10:
                ushort flags = br.ReadUInt16();
                uint order = br.ReadUInt32();
                br.ReadUInt64();
                return new UnitAbilityNoTargetAction(flags, order);
            case 0x11:
                flags = br.ReadUInt16();
                order = br.ReadUInt32();
                br.ReadUInt64();
                float x = br.ReadSingle();
                float y = br.ReadSingle();
                return new UnitAbilityTargetPositionAction(flags, order, x, y);
            case 0x12:
                flags = br.ReadUInt16();
                order = br.ReadUInt32();
                br.ReadUInt64();
                x = br.ReadSingle();
                y = br.ReadSingle();
                var obj = ReadNetTag(br);
                return new UnitAbilityTargetPositionObjectAction(flags, order, x, y, obj);
            case 0x13:
                flags = br.ReadUInt16();
                order = br.ReadUInt32();
                br.ReadUInt64();
                x = br.ReadSingle();
                y = br.ReadSingle();
                var unit = ReadNetTag(br);
                var item = ReadNetTag(br);
                return new GiveItemToUnitAction(flags, order, x, y, unit, item);
            case 0x16:
                byte mode = br.ReadByte();
                byte count = br.ReadByte();
                var units = new NetTag[count];
                for (int i=0;i<count;i++) units[i]=ReadNetTag(br);
                return new ChangeSelectionAction(mode, units);
            case 0x17:
                byte grp = br.ReadByte();
                count = br.ReadByte();
                units = new NetTag[count];
                for(int i=0;i<count;i++) units[i]=ReadNetTag(br);
                return new AssignGroupHotkeyAction(grp, units);
            case 0x18:
                grp = br.ReadByte();
                return new SelectGroupHotkeyAction(grp);
            case 0x19:
                uint itemId = br.ReadUInt32();
                var obj19 = ReadNetTag(br);
                return new SelectSubgroupAction(itemId, obj19);
            case 0x1B:
                return new SelectUnitAction(ReadNetTag(br));
            case 0x1C:
                return new SelectGroundItemAction(ReadNetTag(br));
            case 0x1D:
                return new CancelHeroRevivalAction(ReadNetTag(br));
            case 0x1E:
            case 0x1F:
                byte slot = br.ReadByte();
                itemId = br.ReadUInt32();
                return new RemoveUnitFromQueueAction(id, slot, itemId);
            case 0x51:
                slot = br.ReadByte();
                uint gold = br.ReadUInt32();
                uint lumber = br.ReadUInt32();
                return new TransferResourcesAction(slot, gold, lumber);
            case 0x60:
                uint a = br.ReadUInt32();
                uint b = br.ReadUInt32();
                string msg = ReadString(br);
                return new ChatMessageAction(a, b, msg);
            case 0x62:
                uint x32 = br.ReadUInt32();
                uint y32 = br.ReadUInt32();
                uint pingFlags = br.ReadUInt32();
                return new MinimapPingAction(x32, y32, pingFlags);
            case 0x75:
                byte arrow = br.ReadByte();
                return new ArrowKeyAction(arrow);
            case 0x76:
                byte eventId = br.ReadByte();
                x = br.ReadSingle();
                y = br.ReadSingle();
                byte button = br.ReadByte();
                return new MouseAction(eventId, x, y, button);
            case 0x77:
                uint cmd = br.ReadUInt32();
                uint data = br.ReadUInt32();
                uint len = br.ReadUInt32();
                string buffer = Encoding.UTF8.GetString(br.ReadBytes((int)len));
                return new W3ApiAction(cmd, data, buffer);
            case 0x78:
                string identifier = ReadString(br);
                string value = ReadString(br);
                br.ReadUInt32();
                return new BlzSyncAction(identifier, value);
            case 0x79:
                br.ReadUInt64();
                uint eventId32 = br.ReadUInt32();
                float val = br.ReadSingle();
                string text = ReadString(br);
                return new CommandFrameAction(eventId32, val, text);
            case 0x6B:
                string tag = ReadString(br);
                string value2 = ReadString(br);
                string text2 = ReadString(br);
                uint data2 = br.ReadUInt32();
                return new MmdMessageAction(tag, value2, text2, data2);
            default:
                var remaining = br.ReadBytes((int)(br.BaseStream.Length - br.BaseStream.Position));
                return new UnknownAction(id, remaining);
        }
    }

    private static NetTag ReadNetTag(BinaryReader br) => new(br.ReadUInt32(), br.ReadUInt32());

    private static string ReadString(BinaryReader br)
    {
        List<byte> bytes = new();
        byte b;
        while ((b = br.ReadByte()) != 0)
        {
            bytes.Add(b);
        }
        return Encoding.UTF8.GetString(bytes.ToArray());
    }
}
