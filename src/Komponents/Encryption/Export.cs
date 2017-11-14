using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Kontract;
using Kontract.IO;
using Kontract.Interface;

namespace Encryption
{
    [Export("BlowFishCBC", typeof(IEncryption))]
    public class BlowFishCBC : IEncryption
    {
        public byte[] Decrypt(Stream input)
        {
            var key = InputBox.Show("Input decryption key:", "Decrypt Blowfish_CBC");
            if (key == String.Empty) throw new Exception("Key can't be empty!");

            var bf = new BlowFish(key);
            var ms = new MemoryStream();
            var dec = bf.Decrypt_CBC(new BinaryReaderX(input, true).ReadAllBytes());
            ms.Write(dec, 0, dec.Length);

            return ms.ToArray();
        }

        public byte[] Encrypt(Stream input)
        {
            var key = InputBox.Show("Input encryption key:", "Encrypt Blowfish CBC");

            if (key == String.Empty) throw new Exception("Key can't be empty!");
            var bf = new BlowFish(key);
            var ms = new MemoryStream();
            var enc = bf.Encrypt_CBC(new BinaryReaderX(input, true).ReadAllBytes());
            ms.Write(enc, 0, enc.Length);

            return ms.ToArray();
        }
    }

    [Export("BlowFishECB", typeof(IEncryption))]
    public class BlowFishECB : IEncryption
    {
        public byte[] Decrypt(Stream input)
        {
            var key = InputBox.Show("Input decryption key:", "Decrypt Blowfish_ECB");
            if (key == String.Empty) throw new Exception("Key can't be empty!");

            var bf = new BlowFish(key);
            var ms = new MemoryStream();
            var dec = bf.Decrypt_ECB(new BinaryReaderX(input, true).ReadAllBytes());
            ms.Write(dec, 0, dec.Length);

            return ms.ToArray();
        }

        public byte[] Encrypt(Stream input)
        {
            var key = InputBox.Show("Input encryption key:", "Encrypt Blowfish_ECB");

            var bf = new BlowFish(key);
            var ms = new MemoryStream();
            var enc = bf.Decrypt_ECB(new BinaryReaderX(input, true).ReadAllBytes());
            ms.Write(enc, 0, enc.Length);

            return ms.ToArray();
        }
    }

    [Export("MTFramework", typeof(IEncryption))]
    public class MTFrameworkEncryption : IEncryption
    {
        public byte[] Decrypt(Stream input)
        {
            var key1 = InputBox.Show("Input 1st decryption key:", "Decrypt MTFramework");
            var key2 = InputBox.Show("Input 2nd decryption key:", "Decrypt MTFramework");
            if (key1 == String.Empty || key2 == String.Empty) throw new Exception("Keys can't be empty!");

            var ms = new MemoryStream();
            var dec = MTFramework.Decrypt(input, key1, key2);
            ms.Write(dec, 0, dec.Length);

            return ms.ToArray();
        }

        public byte[] Encrypt(Stream input)
        {
            var key1 = InputBox.Show("Input 1st encryption key:", "Encrypt MTFramework");
            var key2 = InputBox.Show("Input 2nd encryption key:", "Encrypt MTFramework");
            if (key1 == String.Empty || key2 == String.Empty) throw new Exception("Keys can't be empty!");

            var ms = new MemoryStream();
            var enc = MTFramework.Encrypt(input, key1, key2);
            ms.Write(enc, 0, enc.Length);

            return ms.ToArray();
        }
    }

    [Export("CTR_3DS", typeof(IEncryption))]
    public class CTR3DS : IEncryption
    {
        public byte[] Decrypt(Stream input)
        {
            var engine = new CTR.AesEngine();

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

    [Export("CTR_CIA", typeof(IEncryption))]
    public class CTRCIA : IEncryption
    {
        public byte[] Decrypt(Stream input)
        {
            var engine = new CTR.AesEngine();

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
