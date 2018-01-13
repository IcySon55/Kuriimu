using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Kontract.IO;

namespace text_gds
{
    public class GDS
    {
        public List<Label> Labels = new List<Label>();

        List<Command> commands = new List<Command>();

        public GDS(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                var size = br.ReadInt32();

                //Read commands
                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    var type = br.ReadInt16();
                    object value = null;
                    switch (type)
                    {
                        case 0:
                            value = br.ReadInt16();
                            break;
                        case 1:
                            value = br.ReadInt32();
                            break;
                        case 3:
                            var stringSize = br.ReadInt16();
                            value = Encoding.UTF8.GetString(br.ReadBytes(stringSize)).TrimEnd('\0');
                            break;
                        case 12:
                            //Script Exitcode
                            break;
                        default:
                            throw new NotImplementedException($"Type {type} not supported.");
                    }

                    commands.Add(new Command
                    {
                        type = type,
                        value = value
                    });
                }

                //Labels
                int textCount = 0;
                foreach (var command in commands)
                    if (command.type == 0x3)
                        Labels.Add(new Label
                        {
                            Name = $"Text{textCount:0000}",
                            TextID = (uint)textCount++,
                            Text = (string)command.value
                        });
            }
        }

        public void Save(string filename)
        {
            //Update Texts
            int labelCount = 0;
            foreach (var command in commands)
                if (command.type == 0x3)
                    command.value = Labels[labelCount++].Text;

            using (var bw = new BinaryWriterX(File.Create(filename)))
            {
                //Write Commands
                bw.BaseStream.Position = 4;
                foreach (var command in commands)
                {
                    bw.Write(command.type);
                    switch (command.type)
                    {
                        case 0:
                            bw.Write((short)command.value);
                            break;
                        case 1:
                            bw.Write((int)command.value);
                            break;
                        case 3:
                            var stringBytes = Encoding.UTF8.GetBytes((string)(command.value) + "\0");
                            bw.Write((short)(stringBytes.Length));
                            bw.Write(stringBytes);
                            break;
                        case 12:
                            //Script Exitcode
                            break;
                        default:
                            throw new NotImplementedException($"Type {command.type} not supported.");
                    }
                }

                //Write fileSize
                bw.BaseStream.Position = 0;
                bw.Write((int)(bw.BaseStream.Length - 4));
            }
        }
    }
}
