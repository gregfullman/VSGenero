using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VSGenero.Analysis.Parsing.Schema
{
    public class SchemaParser
    {
        public string SchemaFilename { get; private set; }

        private Dictionary<string, SchemaTable> _schema = new Dictionary<string, SchemaTable>();

        public Dictionary<string, SchemaTable> Schema
        {
            get
            {
                if (_schema == null)
                    _schema = new Dictionary<string, SchemaTable>();
                return _schema;
            }
        }

        public SchemaParser(string schemaFilename)
        {
            SchemaFilename = schemaFilename;
        }

        public void Parse()
        {
            using (var filestream = new FileStream(SchemaFilename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using(var sr = new StreamReader(filestream, Encoding.UTF8, true, 4096))
            {
                string line;
                while((line = sr.ReadLine()) != null)
                {
                    var fields = line.Split('^');
                    SchemaTable table;
                    if(!_schema.TryGetValue(fields[0], out table))
                    {
                        table = new SchemaTable(fields[0]);
                        _schema.Add(fields[0], table);
                    }
                    var schemaColumn = new SchemaColumn(fields[1], int.Parse(fields[2]), int.Parse(fields[3]), int.Parse(fields[4]));
                    table.Columns.Add(schemaColumn.Name, schemaColumn);
                }
            }
        }
    }

    public class SchemaTable
    {
        public string Name { get; private set; }

        private Dictionary<string, SchemaColumn> _columns = new Dictionary<string, SchemaColumn>();
        public Dictionary<string, SchemaColumn> Columns
        {
            get { return _columns; }
        }

        public SchemaTable(string name)
        {
            Name = name;
        }
    }

    public class SchemaColumn
    {
        public string Name { get; private set; }
        public SchemaColumnType ColumnType { get; private set; }
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

        public SchemaColumn(string name, int dataType, int length, int ordinalPosition)
        {
            Name = name;
            Init(dataType, length, ordinalPosition);
        }

        private void Init(int dataType, int length, int ordinalPosition)
        {
            TablePosition = ordinalPosition;
            IsNullable = true;
            if(dataType >= 256)
            {
                // non-nullable
                IsNullable = false;
                dataType -= 256;
            }
            switch(dataType)
            {
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
                    if(length > 0)
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
    }

    public enum SchemaColumnType
    {
        Char,
        Smallint,
        Integer,
        Float,
        Smallfloat,
        Decimal,
        Serial,
        Date,
        Money,
        Datetime,
        Byte,
        Text,
        Varchar,
        Interval,
        NChar,
        NVarchar,
        Int8,
        Serial8,
        Boolean,
        Bigint,
        Bigserial,
        Varchar2,
        NVarchar2
    }

    public enum DtiQualifier
    {
        Year = 0,
        Month = 2,
        Day = 4,
        Hour = 6,
        Minute = 8,
        Second = 10,
        Fraction1 = 11,
        Fraction2 = 12,
        Fraction3 = 13,
        Fraction4 = 14,
        Fraction5 = 15
    }
}
