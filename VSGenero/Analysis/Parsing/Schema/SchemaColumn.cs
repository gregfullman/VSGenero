using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSGenero.Analysis.Parsing.AST_4GL;

namespace VSGenero.Analysis.Parsing.Schema
{
    public class SchemaColumn : IAnalysisResult
    {
        public string TableName { get; private set; }
        public string Name { get; private set; }
        public SchemaColumnType ColumnType { get; private set; }
        public string UnknownColumnType { get; private set; }
        public bool IsNullable { get; private set; }
        public int? Length { get; private set; }
        public int TablePosition { get; private set; }

        public int? Precision { get; private set; }
        public int? Scale { get; private set; }

        public int? MinSpace { get; private set; }
        public int? MaxSize { get; private set; }

        public int? Digits { get; private set; }
        public DtiQualifier? Qualifier1 { get; private set; }
        public DtiQualifier? Qualifier2 { get; private set; }

        public SchemaColumn(string tableName, string name, string dataType, string length, string ordinalPosition)
        {
            TableName = tableName;
            Name = name;
            int ordPos;
            int.TryParse(ordinalPosition, out ordPos);
            Init(dataType, length, ordPos);
        }

        private void Init(string dataTypeStr, string lengthStr, int ordinalPosition)
        {
            int dataType;
            if(!int.TryParse(dataTypeStr, out dataType))
            {
                dataType = -1;
            }

            int length;
            if(!int.TryParse(lengthStr, out length))
            {
                length = -1;
            }

            TablePosition = ordinalPosition;
            IsNullable = true;
            if (dataType >= 256)
            {
                // non-nullable
                IsNullable = false;
                dataType -= 256;
            }
            switch (dataType)
            {
                case -1:
                    ColumnType = SchemaColumnType.Unknown;
                    UnknownColumnType = dataTypeStr;
                    break;
                case 0:
                    // CHAR
                    ColumnType = SchemaColumnType.Char;
                    Length = length;
                    break;
                case 1:
                    // SMALLINT
                    ColumnType = SchemaColumnType.Smallint;
                    Length = 2;
                    break;
                case 2:
                    // INTEGER
                    ColumnType = SchemaColumnType.Integer;
                    Length = 4;
                    break;
                case 3:
                    // FLOAT
                    ColumnType = SchemaColumnType.Float;
                    Length = 8;
                    break;
                case 4:
                    // SMALLFLOAT
                    ColumnType = SchemaColumnType.Smallfloat;
                    Length = 4;
                    break;
                case 5:
                    // DECIMAL
                    ColumnType = SchemaColumnType.Decimal;
                    Scale = length % 256;
                    Precision = (length - Scale) / 256;
                    if (Scale == 255)
                        Scale = -1;     // Floating point dec
                    break;
                case 6:
                    // SERIAL
                    ColumnType = SchemaColumnType.Serial;
                    Length = 4;
                    break;
                case 7:
                    // DATE
                    ColumnType = SchemaColumnType.Date;
                    Length = 4;
                    break;
                case 8:
                    // MONEY
                    ColumnType = SchemaColumnType.Money;
                    Scale = length % 256;
                    Precision = (length - Scale) / 256;
                    break;
                case 10:
                    // DATETIME
                    ColumnType = SchemaColumnType.Datetime;
                    Digits = length.GetNibble(2);
                    Qualifier1 = (DtiQualifier)length.GetNibble(1);
                    Qualifier2 = (DtiQualifier)length.GetNibble(0);
                    break;
                case 11:
                    // BYTE
                    ColumnType = SchemaColumnType.Byte;
                    break;
                case 12:
                    // TEXT
                    ColumnType = SchemaColumnType.Text;
                    break;
                case 13:
                    // VARCHAR
                    ColumnType = SchemaColumnType.Varchar;
                    if (length > 0)
                    {
                        MaxSize = length % 256;
                        MinSpace = (length - MaxSize) / 256;
                    }
                    else
                    {
                        MaxSize = (length + 65536) % 256;
                        MinSpace = ((length + 65536) - MaxSize) / 256;
                    }
                    break;
                case 14:
                    // INTERVAL
                    ColumnType = SchemaColumnType.Interval;
                    Digits = length.GetNibble(2);
                    Qualifier1 = (DtiQualifier)length.GetNibble(1);
                    Qualifier2 = (DtiQualifier)length.GetNibble(0);
                    break;
                case 15:
                    // NCHAR
                    ColumnType = SchemaColumnType.NChar;
                    Length = length;
                    break;
                case 16:
                    // NVARCHAR
                    ColumnType = SchemaColumnType.NVarchar;
                    if (length > 0)
                    {
                        MaxSize = length % 256;
                        MinSpace = (length - MaxSize) / 256;
                    }
                    else
                    {
                        MaxSize = (length + 65536) % 256;
                        MinSpace = ((length + 65536) - MaxSize) / 256;
                    }
                    break;
                case 17:
                    // INT8
                    ColumnType = SchemaColumnType.Int8;
                    Length = 10;
                    break;
                case 18:
                    // SERIAL8
                    ColumnType = SchemaColumnType.Serial8;
                    Length = 10;
                    break;
                case 45:
                    // BOOLEAN
                    ColumnType = SchemaColumnType.Boolean;
                    break;
                case 52:
                    // BIGINT
                    ColumnType = SchemaColumnType.Bigint;
                    Length = 8;
                    break;
                case 53:
                    // BIGSERIAL
                    ColumnType = SchemaColumnType.Bigserial;
                    Length = 8;
                    break;
                case 201:
                    // VARCHAR2
                    ColumnType = SchemaColumnType.Varchar2;
                    if (length > 0)
                    {
                        MaxSize = length % 256;
                        MinSpace = (length - MaxSize) / 256;
                    }
                    else
                    {
                        MaxSize = (length + 65536) % 256;
                        MinSpace = ((length + 65536) - MaxSize) / 256;
                    }
                    break;
                case 202:
                    // NVARCHAR2
                    ColumnType = SchemaColumnType.NVarchar2;
                    if (length > 0)
                    {
                        MaxSize = length % 256;
                        MinSpace = (length - MaxSize) / 256;
                    }
                    else
                    {
                        MaxSize = (length + 65536) % 256;
                        MinSpace = ((length + 65536) - MaxSize) / 256;
                    }
                    break;
            }
        }

        private string _type;

        public string Type
        {
            get
            {
                if (_type == null)
                {
                    StringBuilder sb = new StringBuilder();
                    var type = typeof(SchemaColumnType);
                    if (ColumnType != SchemaColumnType.Unknown)
                    {
                        var memInfo = type.GetMember(ColumnType.ToString());
                        var attributes = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                        var strRep = ((DescriptionAttribute)attributes[0]).Description;
                        sb.Append(strRep);
                    }
                    switch (ColumnType)
                    {
                        case SchemaColumnType.Unknown:
                            sb.Append(UnknownColumnType);
                            break;
                        case SchemaColumnType.Char:
                        case SchemaColumnType.NChar:
                            if (Length.HasValue)
                                sb.AppendFormat("({0})", Length.Value);
                            break;
                        case SchemaColumnType.Varchar:
                        case SchemaColumnType.NVarchar:
                        case SchemaColumnType.Varchar2:
                        case SchemaColumnType.NVarchar2:
                            if (MaxSize.HasValue)
                                sb.AppendFormat("({0})", MaxSize.Value);
                            break;
                        case SchemaColumnType.Decimal:
                        case SchemaColumnType.Money:
                            if (Precision.HasValue && Scale.HasValue)
                                sb.AppendFormat("({0},{1})", Precision.Value, Scale.Value);
                            break;
                            // TODO: intervals and date/datetime
                    }
                    _type = sb.ToString();
                }
                return _type;
            }
        }

        #region IAnalysisResult implementation
        public string Scope
        {
            get
            {
                return null;
            }
            set
            {
            }
        }

        public string Documentation
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                if (!string.IsNullOrWhiteSpace(Type))
                    sb.AppendFormat("{0} ", Type);
                if (!string.IsNullOrWhiteSpace(TableName))
                    sb.AppendFormat("{0}.", TableName);
                sb.Append(Name);
                return sb.ToString();
            }
        }

        public int LocationIndex
        {
            get { return -1; }
        }

        public LocationInfo Location { get { return null; } }

        public bool CanGetValueFromDebugger
        {
            get { return true; }
        }

        public bool IsPublic { get { return true; } }

        public string Typename
        {
            get { return Type; }
        }

        public GeneroLanguageVersion MinimumLanguageVersion
        {
            get
            {
                return GeneroLanguageVersion.None;
            }
        }

        public GeneroLanguageVersion MaximumLanguageVersion
        {
            get
            {
                return GeneroLanguageVersion.Latest;
            }
        }

        public bool HasChildFunctions(Genero4glAst ast)
        {
            return false;
        }

        public IAnalysisResult GetMember(string name, Genero4glAst ast, out IGeneroProject definingProject, out IProjectEntry projectEntry, bool function)
        {
            definingProject = null;
            projectEntry = null;
            return null;
        }

        public IEnumerable<MemberResult> GetMembers(Genero4glAst ast, MemberType memberType, bool function)
        {
            return new List<MemberResult>();
        }

        #endregion
    }
}
