using System;
using System.IO;
using System.Net;
using System.Numerics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Kuriimu.IO;
using System.Collections.Generic;

using Security.Cryptography;

namespace Kuriimu.CTR
{
    public enum AesMode
    {
        CCM = 0,
        CTR = 1,
        CBC = 2,
        ECB = 3
    }
    public class AesEngine
    {
        private byte[][] KeyXs;
        private byte[][] KeyYs;

        private byte[][] NormalKeys;

        private byte[] CTR_IV_NONCE;

        private byte[] MAC;

        private AesMode Mode;

        private int Slot;

        private Dictionary<ulong, byte[]> contLockSeeds;

        private bool _isdev;

        private byte[] OTP = null;
        private byte[] OTP_d = null;

        private byte[] _nandcid;

        public AesEngine()
        {
            KeyXs = new byte[0x40][];
            KeyYs = new byte[0x40][];
            NormalKeys = new byte[0x40][];

            CTR_IV_NONCE = new byte[0x10];
            MAC = new byte[0x10];
            _nandcid = new byte[0x10];

            for (int i = 0; i < NormalKeys.Length; i++)
            {
                KeyXs[i] = new byte[0x10];
                KeyYs[i] = new byte[0x10];
                NormalKeys[i] = new byte[0x10];
            }

            _isdev = false;

            InitializeKeyslots();
            Slot = 0;
        }

        public void SetDev(bool dev)
        {
            IsDev = dev;
            InitializeKeyslots();
        }

        public bool IsDev
        {
            get { return _isdev; }
            set { _isdev = value; }
        }

        public byte[] Encrypt(byte[] input)
        {
            byte[] output = new byte[input.Length];

            byte[] key = (byte[])(NormalKeys[Slot].Clone());
            byte[] ctr_iv_nonce = new byte[0x10];
            CTR_IV_NONCE.CopyTo(ctr_iv_nonce, 0);

            switch (Mode)
            {
                case AesMode.CCM:
                    byte[] nonce = ctr_iv_nonce.Take(0xC).ToArray();
                    using (var _aes = new AuthenticatedAesCng { Key = key, IV = nonce, CngMode = CngChainingMode.Ccm, Padding = PaddingMode.None })
                    {
                        using (var encryptor = _aes.CreateAuthenticatedEncryptor())
                        using (CryptoStream cs = new CryptoStream(new MemoryStream(output), encryptor, CryptoStreamMode.Write))
                        {
                            cs.Write(input, 0, input.Length);
                            cs.FlushFinalBlock();
                            SetMAC(encryptor.GetTag());
                        }
                    }
                    break;
                case AesMode.CBC:
                    using (var _aes = new AesManaged { Key = key, IV = ctr_iv_nonce, Mode = CipherMode.CBC, Padding = PaddingMode.None })
                    {
                        _aes.CreateEncryptor(_aes.Key, _aes.IV).TransformBlock(input, 0, input.Length, output, 0);
                    }
                    break;
                case AesMode.CTR:
                    using (var _aes = new AesCtr(ctr_iv_nonce))
                    {
                        _aes.CreateEncryptor(key).TransformBlock(input, 0, input.Length, output, 0);
                    }
                    break;
                case AesMode.ECB:
                    using (var _aes = new AesManaged { Key = key, IV = ctr_iv_nonce, Mode = CipherMode.ECB, Padding = PaddingMode.None })
                    {
                        _aes.CreateEncryptor(_aes.Key, _aes.IV).TransformBlock(input, 0, input.Length, output, 0);
                    }
                    break;
            }
            return output;
        }

        public void Encrypt(Stream inStream, Stream outStream, long count)
        {
            byte[] key = (byte[])(NormalKeys[Slot].Clone());
            byte[] ctr_iv_nonce = new byte[0x10];
            CTR_IV_NONCE.CopyTo(ctr_iv_nonce, 0);
            switch (Mode)
            {
                case AesMode.CCM:
                    byte[] nonce = ctr_iv_nonce.Take(0xC).ToArray();
                    using (var _aes = new AuthenticatedAesCng { Key = key, IV = nonce, CngMode = CngChainingMode.Ccm, Padding = PaddingMode.None })
                    {
                        using (var encryptor = _aes.CreateAuthenticatedEncryptor())
                        {
                            byte[] inBuf;
                            byte[] outBuf;
                            while (count > 0)
                            {
                                inBuf = new byte[count > 0x100000 ? 0x100000 : count];
                                outBuf = new byte[count > 0x100000 ? 0x100000 : count];
                                inStream.Read(inBuf, 0, inBuf.Length);
                                encryptor.TransformBlock(inBuf, 0, inBuf.Length, outBuf, 0);
                                outStream.Write(outBuf, 0, outBuf.Length);
                                count -= inBuf.Length;
                            }
                            SetMAC(encryptor.GetTag());
                        }
                    }
                    break;
                case AesMode.CBC:
                    using (var _aes = new AesManaged { Key = key, IV = ctr_iv_nonce, Mode = CipherMode.CBC, Padding = PaddingMode.None })
                    {
                        using (var encryptor = _aes.CreateEncryptor(_aes.Key, _aes.IV))
                        {
                            byte[] inBuf;
                            byte[] outBuf;
                            while (count > 0)
                            {
                                inBuf = new byte[count > 0x100000 ? 0x100000 : count];
                                outBuf = new byte[count > 0x100000 ? 0x100000 : count];
                                inStream.Read(inBuf, 0, inBuf.Length);
                                encryptor.TransformBlock(inBuf, 0, inBuf.Length, outBuf, 0);
                                outStream.Write(outBuf, 0, outBuf.Length);
                                count -= inBuf.Length;
                            }
                        }
                    }
                    break;
                case AesMode.CTR:
                    using (var _aes = new AesCtr(ctr_iv_nonce))
                    {
                        using (var encryptor = _aes.CreateEncryptor(key))
                        {
                            byte[] inBuf;
                            byte[] outBuf;
                            while (count > 0)
                            {
                                inBuf = new byte[count > 0x100000 ? 0x100000 : count];
                                outBuf = new byte[count > 0x100000 ? 0x100000 : count];
                                inStream.Read(inBuf, 0, inBuf.Length);
                                encryptor.TransformBlock(inBuf, 0, inBuf.Length, outBuf, 0);
                                outStream.Write(outBuf, 0, outBuf.Length);
                                count -= inBuf.Length;
                            }
                        }
                    }
                    break;
                case AesMode.ECB:
                    using (var _aes = new AesManaged { Key = key, IV = ctr_iv_nonce, Mode = CipherMode.ECB, Padding = PaddingMode.None })
                    {
                        using (var encryptor = _aes.CreateEncryptor(key, _aes.IV))
                        {
                            byte[] inBuf;
                            byte[] outBuf;
                            while (count > 0)
                            {
                                inBuf = new byte[count > 0x100000 ? 0x100000 : count];
                                outBuf = new byte[count > 0x100000 ? 0x100000 : count];
                                inStream.Read(inBuf, 0, inBuf.Length);
                                encryptor.TransformBlock(inBuf, 0, inBuf.Length, outBuf, 0);
                                outStream.Write(outBuf, 0, outBuf.Length);
                                count -= inBuf.Length;
                            }
                        }
                    }
                    break;
            }
        }

        public byte[] Decrypt(byte[] input)
        {
            byte[] output = new byte[input.Length];

            byte[] key = (byte[])(NormalKeys[Slot].Clone());
            byte[] ctr_iv_nonce = new byte[0x10];
            CTR_IV_NONCE.CopyTo(ctr_iv_nonce, 0);

            switch (Mode)
            {
                case AesMode.CCM:
                    byte[] nonce = ctr_iv_nonce.Take(0xC).ToArray();
                    using (var _aes = new AuthenticatedAesCng { Key = key, IV = nonce, Tag = GetMAC(), CngMode = CngChainingMode.Ccm, Padding = PaddingMode.None })
                    {
                        var enc = _aes.CreateDecryptor();
                        using (CryptoStream cs = new CryptoStream(new MemoryStream(output), enc, CryptoStreamMode.Write))
                        {
                            cs.Write(input, 0, input.Length);
                            cs.FlushFinalBlock();
                        }
                    }
                    break;
                case AesMode.CBC:
                    using (var _aes = new AesManaged { Key = key, IV = ctr_iv_nonce, Mode = CipherMode.CBC, Padding = PaddingMode.None })
                    {
                        _aes.CreateDecryptor(_aes.Key, _aes.IV).TransformBlock(input, 0, input.Length, output, 0);
                    }
                    break;
                case AesMode.CTR:
                    using (var _aes = new AesCtr(ctr_iv_nonce))
                    {
                        _aes.CreateDecryptor(key).TransformBlock(input, 0, input.Length, output, 0);
                    }
                    break;
                case AesMode.ECB:
                    using (var _aes = new AesManaged { Key = key, IV = ctr_iv_nonce, Mode = CipherMode.ECB, Padding = PaddingMode.None })
                    {
                        _aes.CreateDecryptor(_aes.Key, _aes.IV).TransformBlock(input, 0, input.Length, output, 0);
                    }
                    break;
            }

            return output;
        }

        public void Decrypt(Stream inStream, Stream outStream, long count)
        {
            byte[] key = (byte[])(NormalKeys[Slot].Clone());
            byte[] ctr_iv_nonce = new byte[0x10];
            CTR_IV_NONCE.CopyTo(ctr_iv_nonce, 0);
            switch (Mode)
            {
                case AesMode.CCM:
                    byte[] nonce = ctr_iv_nonce.Take(0xC).ToArray();
                    using (var _aes = new AuthenticatedAesCng { Key = key, IV = nonce, Tag = GetMAC(), CngMode = CngChainingMode.Ccm, Padding = PaddingMode.None })
                    {
                        using (var decryptor = _aes.CreateAuthenticatedEncryptor())
                        {
                            byte[] inBuf;
                            byte[] outBuf;
                            while (count > 0)
                            {
                                inBuf = new byte[count > 0x100000 ? 0x100000 : count];
                                outBuf = new byte[count > 0x100000 ? 0x100000 : count];
                                inStream.Read(inBuf, 0, inBuf.Length);
                                decryptor.TransformBlock(inBuf, 0, inBuf.Length, outBuf, 0);
                                outStream.Write(outBuf, 0, outBuf.Length);
                                count -= inBuf.Length;
                            }
                        }
                    }
                    break;
                case AesMode.CBC:
                    using (var _aes = new AesManaged { Key = key, IV = ctr_iv_nonce, Mode = CipherMode.CBC, Padding = PaddingMode.None })
                    {
                        using (var decryptor = _aes.CreateDecryptor(_aes.Key, _aes.IV))
                        {
                            byte[] inBuf;
                            byte[] outBuf;
                            while (count > 0)
                            {
                                inBuf = new byte[count > 0x100000 ? 0x100000 : count];
                                outBuf = new byte[count > 0x100000 ? 0x100000 : count];
                                inStream.Read(inBuf, 0, inBuf.Length);
                                decryptor.TransformBlock(inBuf, 0, inBuf.Length, outBuf, 0);
                                outStream.Write(outBuf, 0, outBuf.Length);
                                count -= inBuf.Length;
                            }
                        }
                    }
                    break;
                case AesMode.CTR:
                    using (var _aes = new AesCtr(ctr_iv_nonce))
                    {
                        using (var decryptor = _aes.CreateDecryptor(key))
                        {
                            byte[] inBuf;
                            byte[] outBuf;
                            while (count > 0)
                            {
                                inBuf = new byte[count > 0x100000 ? 0x100000 : count];
                                outBuf = new byte[count > 0x100000 ? 0x100000 : count];
                                inStream.Read(inBuf, 0, inBuf.Length);
                                decryptor.TransformBlock(inBuf, 0, inBuf.Length, outBuf, 0);
                                outStream.Write(outBuf, 0, outBuf.Length);
                                count -= inBuf.Length;
                            }
                        }
                    }
                    break;
                case AesMode.ECB:
                    using (var _aes = new AesManaged { Key = key, IV = ctr_iv_nonce, Mode = CipherMode.ECB, Padding = PaddingMode.None })
                    {
                        using (var decryptor = _aes.CreateDecryptor(key, _aes.IV))
                        {
                            byte[] inBuf;
                            byte[] outBuf;
                            while (count > 0)
                            {
                                inBuf = new byte[count > 0x100000 ? 0x100000 : count];
                                outBuf = new byte[count > 0x100000 ? 0x100000 : count];
                                inStream.Read(inBuf, 0, inBuf.Length);
                                decryptor.TransformBlock(inBuf, 0, inBuf.Length, outBuf, 0);
                                outStream.Write(outBuf, 0, outBuf.Length);
                                count -= inBuf.Length;
                            }
                        }
                    }
                    break;
            }
        }

        public void SelectKeyslot(int keyslot)
        {
            if (keyslot < 0 || keyslot >= 0x40)
                throw new ArgumentException("Invalid keyslot selected. Must be in range [0, 0x40).");
            Slot = keyslot;
        }

        public void SetMode(AesMode m)
        {
            Mode = m;
        }

        public void SetCTR(byte[] ctr)
        {
            if (ctr.Length != 0x10)
                return;
            ctr.CopyTo(CTR_IV_NONCE, 0);
        }

        public void SetNandCID(byte[] cid)
        {
            if (cid.Length != 0x10)
                return;
            cid.CopyTo(_nandcid, 0);
        }

        public void SetOTP(byte[] otp)
        {
            if (otp.Length != 0x100)
                return;
            OTP = (byte[])(otp.Clone());

            InitializeKeyslots();
        }

        public byte[] GetDecryptedOTP()
        {
            return (byte[])(OTP_d.Clone());
        }


        public void AdvanceCTR(uint adv)
        {
            ulong current = (BigEndian.ToUInt32(CTR_IV_NONCE, 8));
            current <<= 32;
            current |= (BigEndian.ToUInt32(CTR_IV_NONCE, 0xC));
            ulong next = current + adv;
            BigEndian.GetBytes((uint)(next & 0xFFFFFFFF)).CopyTo(CTR_IV_NONCE, 12);
            BigEndian.GetBytes((uint)((next >> 32) & 0xFFFFFFFF)).CopyTo(CTR_IV_NONCE, 8);
            // Handle u64 overflow.
            if (next < current)
            {
                for (var ofs = 7; ofs >= 0; ofs--)
                {
                    if ((++CTR_IV_NONCE[ofs]) != 0)
                        break;
                }
            }
        }

        public void SetCTR(ulong high, ulong low)
        {
            BitConverter.GetBytes(high).Reverse().ToArray().CopyTo(CTR_IV_NONCE, 0);
            BitConverter.GetBytes(low).Reverse().ToArray().CopyTo(CTR_IV_NONCE, 8);
        }

        public void SetIV(byte[] iv)
        {
            if (iv.Length != 0x10)
                return;
            iv.CopyTo(CTR_IV_NONCE, 0);
        }

        public void SetNonce(byte[] nonce)
        {
            if (nonce.Length != 0xC)
                return;
            byte[] n = new byte[0x10];
            nonce.CopyTo(n, 0);
            n.CopyTo(CTR_IV_NONCE, 0);
        }

        public void SetMAC(byte[] mac)
        {
            if (mac.Length != 0x10)
                return;
            mac.CopyTo(MAC, 0);
        }

        public byte[] GetMAC()
        {
            return (byte[])MAC.Clone();
        }

        public void SetKeyX(int keyslot, byte[] key)
        {
            if (key.Length != 0x10)
                return;
            key.CopyTo(KeyXs[keyslot], 0);
            if (keyslot <= 3)
                KeyScrambler.GetDSINormalKey(KeyXs[keyslot], KeyYs[keyslot]).CopyTo(NormalKeys[keyslot], 0);
            else
                KeyScrambler.GetNormalKey(KeyXs[keyslot], KeyYs[keyslot]).CopyTo(NormalKeys[keyslot], 0);
        }

        public void SetKeyX(byte[] key)
        {
            if (key.Length != 0x10)
                return;
            key.CopyTo(KeyXs[Slot], 0);
            if (Slot <= 3)
                KeyScrambler.GetDSINormalKey(KeyXs[Slot], KeyYs[Slot]).CopyTo(NormalKeys[Slot], 0);
            else
                KeyScrambler.GetNormalKey(KeyXs[Slot], KeyYs[Slot]).CopyTo(NormalKeys[Slot], 0);
        }

        public void SetKeyY(int keyslot, byte[] key)
        {
            if (key.Length != 0x10)
                return;
            key.CopyTo(KeyYs[keyslot], 0);
            if (keyslot <= 3)
                KeyScrambler.GetDSINormalKey(KeyXs[keyslot], KeyYs[keyslot]).CopyTo(NormalKeys[keyslot], 0);
            else
                KeyScrambler.GetNormalKey(KeyXs[keyslot], KeyYs[keyslot]).CopyTo(NormalKeys[keyslot], 0);
        }

        public void SetKeyY(byte[] key)
        {
            if (key.Length != 0x10)
                return;
            key.CopyTo(KeyYs[Slot], 0);
            if (Slot <= 3)
                KeyScrambler.GetDSINormalKey(KeyXs[Slot], KeyYs[Slot]).CopyTo(NormalKeys[Slot], 0);
            else
                KeyScrambler.GetNormalKey(KeyXs[Slot], KeyYs[Slot]).CopyTo(NormalKeys[Slot], 0);
        }

        public void SetNormalKey(int keyslot, byte[] key)
        {
            if (key.Length != 0x10)
                return;
            key.CopyTo(NormalKeys[keyslot], 0);
        }
        public void SetNormalKey(byte[] key)
        {
            if (key.Length != 0x10)
                return;
            key.CopyTo(NormalKeys[Slot], 0);
        }

        public void InitializeKeyslots()
        {
            LoadBootromKeys();
            LoadConsoleUniqueKeys();
            LoadFirmwareKeys();
        }

        public byte[] GetKeyX(uint i)
        {
            if (i >= 0x40)
                return null;
            return (byte[])(KeyXs[i].Clone());
        }
        public byte[] GetKeyY(uint i)
        {
            if (i >= 0x40)
                return null;
            return (byte[])(KeyYs[i].Clone());
        }
        public byte[] GetKey(uint i)
        {
            if (i >= 0x40)
                return null;
            return (byte[])(NormalKeys[i].Clone());
        }

        public void LoadContentLockSeeds()
        {
            var tmp = File.OpenRead("bin\\seeddb.bin");
            contLockSeeds = new Dictionary<ulong, byte[]>();
            using (var br = new BinaryReaderX(tmp))
            {
                var entryCount = br.ReadUInt32();
                br.BaseStream.Position += 0xc;
                for (int i = 0; i < entryCount; i++)
                {
                    var titleID = br.ReadBytes(8).Reverse().ToArray().BytesToStruct<ulong>();
                    var seed = br.ReadBytes(0x10);
                    contLockSeeds.Add(titleID, seed);
                    br.BaseStream.Position += 0x8;
                }
            }
        }

        public void LoadBootromKeys()
        {
            var tmp = File.OpenRead("bin\\boot9.bin");
            byte[] boot9 = new byte[tmp.Length];
            tmp.Read(boot9, 0, (int)tmp.Length);
            tmp.Close();
            LoadKeysFromBootromFile(boot9);
        }

        public void LoadKeysFromBootromFile(byte[] boot9)
        {
            // Will use LoadKeysFromBootrom() implementation for those who
            // don't want to manually compile with bootrom as a resource.
            var keyarea_ofs = (boot9.Length == 0x10000) ? (IsDev) ? 0xDC60 : 0xD860 : (IsDev) ? 0x5C60 : 0x5860;

            var keyX = new byte[0x10];
            var keyY = new byte[0x10];
            var normkey = new byte[0x10];

            // Skip over AESIV for consolue_unique data
            keyarea_ofs += 0x24;
            // Block 0
            keyarea_ofs += 0x74;
            // Block 1
            keyarea_ofs += 0x44;
            // Block 2
            keyarea_ofs += 0x74;
            // Block 3
            keyarea_ofs += 0x20;

            // 0x2C KeyX
            Array.Copy(boot9, keyarea_ofs, keyX, 0, 0x10);
            for (var i = 0; i < 4; i++)
                SetKeyX(0x2C + i, keyX);
            keyarea_ofs += 0x10;

            // 0x30 KeyX
            Array.Copy(boot9, keyarea_ofs, keyX, 0, 0x10);
            for (var i = 0; i < 4; i++)
                SetKeyX(0x30 + i, keyX);
            keyarea_ofs += 0x10;

            // 0x34 KeyX
            Array.Copy(boot9, keyarea_ofs, keyX, 0, 0x10);
            for (var i = 0; i < 4; i++)
                SetKeyX(0x34 + i, keyX);
            keyarea_ofs += 0x10;

            // 0x38 KeyX
            Array.Copy(boot9, keyarea_ofs, keyX, 0, 0x10);
            for (var i = 0; i < 4; i++)
                SetKeyX(0x38 + i, keyX);
            keyarea_ofs += 0x10;

            // 0x3C-0x3F KeyX
            for (var i = 0; i < 4; i++)
            {
                Array.Copy(boot9, keyarea_ofs, keyX, 0, 0x10);
                SetKeyX(0x3C + i, keyX);
                keyarea_ofs += 0x10;
            }

            // 0x4-0xB KeyY
            for (var i = 0; i < 8; i++)
            {
                Array.Copy(boot9, keyarea_ofs, keyY, 0, 0x10);
                SetKeyY(0x4 + i, keyY);
                keyarea_ofs += 0x10;
            }

            // 0xC Normkey
            Array.Copy(boot9, keyarea_ofs, normkey, 0, 0x10);
            for (var i = 0; i < 4; i++)
                SetNormalKey(0xC + i, normkey);
            keyarea_ofs += 0x10;

            // 0x10 Normkey
            Array.Copy(boot9, keyarea_ofs, normkey, 0, 0x10);
            for (var i = 0; i < 4; i++)
                SetNormalKey(0x10 + i, normkey);
            keyarea_ofs += 0x10;

            // 0x14-0x17 normkey
            for (var i = 0; i < 4; i++)
            {
                Array.Copy(boot9, keyarea_ofs, normkey, 0, 0x10);
                SetNormalKey(0x14 + i, normkey);
                keyarea_ofs += 0x10;
            }

            // 0x18 normkey
            Array.Copy(boot9, keyarea_ofs, normkey, 0, 0x10);
            for (var i = 0; i < 4; i++)
                SetNormalKey(0x18 + i, normkey);
            keyarea_ofs += 0x10;

            // 0x1C Normkey
            Array.Copy(boot9, keyarea_ofs, normkey, 0, 0x10);
            for (var i = 0; i < 4; i++)
                SetNormalKey(0x1C + i, normkey);
            keyarea_ofs += 0x10;

            // 0x20 Normkey
            Array.Copy(boot9, keyarea_ofs, normkey, 0, 0x10);
            for (var i = 0; i < 4; i++)
                SetNormalKey(0x20 + i, normkey);
            keyarea_ofs += 0x10;

            // 0x24 Normkey
            Array.Copy(boot9, keyarea_ofs, normkey, 0, 0x10);
            for (var i = 0; i < 4; i++)
                SetNormalKey(0x24 + i, normkey);
            // No increase

            // 0x28-0x2C normkey
            for (var i = 0; i < 4; i++)
            {
                Array.Copy(boot9, keyarea_ofs, normkey, 0, 0x10);
                SetNormalKey(0x28 + i, normkey);
                keyarea_ofs += 0x10;
            }

            // 0x2C Normkey
            Array.Copy(boot9, keyarea_ofs, normkey, 0, 0x10);
            for (var i = 0; i < 4; i++)
                SetNormalKey(0x2C + i, normkey);
            keyarea_ofs += 0x10;

            // 0x30 Normkey
            Array.Copy(boot9, keyarea_ofs, normkey, 0, 0x10);
            for (var i = 0; i < 4; i++)
                SetNormalKey(0x30 + i, normkey);
            keyarea_ofs += 0x10;

            // 0x34 Normkey
            Array.Copy(boot9, keyarea_ofs, normkey, 0, 0x10);
            for (var i = 0; i < 4; i++)
                SetNormalKey(0x34 + i, normkey);
            keyarea_ofs += 0x10;

            // 0x38 Normkey
            Array.Copy(boot9, keyarea_ofs, normkey, 0, 0x10);
            for (var i = 0; i < 4; i++)
                SetNormalKey(0x38 + i, normkey);

            // 0x3C-0x3F normkeys
            for (var i = 0; i < 4; i++)
            {
                Array.Copy(boot9, keyarea_ofs, normkey, 0, 0x10);
                SetNormalKey(0x3C + i, normkey);
                keyarea_ofs += 0x10;
            }

        }

        public byte[] DecryptOTP(byte[] o)
        {
            if (o.Length != 0x100)
            {
                return null;
            }

            var tmp = File.OpenRead("bin\\boot9.bin");
            byte[] boot9 = new byte[tmp.Length];
            tmp.Read(boot9, 0, (int)tmp.Length);
            tmp.Close();

            var otpkey_ofs = (boot9.Length == 0x10000) ? (IsDev) ? 0xD710 : 0xD6E0 : (IsDev) ? 0x5710 : 0x56E0;
            var otpkey = new byte[0x10];
            var otpiv = new byte[0x10];
            var OTP_dec = new byte[o.Length];
            Array.Copy(boot9, otpkey_ofs, otpkey, 0, 0x10);
            Array.Copy(boot9, otpkey_ofs + 0x10, otpiv, 0, 0x10);
            using (var _aes = new AesManaged { Key = otpkey, IV = otpiv, Mode = CipherMode.CBC, Padding = PaddingMode.None })
            {
                _aes.CreateDecryptor(_aes.Key, _aes.IV).TransformBlock(OTP, 0, OTP.Length, OTP_dec, 0);
            }


            return OTP_dec;
        }

        public void LoadConsoleUniqueKeys()
        {
            if (OTP == null)
                return;

            var tmp = File.OpenRead("bin\\boot9.bin");
            byte[] boot9 = new byte[tmp.Length];
            tmp.Read(boot9, 0, (int)tmp.Length);
            tmp.Close();

            OTP_d = DecryptOTP(OTP);

            var keyarea_ofs = (IsDev) ? 0x5C60 : 0x5860;
            var otp_pos = (new SHA256Managed().ComputeHash(OTP_d, 0, 0xE0).SequenceEqual(OTP_d.Skip(0xE0))) ? 0x90 : 0x0;
            var hashdata = new byte[0x40];
            Array.Copy(OTP_d, otp_pos, hashdata, 0, 0x1C);
            Array.Copy(boot9, keyarea_ofs, hashdata, 0x1C, 0x24);
            var hash = new SHA256Managed().ComputeHash(hashdata);
            SetMode(AesMode.CBC);
            SetKeyX(0x3F, hash.Take(0x10).ToArray());
            SetKeyY(0x3F, hash.Skip(0x10).ToArray());
            SelectKeyslot(0x3F);

            keyarea_ofs += 0x24;

            var aesiv = new byte[0x10];
            Array.Copy(boot9, keyarea_ofs, aesiv, 0, 0x10);
            keyarea_ofs += 0x10;
            Array.Copy(boot9, keyarea_ofs, hashdata, 0, 0x40);
            keyarea_ofs += 0x40;
            SetIV(aesiv);
            var keydata = Encrypt(hashdata);
            for (var i = 0; i < 4; i++)
            {
                SetKeyX(0x4 + i, keydata.Skip(0x00).Take(0x10).ToArray());
                SetKeyX(0x8 + i, keydata.Skip(0x10).Take(0x10).ToArray());
                SetKeyX(0xC + i, keydata.Skip(0x20).Take(0x10).ToArray());
            }
            SetKeyX(0x10, keydata.Skip(0x30).Take(0x10).ToArray());

            Array.Copy(boot9, keyarea_ofs, aesiv, 0, 0x10);
            keyarea_ofs += 0x10;
            Array.Copy(boot9, keyarea_ofs, hashdata, 0, 0x40);
            keyarea_ofs += 0x10;
            SetIV(aesiv);
            keydata = Encrypt(hashdata);
            for (var i = 0; i < 4; i++)
            {
                SetKeyX(0x14 + i, keydata.Skip(0x10 * i).Take(0x10).ToArray());
            }

            Array.Copy(boot9, keyarea_ofs, aesiv, 0, 0x10);
            keyarea_ofs += 0x10;
            Array.Copy(boot9, keyarea_ofs, hashdata, 0, 0x40);
            keyarea_ofs += 0x40;
            SetIV(aesiv);
            keydata = Encrypt(hashdata);
            for (var i = 0; i < 4; i++)
            {
                SetKeyX(0x18 + i, keydata.Skip(0x00).Take(0x10).ToArray());
                SetKeyX(0x1C + i, keydata.Skip(0x10).Take(0x10).ToArray());
                SetKeyX(0x20 + i, keydata.Skip(0x20).Take(0x10).ToArray());
            }
            SetKeyX(0x24, keydata.Skip(0x30).Take(0x10).ToArray());

            Array.Copy(boot9, keyarea_ofs, aesiv, 0, 0x10);
            keyarea_ofs += 0x10;
            Array.Copy(boot9, keyarea_ofs, hashdata, 0, 0x40);
            SetIV(aesiv);
            keydata = Encrypt(hashdata);
            for (var i = 0; i < 4; i++)
            {
                SetKeyX(0x28 + i, keydata.Skip(0x10 * i).Take(0x10).ToArray());
            }

            // Console unique data loading not yet implemented.
        }

        public void LoadFirmwareKeys()
        {
            // Buffers extracted from Process9 .data. 
            var buf1 = "A48DE4F10B3644AA903128FF4DCA76DF".ToByteArray();
            var buf2 = "DDDAA4C62CC450E9DAB69B0D9D2A2198".ToByteArray();

            byte[] keysector;
            if (IsDev)
            {
                var tmp = File.OpenRead("bin\\n3ds_keysector_dev.bin");
                byte[] key_dev = new byte[tmp.Length];
                tmp.Read(key_dev, 0, (int)tmp.Length);
                tmp.Close();

                keysector = key_dev;
            }
            else
            {
                var tmp = File.OpenRead("bin\\n3ds_keysector_retail.bin");
                byte[] key_ret = new byte[tmp.Length];
                tmp.Read(key_ret, 0, (int)tmp.Length);
                tmp.Close();

                keysector = key_ret;
            }

            var first_key = new byte[0x10];
            var second_key = new byte[0x10];
            Array.Copy(keysector, 0, first_key, 0, 0x10);
            Array.Copy(keysector, 0x10, second_key, 0, 0x10);

            SetMode(AesMode.ECB);

            SetNormalKey(0x11, first_key);
            SelectKeyslot(0x11);
            SetKeyX(0x18, Decrypt(buf1));

            SetNormalKey(0x11, second_key);
            SelectKeyslot(0x11);
            for (var i = 0x19; i < 0x20; i++)
            {
                SetKeyX(i, Decrypt(buf2));
                buf2[0xF]++;
            }

            var P9KeyY = "7462553F9E5A7904B8647CCA736DA1F5".ToByteArray();
            SetKeyY(0x31, P9KeyY);
            SetKeyY(0x39, P9KeyY);
            SetKeyY(0x2E, P9KeyY);
            SetKeyY(0x14, P9KeyY);

            // Firmware initialized keys for Keyslots 0x25, 0x2F.
            var modulus = IsDev ? "00EB686B6005538368435FAE6D2016521E218058B17D9B67EA1B71042835FA39F86BFD0BDD11445846705A49E5D26117528F4DE4A0D5F186D068DE06D8382FBFCAAE403A61540B6E3A24D9A283D340B6BEC6802DAB3441E4260639D66B134E8123FB06748926CC93089E3FE539DA765A7CC0486EBAEC74C26F2B41A8B118E3E9355104BE57DAA370494E09DAC7611D625639EFDE63E02B71EDB00CBA9509E49A3A8C46F19B22107193FF840088BEB3AD510E720B024A36FBBED81B4AC0A0512ED63E1346FDBD8452C0160F8774FFDBE708C761F49DD9CA138B418D947E98774A87A4CDAAED7021C5AA30F88ACC4B07E531BBDFC71A879AE85EDB8214F6D23910DB".ToByteArray() : "00C12E4877FF0FEDB98AFA6DBE5D7AC86489A4E0901DBA22D2E7BA945C830126CA71976F284A5E57E386B4778FBB78E3F1750ECD8B6ED3984F3CD363F801DD9ED5271EA859F49FF4DA02D88445EFBA774B3A46279167CB905300D0903ECB71772B78CFE918921E2BEF699F87D9CBEF273FBC0E320D177E925021E869BB827F227F17DA206AE9CE5DE262566C4FAB0BE86866F7218FD48C248C0D8CBDEB3ADBC423A6CD1B572F82C0172D23E32B1B4F0C30B04C20749126851DBA39B31A2918C23F91034969E601C0A1099742E8FD31CFE4A02CD9E6BD7B646C52465567C7463B2A9E7E9FA53A0E260D4DAEE68091EE7ADFA1C1491538276161E1C10293F4592A33".ToByteArray();
            var exponent = IsDev ? "007C2656B28E3C8C09F9B6456451CED3C206430D03964DD650D5E6159E2B516B2F43AC718C4625F83435DEB828577A52F57C9444E639CEA61DD5A07A959F94371EEA24FF6503110031F3EB3F552C9D6D8351669A2F376124B49505FDB9FCB5A34FD97C7F0B320623E920A46BFCC5E53335A5A6CD97114B469BA0818138DCFA9DAB389728238BE1A715D584E79DC7F6188DA5D446C54E9C8B009910F46E5718081EA01CBF1DD0AED5073B7EA83BE3852286CCAB51D2C9ABA12ABF173593069FB4109113CF3B68186BDDD09B891AEAA8402F1875A19CC88CCA52141E684FEB8735E84D65446BA7C3B616A3AB5744DA590C8030AAAAB28E523AE437B64C39996E0FD1".ToByteArray() : "00C034829A11C7116A08633E89A78CA0919779DA8CC967077AFC60E1786247E5068B9A76588A15F0304B3887B5147C259F7A2E6E480ACEB0BE35F0C5885EA2D8838FD6AAFE45AC58FC08D4D0569D3CD3B09F9C6985FCD5C7152EBC54A885D6B11129B503611510BBFA0B07552D5800ECF636EF90D101CD475A3F4274E0C1E828B2BB516C8CD80F95F5DB6A239083435509C0190E5D218B19C3BEBC2A3AA73CEBF53E7E4998D9BE01A440F3F1C0F3157BA6F65A7BE9E059D4D26F9DE997141979DF2CEAA175D91DB2079051978F805D6F97741B6DB4E96E3305611392B4B7CA76F0B45387C40D5879CDE6B1509062216CB9AC21F3510CCD6DF4DA8F581FF86D2F31".ToByteArray();

            var secret_data = "A20F47820E3EEF0336FCFCE37220CFD49BFE5A93BB8755ACD3043FEF5D507ED42459BE6EA64BED00FD28CCFB25A72F9DC74C7359C130E9DE332A6B8C33D18039".ToByteArray();
            var message = "0001FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF003031300D0609608648016503040201050004200000000000000000000000000000000000000000000000000000000000000000".ToByteArray();

            (new SHA256Managed().ComputeHash(secret_data)).CopyTo(message, 0xE0);

            var N = new BigInteger(modulus.Reverse().ToArray());
            var D = new BigInteger(exponent.Reverse().ToArray());
            var M = new BigInteger(message.Reverse().ToArray());

            var sig = BigInteger.ModPow(M, D, N).ToByteArray().Reverse().ToArray();
            if (sig.Length < 0x100)
                sig = new byte[0x100 - sig.Length].Concat(sig).ToArray();
            else if (sig.Length > 0x100)
                sig = sig.Skip(sig.Length - 0x100).ToArray();

            var keydata_6x7x = (new SHA256Managed().ComputeHash(sig));

            SetKeyY(0x2F, keydata_6x7x.Take(0x10).ToArray());
            SetKeyX(0x25, keydata_6x7x.Skip(0x10).ToArray());
        }

        public void SetDLPKeyY()
        {
            var P9KeyY = "7462553F9E5A7904B8647CCA736DA1F5".ToByteArray();
            SetKeyY(0x39, P9KeyY);
        }

        public void SetNFCKeyY()
        {
            var KeyY = IsDev ? "E02D27441DB9558BAD087FD746DF1057".ToByteArray() : "ED7858A8BBA7EED7FC970C5979BC0AF2".ToByteArray();
            SetKeyY(0x39, KeyY);
        }

        public void SetCommonKeyY(uint i)
        {
            if (i > 5)
                i = 0;
            var keys = new[] { IsDev ? "85215E96CB95A9ECA4B4DE601CB562C7".ToByteArray() : "D07B337F9CA4385932A2E25723232EB9".ToByteArray(),
                                "0C767230F0998F1C46828202FAACBE4C".ToByteArray(),
                                "C475CB3AB8C788BB575E12A10907B8A4".ToByteArray(),
                                "E486EEE3D0C09C902F6686D4C06F649F".ToByteArray(),
                                "ED31BA9C04B067506C4497A35B7804FC".ToByteArray(),
                                "5E66998AB4E8931606850FD7A16DD755".ToByteArray() };
            SetKeyY(0x3D, keys[i]);
        }

        public byte[] DecryptFIRM(byte[] FIRM, bool debug = false)
        {
            byte[] keysector;
            if (IsDev)
            {
                var tmp = File.OpenRead("bin\\n3ds_keysector_dev.bin");
                byte[] key_dev = new byte[tmp.Length];
                tmp.Read(key_dev, 0, (int)tmp.Length);
                tmp.Close();

                keysector = key_dev;
            }
            else
            {
                var tmp = File.OpenRead("bin\\n3ds_keysector_retail.bin");
                byte[] key_ret = new byte[tmp.Length];
                tmp.Read(key_ret, 0, (int)tmp.Length);
                tmp.Close();

                keysector = key_ret;
            }

            var dec_firm = (byte[])FIRM.Clone();
            if (Encoding.ASCII.GetString(FIRM, 0, 4) != "FIRM")
                return dec_firm;

            var header_ofs = -1;
            for (var i = 0; i < 4; i++)
            {
                if (BitConverter.ToUInt32(FIRM, 0x4C + 0x30 * i) == 0)
                {
                    header_ofs = BitConverter.ToInt32(FIRM, 0x40 + 0x30 * i);
                    break;
                }
            }

            var k9l2 = Encoding.ASCII.GetString(FIRM, header_ofs + 0x50, 4) == "K9L2";

            var key_x_15 = new byte[0x10];
            var key_x_16 = new byte[0x10];
            var key_y = new byte[0x10];
            var ctr = new byte[0x10];



            Array.Copy(FIRM, header_ofs + 0x00, key_x_15, 0, 0x10);
            Array.Copy(FIRM, header_ofs + 0x10, key_y, 0, 0x10);
            Array.Copy(FIRM, header_ofs + 0x20, ctr, 0, 0x10);
            Array.Copy(FIRM, header_ofs + 0x60, key_x_16, 0, 0x10);

            var secret_key = new byte[0x10];
            Array.Copy(keysector, 0, secret_key, 0, 0x10);
            SetNormalKey(0x11, secret_key);
            SetMode(AesMode.ECB);
            SelectKeyslot(0x11);
            key_x_15 = Decrypt(key_x_15);
            Array.Copy(keysector, 0x10, secret_key, 0, 0x10);
            SetNormalKey(0x11, secret_key);
            key_x_16 = Decrypt(key_x_16);

            if (k9l2)
            {
                SetKeyX(0x16, key_x_16);
                SetKeyY(0x16, key_y);
                SelectKeyslot(0x16);
            }
            else
            {
                SetKeyX(0x15, key_x_15);
                SetKeyY(0x15, key_y);
                SelectKeyslot(0x15);
            }
            SetMode(AesMode.CTR);
            SetCTR(ctr);

            var len = uint.Parse(Encoding.ASCII.GetString(FIRM, header_ofs + 0x30, 8));
            var arm9_bin = new byte[len];
            Array.Copy(FIRM, header_ofs + 0x800, arm9_bin, 0, len);
            Array.Copy(Decrypt(arm9_bin), 0, dec_firm, header_ofs + 0x800, len);

            return dec_firm;
        }

        public byte[] EncryptFIRM(byte[] FIRM, bool debug = false)
        {
            byte[] keysector;
            if (IsDev)
            {
                var tmp = File.OpenRead("bin\\n3ds_keysector_dev.bin");
                byte[] key_dev = new byte[tmp.Length];
                tmp.Read(key_dev, 0, (int)tmp.Length);
                tmp.Close();

                keysector = key_dev;
            }
            else
            {
                var tmp = File.OpenRead("bin\\n3ds_keysector_retail.bin");
                byte[] key_ret = new byte[tmp.Length];
                tmp.Read(key_ret, 0, (int)tmp.Length);
                tmp.Close();

                keysector = key_ret;
            }

            var enc_firm = (byte[])FIRM.Clone();
            if (Encoding.ASCII.GetString(FIRM, 0, 4) != "FIRM")
                return enc_firm;

            var header_ofs = -1;
            for (var i = 0; i < 4; i++)
            {
                if (BitConverter.ToUInt32(FIRM, 0x4C + 0x30 * i) == 0)
                {
                    header_ofs = BitConverter.ToInt32(FIRM, 0x40 + 0x30 * i);
                    break;
                }
            }

            var k9l2 = Encoding.ASCII.GetString(FIRM, header_ofs + 0x50, 4) == "K9L2";

            var key_x_15 = new byte[0x10];
            var key_x_16 = new byte[0x10];
            var key_y = new byte[0x10];
            var ctr = new byte[0x10];



            Array.Copy(FIRM, header_ofs + 0x00, key_x_15, 0, 0x10);
            Array.Copy(FIRM, header_ofs + 0x10, key_y, 0, 0x10);
            Array.Copy(FIRM, header_ofs + 0x20, ctr, 0, 0x10);
            Array.Copy(FIRM, header_ofs + 0x60, key_x_16, 0, 0x10);

            var secret_key = new byte[0x10];
            Array.Copy(keysector, 0, secret_key, 0, 0x10);
            SetNormalKey(0x11, secret_key);
            SetMode(AesMode.ECB);
            SelectKeyslot(0x11);
            key_x_15 = Decrypt(key_x_15);
            Array.Copy(keysector, 0x10, secret_key, 0, 0x10);
            SetNormalKey(0x11, secret_key);
            key_x_16 = Decrypt(key_x_16);

            if (k9l2)
            {
                SetKeyX(0x16, key_x_16);
                SetKeyY(0x16, key_y);
                SelectKeyslot(0x16);
            }
            else
            {
                SetKeyX(0x15, key_x_15);
                SetKeyY(0x15, key_y);
                SelectKeyslot(0x15);
            }
            SetMode(AesMode.CTR);
            SetCTR(ctr);

            var len = uint.Parse(Encoding.ASCII.GetString(FIRM, header_ofs + 0x30, 8));
            var arm9_bin = new byte[len];
            Array.Copy(FIRM, header_ofs + 0x800, arm9_bin, 0, len);
            Array.Copy(Encrypt(arm9_bin), 0, enc_firm, header_ofs + 0x800, len);

            return enc_firm;
        }


        public void DecryptGameNCSD(Stream ncsdInStream, Stream ncsdOutStream)
        {
            if (ncsdInStream.Position != ncsdOutStream.Position)
                throw new ArgumentException("Instream and Outstream must be synchronized.");
            byte[] _u32 = new byte[4];
            ncsdInStream.Seek(0x100, SeekOrigin.Begin);
            ncsdInStream.Read(_u32, 0, 4);
            string magic = new string(_u32.Select(b => (char)b).ToArray());
            if (magic != "NCSD")
                throw new ArgumentException("Invalid NCSD passed to decryption method!");
            ncsdInStream.Seek(0x120, SeekOrigin.Begin);
            long[] Offsets = new long[8];
            long[] Sizes = new long[8];
            for (int i = 0; i < 8; i++)
            {
                ncsdInStream.Read(_u32, 0, 4);
                Offsets[i] = BitConverter.ToUInt32(_u32, 0) * 0x200;
                ncsdInStream.Read(_u32, 0, 4);
                Sizes[i] = BitConverter.ToUInt32(_u32, 0) * 0x200;
            }

            for (int i = 0; i < 8; i++)
            {
                if (Sizes[i] > 0)
                {
                    ncsdInStream.Seek(Offsets[i], SeekOrigin.Begin);
                    ncsdOutStream.Seek(Offsets[i], SeekOrigin.Begin);
                    DecryptNCCH(ncsdInStream, ncsdOutStream);
                }
            }
            ncsdInStream.Seek(0, SeekOrigin.Begin);
            ncsdOutStream.Seek(0, SeekOrigin.Begin);
        }

        public void DecryptNCCH(Stream ncchInStream, Stream ncchOutStream, byte[] seed = null)
        {
            var StartOffset = ncchInStream.Position;
            if (StartOffset != ncchOutStream.Position)
                throw new ArgumentException("Instream and Outstream must be synchronized.");
            byte[] NCCHKeyY = new byte[0x10];
            byte[] SeedKeyY = new byte[0x10];
            ncchInStream.Read(NCCHKeyY, 0, NCCHKeyY.Length);
            #region GetNCCHMetadata
            BinaryReader br = new BinaryReader(ncchInStream);
            br.BaseStream.Seek(0xF0, SeekOrigin.Current);
            string magic = new string(br.ReadChars(4));
            if (magic != "NCCH")
                throw new ArgumentException("Invalid NCCH passed to decryption method!");
            br.BaseStream.Seek(4, SeekOrigin.Current);
            ulong PartitionID = br.ReadUInt64();
            br.BaseStream.Seek(4, SeekOrigin.Current);
            uint SeedHash = br.ReadUInt32();
            ulong ProgramID = br.ReadUInt64();
            br.BaseStream.Seek(0x60, SeekOrigin.Current);
            uint ExheaderLen = br.ReadUInt32();
            br.BaseStream.Seek(4, SeekOrigin.Current);
            br.BaseStream.Seek(3, SeekOrigin.Current);
            byte CryptoMethod = br.ReadByte();
            br.BaseStream.Seek(3, SeekOrigin.Current);
            byte BitFlags = br.ReadByte();
            bool UsesFixedKey = (BitFlags & 1) > 0;
            bool UsesSeedCrypto = (BitFlags & 0x20) > 0;
            bool UsesCrypto = (BitFlags & 0x4) == 0;
            if (UsesFixedKey) // Fixed Key
            {
                byte[] zeroKey = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                byte[] sysKey = { 0x52, 0x7C, 0xE6, 0x30, 0xA9, 0xCA, 0x30, 0x5F, 0x36, 0x96, 0xF3, 0xCD, 0xE9, 0x54, 0x19, 0x4B };
                if ((PartitionID & ((ulong)0x10 << 32)) > 0)
                    SetNormalKey(0x11, sysKey);
                else
                    SetNormalKey(0x11, zeroKey);
            }
            if (UsesSeedCrypto) // Seed Crypto
            {
                if (seed == null)
                    seed = GetExtSeed(ProgramID);
                if (seed == null)
                    throw new ArgumentNullException("Seed must not be null for NCCH using seed crypto.");
                if (seed.Length != 0x10)
                    throw new ArgumentException("Content lock seeds must be 0x10 bytes long.");
                byte[] SeedCheck = new byte[0x18];
                seed.CopyTo(SeedCheck, 0);
                BitConverter.GetBytes(PartitionID).CopyTo(SeedCheck, 0x10);
                uint TestHash = BitConverter.ToUInt32((new SHA256Managed()).ComputeHash(SeedCheck), 0);
                if (TestHash != SeedHash)
                    throw new ArgumentException("Content lock seed is incorrect. Cannot decrypt NCCH.");
                byte[] SeedBuffer = new byte[0x20];
                NCCHKeyY.CopyTo(SeedBuffer, 0);
                seed.CopyTo(SeedBuffer, 0x10);
                Array.Copy((new SHA256Managed()).ComputeHash(SeedBuffer), 0, SeedKeyY, 0, 0x10);
            }
            var keyY = UsesSeedCrypto ? SeedKeyY : NCCHKeyY;

            // Assume 0x800 Exheader + AccessDesc
            br.BaseStream.Seek(0x10, SeekOrigin.Current);
            long ExeFSOffset = br.ReadUInt32() * 0x200;
            long ExeFSSize = br.ReadUInt32() * 0x200;
            br.BaseStream.Seek(8, SeekOrigin.Current);
            long RomFSOffset = br.ReadUInt32() * 0x200;
            long RomFSSize = br.ReadUInt32() * 0x200;
            #endregion

            if (UsesCrypto)
            {
                SetMode(AesMode.CTR);
                if (ExheaderLen > 0)
                {
                    ncchInStream.Seek(StartOffset + 0x200, SeekOrigin.Begin);
                    ncchOutStream.Seek(StartOffset + 0x200, SeekOrigin.Begin);
                    if (UsesFixedKey)
                        SelectKeyslot(0x11);
                    else
                    {
                        SelectKeyslot(0x2C);
                        SetKeyY(NCCHKeyY);
                    }
                    SetCTR(PartitionID, (ulong)(1) << 56);
                    Decrypt(ncchInStream, ncchOutStream, 0x800);
                }
                if (ExeFSSize > 0)
                {
                    ncchInStream.Seek(StartOffset + ExeFSOffset, SeekOrigin.Begin);
                    ncchOutStream.Seek(StartOffset + ExeFSOffset, SeekOrigin.Begin);
                    if (UsesFixedKey)
                        SelectKeyslot(0x11);
                    else
                    {
                        SelectKeyslot(0x2C);
                        SetKeyY(NCCHKeyY);
                    }
                    SetCTR(PartitionID, (ulong)(2) << 56);
                    Decrypt(ncchInStream, ncchOutStream, 0x200);
                    ncchOutStream.Seek(-0x200, SeekOrigin.Current);
                    byte[] ExeFSMeta = new byte[0x200];
                    ncchOutStream.Read(ExeFSMeta, 0, ExeFSMeta.Length);
                    for (int i = 0; i < 10; i++)
                    {
                        string file_name = Encoding.ASCII.GetString(ExeFSMeta, i * 0x10, 8).TrimEnd((char)0);
                        uint file_ofs = BitConverter.ToUInt32(ExeFSMeta, i * 0x10 + 8) + 0x200;
                        uint file_size = BitConverter.ToUInt32(ExeFSMeta, i * 0x10 + 0xC);
                        if (file_size % 0x200 != 0)
                            file_size += (0x200 - (file_size % 0x200));
                        if (file_size == 0 || file_ofs + file_size > ExeFSSize)
                            continue;
                        ncchInStream.Seek(StartOffset + ExeFSOffset + file_ofs, SeekOrigin.Begin);
                        ncchOutStream.Seek(StartOffset + ExeFSOffset + file_ofs, SeekOrigin.Begin);
                        if (UsesFixedKey)
                            SelectKeyslot(0x11);
                        else
                        {
                            if (file_name == "icon" || file_name == "banner")
                            {
                                SelectKeyslot(0x2C);
                                SetKeyY(NCCHKeyY);
                            }
                            else
                            {
                                switch (CryptoMethod)
                                {
                                    case 1:
                                        SelectKeyslot(0x25);
                                        break;
                                    case 0xA:
                                        SelectKeyslot(0x18);
                                        break;
                                    case 0xB:
                                        SelectKeyslot(0x1B);
                                        break;
                                    default:
                                        SelectKeyslot(0x2C);
                                        break;
                                }
                                SetKeyY(keyY);
                            }
                        }
                        SetCTR(PartitionID, ((ulong)(2) << 56) + file_ofs / 0x10);
                        Decrypt(ncchInStream, ncchOutStream, file_size);
                    }
                }
                if (RomFSSize > 0)
                {
                    ncchInStream.Seek(StartOffset + RomFSOffset, SeekOrigin.Begin);
                    ncchOutStream.Seek(StartOffset + RomFSOffset, SeekOrigin.Begin);
                    if (UsesFixedKey)
                        SelectKeyslot(0x11);
                    else
                    {
                        switch (CryptoMethod)
                        {
                            case 1:
                                SelectKeyslot(0x25);
                                break;
                            case 0xA:
                                SelectKeyslot(0x18);
                                break;
                            case 0xB:
                                SelectKeyslot(0x1B);
                                break;
                            default:
                                SelectKeyslot(0x2C);
                                break;
                        }
                        SetKeyY(keyY);
                    }
                    SetCTR(PartitionID, (ulong)(3) << 56);
                    Decrypt(ncchInStream, ncchOutStream, RomFSSize);
                }
            }
            ncchOutStream.Seek(StartOffset + 0x188 + 3, SeekOrigin.Begin);
            ncchOutStream.WriteByte(0);
            ncchOutStream.Seek(3, SeekOrigin.Current);
            ncchOutStream.WriteByte((byte)((BitFlags & ((0x1 | 0x20) ^ 0xFF)) | 0x4));

            ncchInStream.Seek(StartOffset, SeekOrigin.Begin);
            ncchOutStream.Seek(StartOffset, SeekOrigin.Begin);
            // And we're done.
        }

        List<byte[]> cia_common_keyYs = new List<byte[]> {
            new byte[] {0xD0, 0x7B, 0x33, 0x7F, 0x9C, 0xA4, 0x38, 0x59, 0x32, 0xA2, 0xE2, 0x57, 0x23, 0x23, 0x2E, 0xB9} , // 0 - eShop Titles
            new byte[] {0x0C, 0x76, 0x72, 0x30, 0xF0, 0x99, 0x8F, 0x1C, 0x46, 0x82, 0x82, 0x02, 0xFA, 0xAC, 0xBE, 0x4C} , // 1 - System Titles
            new byte[] {0xC4, 0x75, 0xCB, 0x3A, 0xB8, 0xC7, 0x88, 0xBB, 0x57, 0x5E, 0x12, 0xA1, 0x09, 0x07, 0xB8, 0xA4} , // 2
            new byte[] {0xE4, 0x86, 0xEE, 0xE3, 0xD0, 0xC0, 0x9C, 0x90, 0x2F, 0x66, 0x86, 0xD4, 0xC0, 0x6F, 0x64, 0x9F} , // 3
            new byte[] {0xED, 0x31, 0xBA, 0x9C, 0x04, 0xB0, 0x67, 0x50, 0x6C, 0x44, 0x97, 0xA3, 0x5B, 0x78, 0x04, 0xFC} , // 4
            new byte[] {0x5E, 0x66, 0x99, 0x8A, 0xB4, 0xE8, 0x93, 0x16, 0x06, 0x85, 0x0F, 0xD7, 0xA1, 0x6D, 0xD7, 0x55} , // 5
        };

        public class chunkRecord
        {
            public byte[] index;
            public ushort type;
            public ulong size;
        }

        //CIA decryption algo by onepiecefreak
        public void DecryptCIA(Stream ciaInStream, Stream ciaOutstream)
        {
            List<chunkRecord> chunkRecords = new List<chunkRecord>();

            using (var br = new BinaryReaderX(ciaInStream, true))
            {
                //get Header Information
                var archiveHeaderSize = br.ReadUInt32();
                br.ReadInt32();
                var certChainSize = br.ReadUInt32();
                var ticketSize = br.ReadUInt32();
                var tmdSize = br.ReadUInt32();
                var metaSize = br.ReadUInt32();
                var contSize = br.ReadUInt64();
                byte[] contentIndex = br.ReadBytes(0x2000);

                //get Ticket
                var ticketOffset = ((archiveHeaderSize + 0x3f & ~0x3f) + certChainSize) + 0x3f & ~0x3f;
                br.BaseStream.Position = ticketOffset;
                byte[] ticket = br.ReadBytes((int)ticketSize);

                //get encrypted TitleKey
                byte[] encTitleKey;
                byte[] titleID;
                ulong titleIDI;
                byte keyYIndex = 0;
                using (var tk = new BinaryReaderX(new MemoryStream(ticket), ByteOrder.BigEndian))
                {
                    var sigType = tk.ReadUInt32();
                    int sigSize = 0;
                    int padSize = 0;
                    switch (sigType)
                    {
                        case 0x010003:
                            sigSize = 0x200;
                            padSize = 0x3c;
                            break;
                        case 0x010004:
                            sigSize = 0x100;
                            padSize = 0x3c;
                            break;
                        case 0x010005:
                            sigSize = 0x3c;
                            padSize = 0x40;
                            break;
                    }
                    tk.BaseStream.Position += sigSize + padSize + 0x7f;
                    encTitleKey = tk.ReadBytes(0x10);
                    tk.BaseStream.Position += 0x0D;
                    titleID = tk.ReadBytes(0x8);
                    titleIDI = titleID.BytesToStruct<ulong>();
                    tk.BaseStream.Position += 0xd;
                    keyYIndex = tk.ReadByte();
                }

                //decrypt TitleKey
                SetMode(AesMode.CBC);
                SelectKeyslot(0x3D);
                SetNormalKey(KeyScrambler.GetNormalKey(GetKeyX(0x3D), cia_common_keyYs[keyYIndex]));
                var iv = new byte[0x10];
                titleID.CopyTo(iv, 0);
                SetIV(iv);
                var decTitleKey = Decrypt(encTitleKey);

                //get TMD
                br.BaseStream.Position = br.BaseStream.Position + 0x3f & ~0x3f;
                var tmdOffset = br.BaseStream.Position;
                byte[] TMD = br.ReadBytes((int)tmdSize);

                //get TMD contentIndeces
                uint contChunkOffset = 0;
                using (var tmd = new BinaryReaderX(new MemoryStream(TMD), ByteOrder.BigEndian))
                {
                    var sigType = tmd.ReadUInt32();
                    int sigSize = 0;
                    int padSize = 0;
                    switch (sigType)
                    {
                        case 0x010003:
                            sigSize = 0x200;
                            padSize = 0x3c;
                            break;
                        case 0x010004:
                            sigSize = 0x100;
                            padSize = 0x3c;
                            break;
                        case 0x010005:
                            sigSize = 0x3c;
                            padSize = 0x40;
                            break;
                    }
                    //Header
                    tmd.BaseStream.Position += sigSize + padSize;
                    tmd.BaseStream.Position += 0x9e;
                    var contCount = tmd.ReadUInt16();
                    tmd.BaseStream.Position += 0x24;

                    tmd.BaseStream.Position += 64 * 0x24;   //Content Info Records

                    contChunkOffset = (uint)tmd.BaseStream.Position + (uint)tmdOffset;
                    for (int i = 0; i < contCount; i++)
                    {
                        tmd.BaseStream.Position += 4;
                        chunkRecords.Add(new chunkRecord
                        {
                            index = tmd.ReadBytes(2),
                            type = tmd.ReadUInt16(),
                            size = tmd.ReadUInt64()
                        });
                        tmd.BaseStream.Position += 0x20;
                    }
                }

                //Ignore MetaData
                br.BaseStream.Position = br.BaseStream.Position + 0x3f & ~0x3f;
                if (metaSize != 0) br.BaseStream.Position += metaSize;
                br.BaseStream.Position = br.BaseStream.Position + 0x3f & ~0x3f;

                //Decrypt CIA Contents with decrypted TitleKey
                LoadContentLockSeeds();
                SetMode(AesMode.CBC);
                SetNormalKey(decTitleKey);
                iv = new byte[0x10];
                SetIV(iv);
                var contOffset = br.BaseStream.Position;
                var decContent = Decrypt(br.ReadBytes((int)contSize));
                File.OpenWrite("C:\\Users\\Kirito\\Desktop\\output_0.bin").Write(decContent, 0, decContent.Length);

                for (int i = 0; i < chunkRecords.Count; i++)
                {
                    using (var br2 = new BinaryReaderX(new MemoryStream(decContent), true))
                    {
                        //Decrypt NCCH
                        var ncchContent = new MemoryStream(br2.ReadBytes((int)chunkRecords[i].size));
                        var decNcchContent = new MemoryStream();
                        ncchContent.CopyTo(decNcchContent);
                        decNcchContent.Position = ncchContent.Position = 0;
                        var seed = (contLockSeeds.ContainsKey(titleIDI)) ? contLockSeeds[titleIDI] : null;
                        DecryptNCCH(ncchContent, decNcchContent, seed);

                        //Write decrypted NCCH
                        using (var bw = new BinaryWriterX(ciaOutstream, true, ByteOrder.BigEndian))
                        {
                            //Set contentType to Encrypted=0
                            chunkRecords[i].type &= 0xfffe;
                            bw.BaseStream.Position = contChunkOffset + i * 0x30 + 6;
                            bw.Write(chunkRecords[i].type);

                            //Write decrypted Content
                            bw.BaseStream.Position = contOffset;
                            decNcchContent.CopyTo(bw.BaseStream);
                            contOffset += (int)chunkRecords[i].size;
                            contOffset = contOffset + 0x3f & ~0x3f;
                        }

                        br2.BaseStream.Position = br2.BaseStream.Position + 0x3f & ~0x3f;
                    }
                }

                decContent = null;
            }
        }

        public void DecryptNAND(Stream nandInStream, Stream nandOutStream, bool isNew3DS)
        {
            if (OTP == null)
                return;

            // TODO
        }

        public byte[] DecryptBOSS(byte[] boss)
        {
            var ctr = new byte[0x10];
            Array.Copy(boss, 0x1C, ctr, 0x0, 0xC);
            ctr[0xF] = 0x1;

            SelectKeyslot(0x38);
            SetMode(AesMode.CTR);
            SetCTR(ctr);

            var encdata = new byte[boss.Length - 0x28];
            Array.Copy(boss, 0x28, encdata, 0, encdata.Length);
            var decdata = Decrypt(encdata);

            var decboss = new byte[boss.Length];
            Array.Copy(boss, decboss, 0x28);
            decdata.CopyTo(decboss, 0x28);

            return decboss;
        }


        private static byte[] GetExtSeed(ulong titleid)
        {
            return GetExtSeed(titleid, "JP") ?? GetExtSeed(titleid, "US") ?? GetExtSeed(titleid, "GB") ?? GetExtSeed(titleid, "KR") ?? GetExtSeed(titleid, "TW") ?? GetExtSeed(titleid, "AU") ?? GetExtSeed(titleid, "NZ");
        }

        private static byte[] GetExtSeed(ulong titleid, string country)
        {
            try
            {
                using (var wc = new WebClient())
                    return wc.DownloadData($"https://kagiya-ctr.cdn.nintendo.net/title/0x{titleid.ToString("X16")}/ext_key?country={country}");
            }
            catch (WebException wex)
            { Console.WriteLine($"https://kagiya-ctr.cdn.nintendo.net/title/0x{titleid.ToString("X16")}/ext_key?country={country}"); return null; }
        }

    }
    public static class StringExtentions
    {
        public static byte[] ToByteArray(this string toTransform)
        {
            return Enumerable
                .Range(0, toTransform.Length / 2)
                .Select(i => Convert.ToByte(toTransform.Substring(i * 2, 2), 16))
                .ToArray();
        }
    }

    public static class ByteArrayExtensions
    {
        public static string ToHexString(this byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:X2}", b);
            return hex.ToString();
        }
    }
}
