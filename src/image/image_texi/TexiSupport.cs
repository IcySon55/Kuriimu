using System.Runtime.InteropServices;
using System.Xml.Serialization;
using System.Collections.Generic;
using Kontract.Image.Format;
using Kontract.Interface;

namespace image_texi
{
    public class Support
    {
        public static Dictionary<int, IImageFormat> Format = new Dictionary<int, IImageFormat>
        {
            [10] = new RGBA(8, 8, 8, 8),
            [11] = new RGBA(8, 8, 8)
        };
    }
    public enum Format
    {
        RGBA8888 = 0xa, RGB888
    }

    #region XML
    [XmlRoot]
    public class SERIList
    {
        [XmlArrayItem("Parameter")]
        public List<SERIParameter> Parameters = new List<SERIParameter>();

        public void Add(SERIParameter Parameter)
        {
            Parameters.Add(Parameter);
        }
    }

    #region SERIParameter
    [XmlInclude(typeof(String))]
    [XmlInclude(typeof(Integer))]
    [XmlInclude(typeof(Boolean))]
    [XmlInclude(typeof(Float))]
    [XmlInclude(typeof(StringArray))]
    [XmlInclude(typeof(IntegerArray))]
    [XmlInclude(typeof(BooleanArray))]
    [XmlInclude(typeof(FloatArray))]
    [XmlInclude(typeof(NestedArray))]
    public class SERIParameter
    {
        [XmlAttribute]
        public string Name;
    }

    public class String : SERIParameter
    {
        public string Value;

        public String(string Name, string Value)
        {
            this.Name = Name;
            this.Value = Value;
        }

        public String()
        {
        }
    }

    public class Integer : SERIParameter
    {
        public int Value;

        public Integer(string Name, int Value)
        {
            this.Name = Name;
            this.Value = Value;
        }

        public Integer()
        {
        }
    }

    public class Boolean : SERIParameter
    {
        public bool Value;

        public Boolean(string Name, bool Value)
        {
            this.Name = Name;
            this.Value = Value;
        }

        public Boolean()
        {
        }
    }

    public class Float : SERIParameter
    {
        public float Value;

        public Float(string Name, float Value)
        {
            this.Name = Name;
            this.Value = Value;
        }

        public Float()
        {
        }
    }

    public class StringArray : SERIParameter
    {
        [XmlArrayItem("Value")]
        public string[] Values;

        public StringArray(string Name, string[] Values)
        {
            this.Name = Name;
            this.Values = Values;
        }

        public StringArray()
        {
        }
    }

    public class IntegerArray : SERIParameter
    {
        [XmlArrayItem("Value")]
        public int[] Values;

        public IntegerArray(string Name, int[] Values)
        {
            this.Name = Name;
            this.Values = Values;
        }

        public IntegerArray()
        {
        }
    }

    public class BooleanArray : SERIParameter
    {
        [XmlArrayItem("Value")]
        public bool[] Values;

        public BooleanArray(string Name, bool[] Values)
        {
            this.Name = Name;
            this.Values = Values;
        }

        public BooleanArray()
        {
        }
    }

    public class FloatArray : SERIParameter
    {
        [XmlArrayItem("Value")]
        public float[] Values;

        public FloatArray(string Name, float[] Values)
        {
            this.Name = Name;
            this.Values = Values;
        }

        public FloatArray()
        {
        }
    }

    public class NestedArray : SERIParameter
    {
        [XmlArrayItem("Parameter")]
        public SERIParameter[] Values;

        public NestedArray(string Name, SERIParameter[] Values)
        {
            this.Name = Name;
            this.Values = Values;
        }

        public NestedArray()
        {
        }
    }
    #endregion

    #endregion
}
