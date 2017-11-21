using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Komponent.IO;
using Komponent.Interface;

namespace Komponent.Encryption
{
    [ExportMetadata("Name", "BlowFish CBC")]
    [ExportMetadata("TabPathEncrypt", "General/BlowFish/CBC")]
    [ExportMetadata("TabPathDecrypt", "General/BlowFish/CBC")]
    [Export("BlowFishCBC", typeof(IEncryption))]
    [Export(typeof(IEncryption))]
    public class BlowFishCBC : IEncryption
    {
        public byte[] Decrypt(Stream input)
        {
            var key = InputBox.Show("Input decryption key:", "Decrypt Blowfish CBC");
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

    [ExportMetadata("Name", "BlowFish ECB")]
    [ExportMetadata("TabPathEncrypt", "General/BlowFish/ECB")]
    [ExportMetadata("TabPathDecrypt", "General/BlowFish/ECB")]
    [Export("BlowFishECB", typeof(IEncryption))]
    [Export(typeof(IEncryption))]
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
            var enc = bf.Encrypt_ECB(new BinaryReaderX(input, true).ReadAllBytes());
            ms.Write(enc, 0, enc.Length);

            return ms.ToArray();
        }
    }

    [ExportMetadata("Name", "MT Framework Encryption")]
    [ExportMetadata("TabPathEncrypt", "Custom/MT Framework")]
    [ExportMetadata("TabPathDecrypt", "Custom/MT Framework")]
    [Export("MTFramework", typeof(IEncryption))]
    [Export(typeof(IEncryption))]
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
}
