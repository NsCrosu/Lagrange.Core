﻿using Lagrange.Core.Common;
using Lagrange.Core.Core.Event.Protocol;
using Lagrange.Core.Core.Event.Protocol.Login;
using Lagrange.Core.Core.Packets;
using Lagrange.Core.Core.Packets.Login.NTLogin;
using Lagrange.Core.Core.Packets.Login.NTLogin.Plain;
using Lagrange.Core.Core.Packets.Login.NTLogin.Plain.Body;
using Lagrange.Core.Core.Packets.Login.NTLogin.Plain.Universal;
using Lagrange.Core.Core.Service.Abstraction;
using Lagrange.Core.Utility.Binary;
using Lagrange.Core.Utility.Crypto;
using Lagrange.Core.Utility.Extension;
using Lagrange.Core.Utility.Generator;
using ProtoBuf;
using BitConverter = Lagrange.Core.Utility.Binary.BitConverter;

namespace Lagrange.Core.Core.Service.Login;

[EventSubscribe(typeof(PasswordLoginEvent))]
[Service("trpc.login.ecdh.EcdhService.SsoNTLoginPasswordLogin")]
internal class PasswordLoginService : BaseService<PasswordLoginEvent>
{
    protected override bool Build(PasswordLoginEvent input, BotKeystore keystore, BotAppInfo appInfo, BotDeviceInfo device,
        out BinaryPacket output, out List<BinaryPacket>? extraPackets)
    {
        var plainBody = new SsoNTLoginPasswordLogin
        {
            Random = (uint)Random.Shared.Next(),
            AppId = (uint)appInfo.AppId,
            Uin = keystore.Uin,
            Timestamp = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            PasswordMd5 = keystore.PasswordMd5.UnHex(),
            RandomBytes = ByteGen.GenRandomBytes(16),
            Guid = device.Guid.ToByteArray(),
            UinString = keystore.Uin.ToString()
        };
        var plainBytes = BinarySerializer.Serialize(plainBody);
        var plainKey = keystore.PasswordMd5.UnHex().Concat(new byte[4]).Concat(BitConverter.GetBytes(keystore.Uin, false)).ToArray().Md5().UnHex();
        var encryptedPlain = keystore.TeaImpl.Encrypt(plainBytes.ToArray(), plainKey); // TODO: Investigate key
        
        output = SsoNTLoginCommon.BuildNTLoginPacket(keystore, appInfo, device, encryptedPlain);
        extraPackets = null;
        return true;
    }

    protected override bool Parse(SsoPacket input, BotKeystore keystore, BotAppInfo appInfo, BotDeviceInfo device,
        out PasswordLoginEvent output, out List<ProtocolEvent>? extraEvents)
    {
        if (keystore.Session.ExchangeKey == null) throw new InvalidOperationException("ExchangeKey is null");

        var payload = input.Payload.ReadBytes(BinaryPacket.Prefix.Uint32 | BinaryPacket.Prefix.WithPrefix);
        var encrypted = Serializer.Deserialize<SsoNTLoginEncryptedData>(payload.AsSpan());
        
        if (encrypted.GcmCalc != null)
        {
            var decrypted = new AesGcmImpl().Decrypt(encrypted.GcmCalc, keystore.Session.ExchangeKey);
            var response = Serializer.Deserialize<SsoNTLoginBase<SsoNTLoginResponse>>(decrypted.AsSpan());
            var body = response.Body;
            
            if (response.Header?.Error != null || body?.Credentials == null)
            {
                keystore.Session.UnusualSign = body?.Unusual?.Sig;
                keystore.Session.UnusualCookies = response.Header?.Cookie?.Cookie;
                keystore.Session.CaptchaUrl = body?.Captcha?.Url;
                
                string? tag = response.Header?.Error?.Tag;
                string? message = response.Header?.Error?.Message;
                output = PasswordLoginEvent.Result((int)(response.Header?.Error?.ErrorCode ?? 1), tag, message);
            }
            else
            {
                keystore.Session.Tgt = body.Credentials.Tgt;
                keystore.Session.D2 = body.Credentials.D2;
                keystore.Session.D2Key = body.Credentials.D2Key;
                keystore.Session.TempPassword = body.Credentials.TempPassword;

                output = PasswordLoginEvent.Result(0);
            }
        }
        else
        {
            output = PasswordLoginEvent.Result(1);
        }

        extraEvents = null;
        return true;
    }
}