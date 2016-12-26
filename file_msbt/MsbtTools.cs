using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;

namespace file_msbt
{
	class MsbtTools
	{
		// Tools
		//public string ExportToCSV(string filename)
		//{
		//    string result = "Successfully exported to CSV.";

		//    try
		//    {
		//        StringBuilder sb = new StringBuilder();

		//        List<string> row = new List<string>();

		//        IEntry ent = null;
		//        if (HasLabels)
		//        {
		//            sb.AppendLine("Label,String");
		//            for (int i = 0; i < TXT2.NumberOfStrings; i++)
		//            {
		//                ent = LBL1.Labels[i];
		//                row.Add(ent.ToString());
		//                row.Add("\"" + ent.ToString(FileEncoding).Replace("\"", "\"\"") + "\"");
		//                sb.AppendLine(string.Join(",", row.ToArray()));
		//                row.Clear();
		//            }
		//        }
		//        else
		//        {
		//            sb.AppendLine("Index,String");
		//            for (int i = 0; i < TXT2.NumberOfStrings; i++)
		//            {
		//                ent = TXT2.Strings[i];
		//                row.Add((ent.Index + 1).ToString());
		//                row.Add("\"" + ent.ToString(FileEncoding).Replace("\"", "\"\"") + "\"");
		//                sb.AppendLine(string.Join(",", row.ToArray()));
		//                row.Clear();
		//            }
		//        }

		//        FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None);
		//        BinaryWriter bw = new BinaryWriter(fs);
		//        bw.Write(new byte[] { 0xEF, 0xBB, 0xBF });
		//        bw.Write(sb.ToString().ToCharArray());
		//        bw.Close();
		//    }
		//    catch (IOException ioex)
		//    {
		//        result = "File Access Error - " + ioex.Message;
		//    }
		//    catch (Exception ex)
		//    {
		//        result = "Unknown Error - " + ex.Message;
		//    }

		//    return result;
		//}

		//public string ExportXMSBT(string filename, bool overwrite = true)
		//{
		//    string result = "Successfully exported to XMSBT.";

		//    try
		//    {
		//        if (!File.Exists(filename) || (File.Exists(filename) && overwrite))
		//        {
		//            if (HasLabels)
		//            {
		//                XmlDocument xmlDocument = new XmlDocument();

		//                XmlWriterSettings xmlSettings = new XmlWriterSettings();
		//                xmlSettings.Encoding = FileEncoding;
		//                xmlSettings.Indent = true;
		//                xmlSettings.IndentChars = "\t";
		//                xmlSettings.CheckCharacters = false;

		//                XmlDeclaration xmlDeclaration = xmlDocument.CreateXmlDeclaration("1.0", FileEncoding.WebName, null);
		//                xmlDocument.AppendChild(xmlDeclaration);

		//                XmlElement xmlRoot = xmlDocument.CreateElement("xmsbt");
		//                xmlDocument.AppendChild(xmlRoot);

		//                foreach (Label lbl in LBL1.Labels)
		//                {
		//                    XmlElement xmlEntry = xmlDocument.CreateElement("entry");
		//                    xmlRoot.AppendChild(xmlEntry);

		//                    XmlAttribute xmlLabelAttribute = xmlDocument.CreateAttribute("label");
		//                    xmlLabelAttribute.Value = lbl.Name;
		//                    xmlEntry.Attributes.Append(xmlLabelAttribute);

		//                    XmlElement xmlString = xmlDocument.CreateElement("text");
		//                    xmlString.InnerText = FileEncoding.GetString(lbl.String.Text).Replace("\n", "\r\n").TrimEnd('\0').Replace("\0", @"\0");
		//                    xmlEntry.AppendChild(xmlString);
		//                }

		//                StreamWriter stream = new StreamWriter(filename, false, FileEncoding);
		//                xmlDocument.Save(XmlWriter.Create(stream, xmlSettings));
		//                stream.Close();
		//            }
		//            else
		//            {
		//                result = "XMSBT does not currently support files without an LBL1 section.";
		//            }
		//        }
		//        else
		//        {
		//            result = filename + " already exists and overwrite was set to false.";
		//        }
		//    }
		//    catch (Exception ex)
		//    {
		//        result = "Unknown Error - " + ex.Message;
		//    }

		//    return result;
		//}

		//public string ImportXMSBT(string filename, bool addNew = false)
		//{
		//    string result = "Successfully imported from XMSBT.";

		//    try
		//    {
		//        if (HasLabels)
		//        {
		//            XmlDocument xmlDocument = new XmlDocument();
		//            xmlDocument.Load(filename);

		//            XmlNode xmlRoot = xmlDocument.SelectSingleNode("/xmsbt");

		//            Dictionary<string, Label> labels = new Dictionary<string, Label>();
		//            foreach (Label lbl in LBL1.Labels)
		//                labels.Add(lbl.Name, lbl);

		//            foreach (XmlNode entry in xmlRoot.SelectNodes("entry"))
		//            {
		//                string label = entry.Attributes["label"].Value;
		//                byte[] str = FileEncoding.GetBytes(entry.SelectSingleNode("text").InnerText.Replace("\r\n", "\n").Replace(@"\0", "\0") + "\0");

		//                if (labels.ContainsKey(label))
		//                    labels[label].String.Value = str;
		//                else if (addNew)
		//                {
		//                    if (label.Trim().Length <= MSBT.LabelMaxLength && Regex.IsMatch(label.Trim(), MSBT.LabelFilter))
		//                    {
		//                        Label lbl = AddEntry(label);
		//                        lbl.String.Value = str;
		//                    }
		//                }
		//            }
		//        }
		//        else
		//        {
		//            result = "XMSBT does not currently support files without an LBL1 section.";
		//        }
		//    }
		//    catch (Exception ex)
		//    {
		//        result = "Unknown Error - " + ex.Message;
		//    }

		//    return result;
		//}

		//public string ExportXMSBTMod(string outFilename, string sourceFilename, bool overwrite = true)
		//{
		//    string result = "Successfully exported to XMSBT.";

		//    try
		//    {
		//        if (!File.Exists(outFilename) || (File.Exists(outFilename) && overwrite))
		//        {
		//            if (HasLabels)
		//            {
		//                XmlDocument xmlDocument = new XmlDocument();

		//                XmlWriterSettings xmlSettings = new XmlWriterSettings();
		//                xmlSettings.Encoding = FileEncoding;
		//                xmlSettings.Indent = true;
		//                xmlSettings.IndentChars = "\t";
		//                xmlSettings.CheckCharacters = false;

		//                XmlDeclaration xmlDeclaration = xmlDocument.CreateXmlDeclaration("1.0", FileEncoding.WebName, null);
		//                xmlDocument.AppendChild(xmlDeclaration);

		//                XmlElement xmlRoot = xmlDocument.CreateElement("xmsbt");
		//                xmlDocument.AppendChild(xmlRoot);

		//                MSBT source = new MSBT(sourceFilename);

		//                foreach (Label lbl in LBL1.Labels)
		//                {
		//                    bool export = true;

		//                    foreach (Label lblSource in source.LBL1.Labels)
		//                        if (lbl.Name == lblSource.Name && lbl.String.Text.SequenceEqual(lblSource.String.Text))
		//                        {
		//                            export = false;
		//                            break;
		//                        }

		//                    if (export)
		//                    {
		//                        XmlElement xmlEntry = xmlDocument.CreateElement("entry");
		//                        xmlRoot.AppendChild(xmlEntry);

		//                        XmlAttribute xmlLabelAttribute = xmlDocument.CreateAttribute("label");
		//                        xmlLabelAttribute.Value = lbl.Name;
		//                        xmlEntry.Attributes.Append(xmlLabelAttribute);

		//                        XmlElement xmlString = xmlDocument.CreateElement("text");
		//                        xmlString.InnerText = FileEncoding.GetString(lbl.String.Text).Replace("\n", "\r\n").TrimEnd('\0').Replace("\0", @"\0");
		//                        xmlEntry.AppendChild(xmlString);
		//                    }
		//                }

		//                StreamWriter stream = new StreamWriter(outFilename, false, FileEncoding);
		//                xmlDocument.Save(XmlWriter.Create(stream, xmlSettings));
		//                stream.Close();
		//            }
		//            else
		//            {
		//                result = "XMSBT does not currently support files without an LBL1 section.";
		//            }
		//        }
		//        else
		//        {
		//            result = outFilename + " already exists and overwrite was set to false.";
		//        }
		//    }
		//    catch (Exception ex)
		//    {
		//        result = "Unknown Error - " + ex.Message;
		//    }

		//    return result;
		//}
	}
}