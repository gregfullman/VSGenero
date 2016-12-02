using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VSGenero.Analysis.Parsing.Schema
{
    public class SchemaParser
    {
        public string SchemaFilename { get; internal set; }

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
            _schema.Clear();
            using (var filestream = new FileStream(SchemaFilename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using(var sr = new StreamReader(filestream, Encoding.UTF8, true, 4096))
            {
                string line;
                while((line = sr.ReadLine()) != null)
                {
                    var fields = line.Split('^');
                    if (fields.Length >= 5)
                    {
                        SchemaTable table;
                        if (!_schema.TryGetValue(fields[0], out table))
                        {
                            table = new SchemaTable(fields[0]);
                            _schema[fields[0]] = table;
                        }
                        var schemaColumn = new SchemaColumn(fields[0], fields[1], fields[2], fields[3], fields[4]);
                        if(!table.Columns.ContainsKey(schemaColumn.Name))
                            table.Columns[schemaColumn.Name] = schemaColumn;
                    }
                }
            }
        }
    }
}
