//------------------------------------------------------------------------------
// <copyright file="CSSqlClassFile.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;

namespace SQLAutoEnums.Generators
{
    //enum ValueType
    //{
    //    Short,
    //    Byte,
    //    Int
    //}

    class EnumDescriptor
    {
        public string ValueType { get { return "int"; } }
        public string Prefix;
        public string Name;
        public string NameFull
        {
            get
            {
                return Prefix + Name;
            }
        }
        
        public string NameEnum
        {
            get
            {
                if (null == _nameEnum || _nameEnum == string.Empty)
                {
                    _nameEnum = Name + "Enum" + Guid.NewGuid().ToString().Replace("-", string.Empty);
                }
                return _nameEnum;
            }
        }
        private string _nameEnum;

        public List<KeyValuePair<string, int>> Values = new List<KeyValuePair<String,Int32>>();
    }

    class SimpleGenerator
    {

        public string Generate(List<EnumDescriptor> list)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(_strHeader);

            foreach (var data in list)
            {
                sb.AppendFormat(_strEnumHeader, data.NameEnum, data.ValueType);
                foreach (var enval in data.Values)
                {
                    sb.AppendFormat(_strEnumItem, enval.Key, enval.Value);
                }
                sb.Append(_strEnumFooter);

                sb.AppendFormat(_strEnumStructHeader, data.NameFull, data.NameEnum);
                foreach (var enval in data.Values)
                {
                    sb.AppendFormat(_strStructValue, data.NameFull, enval.Key, data.NameEnum);
                    sb.AppendFormat(_strStructNumericValue, data.ValueType, "i", enval.Key, data.NameEnum);
                }
                sb.AppendFormat(_strEnumStructFooter, data.NameFull, data.NameEnum);

            }
            sb.Append(_strFooter);
            
            return sb.ToString();
        }



        private string _strHeader = @"
                                    using System; 
                                    using System.Data.SqlTypes; 
                                    using Microsoft.SqlServer.Server; 
                                    using System.Collections;
                                    using System.Collections.Generic;
                                    namespace SqlAutoEnumsGenerated 
                                    { ";
        private string _strEnumHeader = @"public enum {0} : {1} {{  ";  // 0 = enumname, 1 = basetypename
        private string _strEnumItem = @"{0} = {1}, ";  // 0 = enum item name, 1 = value
        private string _strEnumFooter = @" } ";  // 0 = enumname, 1 = basetypename,  2 = enumvals


        private string _strStructValue = // 0 = structname, 1 = enummember name, 2 - enumname
            @"public static {0} {1} {{ get {{ return new {0}({2}.{1}); }} }} "; 
        private string _strStructNumericValue = // 0 = basetypename, 1 - num name prefix, 2 = enummember name, 3 = enumname
            @"public static {0} {1}{2} {{ get {{ return ({0}){3}.{2}; }} }}";

        private string _strEnumStructHeader = //  0 = structname, 1 = enumname
            @"
    [Serializable]
    [SqlUserDefinedType(Format.UserDefined, IsByteOrdered = true, IsFixedLength = true, MaxByteSize=4)] 
    public struct {0} : INullable,   IBinarySerialize
    {{
        public {0}({1} val)
        {{
            _value = val; _null = false;
        }}

        public {1}? Value
        {{
            get {{ return _value; }}
            set {{ _value = value; _null = (value == null); }} 
        }}
        private {1}? _value;
        private bool _null;
        public override string ToString()
        {{
            return Value == null ? string.Empty : Enum.GetName(typeof({1}), Value);
        }}
        public int? ToInt()
        {{
            return Value == null ? (int?)null : (int)Value.Value;
        }}
  



        [SqlFunction(FillRowMethodName = ""ToListFillRow"", TableDefinition=""ID INT, Name nvarchar(4000)"")]  
        public static IEnumerable ToList() 
        {{ 
            var lst = new List<KeyValuePair<int, string>>(); 
            foreach (var data in Enum.GetValues(typeof({1}))) 
                lst.Add(new KeyValuePair<int, string>((int) data, (({1}) data).ToString())); 
            return lst; 
        }} 
 
        public static void ToListFillRow(Object obj, out int id, out string name) 
        {{
            var data = (KeyValuePair<int, string>)obj;
            id = data.Key;
            name = data.Value; 
        }} 








        ";

        private string _strEnumStructFooter =  //  0 = structname, 1 = enumname
@"
        public static bool operator ==({0} a, {0} b) {{ return a.Value == b.Value; }}
        public static bool operator !=({0} a, {0} b) {{ return a.Value != b.Value; }}
        public bool IsNull {{ get {{ return _null; }} }}
        public static {0} Null {{ get {{ var h = new {0} {{ _null = true }}; return h; }} }}

        [SqlFunction(IsDeterministic = true)]
        public static {0} Parse(SqlString s)
        {{
            if (s.IsNull) return Null;
            var u = new {0}
                {{ Value = ({1})Enum.Parse(typeof({1}), s.ToString()) }};
            return u;
        }}

        public void Write(System.IO.BinaryWriter w) 
        {{ 
            if (this.IsNull) 
            {{ 
                w.Write((int)-1); 
                return; 
            }} 
            w.Write((int)_value); 
        }} 
		public void Read(System.IO.BinaryReader r) 
        {{ 
            int data = r.ReadInt32(); 
            if (data == -1) this._null = true; 
			else   this.Value = ({1})data; 
        }} 
    }} 
";

        private string _strFooter = @"}";


    }
}
