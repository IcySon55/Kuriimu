using System.Collections.Generic;
using System.IO;
using System.Text;
using Kontract.IO;

namespace text_td3
{
    public class TD3
    {
        public const int HashTableLength = 0x138;

        public Header Header = null;
        public int MethodCount = 0;
        public List<uint> Instructions = new List<uint>();
        public List<Method> Entries = new List<Method>();
        private byte[] Block;

        //private List<string> _stringMethods = new List<string>
        //{
        //    "SetMsgWait",
        //    "SetUpMsg"
        //};

        public TD3(Stream input)
        {
            using (var br = new BinaryReaderX(input, false))
            {
                Header = br.ReadStruct<Header>();
                Instructions = br.ReadMultiple<uint>(Header.InstructionCount);
                MethodCount = br.ReadInt32();

                for (int i = 0; i < MethodCount; i++)
                {
                    var method = ReadMethod(br);
                    method.ID = (i + 1).ToString("000000");
                    Entries.Add(method);
                    //Method subMethod;

                    //switch (method.Content)
                    //{
                    //    case "SetMsgWait":
                    //        i++;
                    //        method.ID += "_" + method.Content;
                    //        subMethod = ReadMethod(br);
                    //        subMethod.ID = (i + 1).ToString("000000");
                    //        method.Parameters.Add(subMethod);
                    //        break;
                    //    case "SetUpMsg":
                    //        i++;
                    //        method.ID += "_" + method.Content;
                    //        subMethod = ReadMethod(br);
                    //        subMethod.ID = (i + 1).ToString("000000");
                    //        method.Parameters.Add(subMethod);
                    //        i++;
                    //        subMethod = ReadMethod(br);
                    //        subMethod.ID = (i + 1).ToString("000000");
                    //        method.Parameters.Add(subMethod);
                    //        break;
                    //}
                }

                Block = br.ReadBytes((int)(br.BaseStream.Length - br.BaseStream.Position));
            }
        }

        private Method ReadMethod(BinaryReaderX br)
        {
            var method = new Method { Type = br.ReadByte() };
            switch (method.Type)
            {
                case 3: // Integer
                    method.Length = br.ReadUInt32();
                    break;
                case 4: // String
                    method.Length = br.ReadUInt32();
                    method.Content = br.ReadString((int)method.Length, Encoding.UTF8);
                    break;
            }
            return method;
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output))
            {
                bw.WriteStruct(Header);
                foreach (var ins in Instructions)
                    bw.Write(ins);
                bw.Write(MethodCount);

                foreach (var method in Entries)
                {
                    bw.Write(method.Type);
                    switch (method.Type)
                    {
                        case 3: // Integer
                            bw.Write(method.Length);
                            break;
                        case 4: // String
                            var bytes = Encoding.UTF8.GetBytes(method.Content);
                            bw.Write(bytes.Length + 1);
                            bw.Write(bytes);
                            bw.Write((byte)0x0);
                            break;
                    }
                }

                bw.Write(Block);
            }
        }
    }
}
