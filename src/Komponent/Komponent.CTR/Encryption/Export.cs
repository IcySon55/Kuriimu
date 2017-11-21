using System;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel.Composition;
using Komponent.CTR.Encryption.AES;
using Komponent.Interface;

namespace Komponent.CTR.Encryption
{
    public class Export
    {
        [ExportMetadata("Name", "Cardridge Encryption")]
        [ExportMetadata("TabPathEncrypt", "")]
        [ExportMetadata("TabPathDecrypt", "3DS/.3ds")]
        [Export("CTR_3DS", typeof(IEncryption))]
        [Export(typeof(IEncryption))]
        public class CTR3DS : IEncryption
        {
            public byte[] Decrypt(Stream input)
            {
                var engine = new AesEngine();

                var ms = new MemoryStream();
                input.CopyTo(ms);
                ms.Position = 0;
                input.Position = 0;

                engine.DecryptGameNCSD(input, ms);

                return ms.ToArray();
            }

            public byte[] Encrypt(Stream input)
            {
                return null;
            }
        }

        [ExportMetadata("Name", "CIA Encryption")]
        [ExportMetadata("TabPathEncrypt", "")]
        [ExportMetadata("TabPathDecrypt", "3DS/.cia")]
        [Export("CTR_CIA", typeof(IEncryption))]
        [Export(typeof(IEncryption))]
        public class CTRCIA : IEncryption
        {
            public byte[] Decrypt(Stream input)
            {
                var engine = new AesEngine();

                var ms = new MemoryStream();
                input.CopyTo(ms);
                input.Position = 0;
                ms.Position = 0;

                engine.DecryptCIA(input, ms);

                return ms.ToArray();
            }

            public byte[] Encrypt(Stream input)
            {
                return null;
            }
        }
    }
}
