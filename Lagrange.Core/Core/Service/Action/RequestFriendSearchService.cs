using Lagrange.Core.Common;
using Lagrange.Core.Core.Event.Protocol;
using Lagrange.Core.Core.Event.Protocol.Action;
using Lagrange.Core.Core.Packets;
using Lagrange.Core.Core.Packets.Service.Oidb;
using Lagrange.Core.Core.Packets.Service.Oidb.Request;
using Lagrange.Core.Core.Service.Abstraction;
using Lagrange.Core.Utility.Binary;
using Lagrange.Core.Utility.Extension;
using ProtoBuf;

namespace Lagrange.Core.Core.Service.Action;

[EventSubscribe(typeof(RequestFriendSearchEvent))]
[Service("OidbSvcTrpcTcp.0x972_6")]
internal class RequestFriendSearchService : BaseService<RequestFriendSearchEvent>
{
    protected override bool Build(RequestFriendSearchEvent input, BotKeystore keystore, BotAppInfo appInfo, BotDeviceInfo device,
        out BinaryPacket output, out List<BinaryPacket>? extraPackets)
    {
        var packet = new OidbSvcTrpcTcpBase<OidbSvcTrpcTcp0x972_6>(new OidbSvcTrpcTcp0x972_6
        {
            TargetUin = input.TargetUin,
            Settings = new OidbSvcTrpcTcp0x972_6Settings
            {
                Field4 = 25,
                Field11 = "",
                Setting = "{\"search_by_uid\":true, \"scenario\":\"related_people_and_groups_panel\"}"
            }
        });
        
        using var stream = new MemoryStream();
        Serializer.Serialize(stream, packet);
        output = new BinaryPacket(stream);
        
        extraPackets = null;
        return true;
    }
    
    protected override bool Parse(SsoPacket input, BotKeystore keystore, BotAppInfo appInfo, BotDeviceInfo device,
        out RequestFriendSearchEvent output, out List<ProtocolEvent>? extraEvents)
    {
        var payload = input.Payload.ReadBytes(BinaryPacket.Prefix.Uint32 | BinaryPacket.Prefix.WithPrefix);
        
        Console.WriteLine(payload.Hex());
        
        output = RequestFriendSearchEvent.Result(0);
        extraEvents = null;
        return true;
    }
}