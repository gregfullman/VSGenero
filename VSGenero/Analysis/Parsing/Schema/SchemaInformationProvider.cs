using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading;
using VSGenero.Analysis.Interfaces;

namespace VSGenero.Analysis.Parsing.Schema
{
    [Export(typeof(IDatabaseInformationProvider))]
    internal class SchemaInformationProvider : IDatabaseInformationProvider
    {
        [Import(AllowDefault = true)]
        internal ISchemaFileProvider _SchemaFileProvider;

        private bool _eventInitialized = false;

        private void _SchemaFileProvider_SchemaFileChanged(object sender, SchemaFileChangedEventArgs e)
        {
            if(!string.IsNullOrWhiteSpace(e.CurrentFilename) &&
               e.CurrentFilename != e.NewFilename)
            {
                // remove the current filename if exists
                SchemaParser removed;
                _schemaFileParsers.TryRemove(e.CurrentFilename, out removed);
            }

            ParseSchemaFile(e.NewFilename);
        }

        private object _currentParserLock = new object();
        private SchemaParser _currentParser = null;
        internal SchemaParser CurrentParser
        {
            get
            {
                return _currentParser;
            }
            set
            {
                lock(_currentParserLock)
                {
                    _currentParser = value;
                }
            }
        }

        private ConcurrentDictionary<string, SchemaParser> _schemaFileParsers = new ConcurrentDictionary<string, SchemaParser>(StringComparer.OrdinalIgnoreCase);
        private ConcurrentDictionary<string, string> _filenameToPath = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public IAnalysisResult GetColumn(string tableName, string columnName)
        {
            lock (_currentParserLock)
            {
                if (CurrentParser != null)
                {
                    SchemaTable result;
                    if (CurrentParser.Schema.TryGetValue(tableName, out result))
                    {
                        SchemaColumn column;
                        if (result.Columns.TryGetValue(columnName, out column))
                        {
                            return column;
                        }
                    }
                }
            }
            return null;
        }

        public IEnumerable<IAnalysisResult> GetColumns(string tableName)
        {
            lock (_currentParserLock)
            {
                if (CurrentParser != null)
                {
                    SchemaTable result;
                    if (CurrentParser.Schema.TryGetValue(tableName.Replace(".*", string.Empty), out result))
                    {
                        return result.Columns.Values;
                    }
                }
            }
            return new List<IAnalysisResult>();
        }

        public string GetColumnType(string tableName, string columnName)
        {
            var column = GetColumn(tableName, columnName) as SchemaColumn;
            if (column != null)
                return column.Type;
            return null;
        }

        public IAnalysisResult GetTable(string tablename)
        {
            lock (_currentParserLock)
            {
                SchemaTable result = null;
                if (CurrentParser != null)
                    CurrentParser.Schema.TryGetValue(tablename, out result);
                return result;
            }
        }

        public IEnumerable<IDbTableResult> GetTables()
        {
            lock (_currentParserLock)
            {
                if (CurrentParser != null)
                {
                    return CurrentParser.Schema.Values;
                }
            }
            return new List<IDbTableResult>();
        }

        public void SetFilename(string filename)
        {
            lock (_parseLock)
            {
                if (!_eventInitialized && _SchemaFileProvider != null)
                {
                    _SchemaFileProvider.SchemaFileChanged += _SchemaFileProvider_SchemaFileChanged;
                    _eventInitialized = true;
                }
            }

            if (_SchemaFileProvider != null)
            {
                var schemaFilename = _SchemaFileProvider.GetSchemaFilename(filename);
                ParseSchemaFile(schemaFilename);
            }
        }

        private object _parseLock = new object();

        private void ParseSchemaFile(string schemaFilename)
        {
            if (!string.IsNullOrWhiteSpace(schemaFilename) && File.Exists(schemaFilename))
            {
                // Make sure the following logic is only run once at any time
                lock (_parseLock)
                {
                    // Check for an existing schema file entry under a different path.
                    string currentPath;
                    if(!_filenameToPath.TryGetValue(Path.GetFileName(schemaFilename), out currentPath))
                        currentPath = schemaFilename;
                
                    SchemaParser parser;
                    if (_schemaFileParsers.TryGetValue(currentPath, out parser))
                    {
                        if (currentPath != schemaFilename)
                        {
                            _schemaFileParsers.TryRemove(currentPath, out parser);
                            QueueParserWorker(schemaFilename);
                        }
                        else
                        {
                            CurrentParser = parser;
                        }
                    }
                    else
                    {
                        QueueParserWorker(schemaFilename);
                    }
                }
            }
        }

        private void QueueParserWorker(string schemaFilename)
        {
            CurrentParser = null;
            _schemaFileParsers.AddOrUpdate(schemaFilename, CurrentParser, (x, y) => null);
            _filenameToPath.AddOrUpdate(Path.GetFileName(schemaFilename), schemaFilename, (x, y) => schemaFilename);
            // Start a new task that parses the schema file and sets the current parser (and adds it to the dictionary)
            ThreadPool.QueueUserWorkItem((x) =>
            {
                var newParser = new SchemaParser(schemaFilename);
                newParser.Parse();
                _schemaFileParsers.AddOrUpdate(schemaFilename, newParser, (z, y) => newParser);
                CurrentParser = newParser;
            });
        }
    }
}
