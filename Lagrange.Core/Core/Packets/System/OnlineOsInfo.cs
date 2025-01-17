using ProtoBuf;

// ReSharper disable InconsistentNaming
#pragma warning disable CS8618

namespace Lagrange.Core.Core.Packets.System;

[ProtoContract]
internal class OnlineOsInfo
{
	[ProtoMember(1)] public string User { get; set; }
	
	[ProtoMember(2)] public string Os { get; set; }
	
	[ProtoMember(3)] public string OsVer { get; set; }
	
	[ProtoMember(4)] public string? Field4 { get; set; }
	
	[ProtoMember(5)] public string OsLower { get; set; }
}