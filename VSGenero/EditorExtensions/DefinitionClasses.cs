using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;

namespace VSGenero.EditorExtensions
{
    public abstract class GeneroLanguageItemDefinition
    {
        public string Name { get; set; }
        public string ContainingFile { get; set; }
        public int Position { get; set; }
        public int LineNumber { get; set; }
        public int ColumnNumber { get; set; }
        public virtual int Length { get; set; }
    }

    public class TempTableDefinition : GeneroLanguageItemDefinition
    {
        private ConcurrentDictionary<string, VariableDefinition> _columns;
        public ConcurrentDictionary<string, VariableDefinition> Columns
        {
            get
            {
                if (_columns == null)
                    _columns = new ConcurrentDictionary<string, VariableDefinition>();
                return _columns;
            }
        }
    }

    public class CursorPreparation : GeneroLanguageItemDefinition
    {
        public string StatementVariable { get; set; }
        public string CursorStatement { get; set; }
    }

    public class CursorDeclaration : GeneroLanguageItemDefinition
    {
        public string PreparationVariable { get; set; }
        public string StaticSqlStatement { get; set; }

        private List<string> _options;
        public List<string> Options
        {
            get
            {
                if (_options == null)
                    _options = new List<string>();
                return _options;
            }
        }
    }

    public class FunctionDefinition : GeneroLanguageItemDefinition
    {
        private Dictionary<string, bool> existingVariablesParsed = new Dictionary<string, bool>();
        public bool Private { get; set; }
        public bool Main { get; set; }
        public bool Report { get; set; }
        public int End { get; set; }

        private string _containingFile;
        public new string ContainingFile
        {
            get { return _containingFile; }
            set
            {
                if (_containingFile != value)
                {
                    _containingFile = value;
                    foreach (var vardef in Variables)
                        vardef.Value.ContainingFile = _containingFile;
                }
            }
        }

        private ConcurrentDictionary<string, VariableDefinition> _variables;
        public ConcurrentDictionary<string, VariableDefinition> Variables
        {
            get
            {
                if (_variables == null)
                    _variables = new ConcurrentDictionary<string, VariableDefinition>();
                return _variables;
            }
        }

        private ConcurrentDictionary<string, ConstantDefinition> _constants;
        public ConcurrentDictionary<string, ConstantDefinition> Constants
        {
            get
            {
                if (_constants == null)
                    _constants = new ConcurrentDictionary<string, ConstantDefinition>();
                return _constants;
            }
        }

        private ConcurrentDictionary<string, TypeDefinition> _types;
        public ConcurrentDictionary<string, TypeDefinition> Types
        {
            get
            {
                if (_types == null)
                    _types = new ConcurrentDictionary<string, TypeDefinition>();
                return _types;
            }
        }

        private List<string> _parameters;
        public List<string> Parameters
        {
            get
            {
                if (_parameters == null)
                    _parameters = new List<string>();
                return _parameters;
            }
        }

        private List<GeneroFunctionReturn> _returns;
        public List<GeneroFunctionReturn> Returns
        {
            get
            {
                if (_returns == null)
                    _returns = new List<GeneroFunctionReturn>();
                return _returns;
            }
        }
    }

    public class ConstantDefinition : VariableDefinition
    {
        public string Value { get; set; }
    }

    public class TypeDefinition : VariableDefinition
    {
    }

    public class VariableDefinition : GeneroLanguageItemDefinition
    {
        public string Type { get; set; }
        public bool IsRecordType { get; set; }
        public bool IsMimicType { get; set; }
        public int StaticArraySize { get; set; }
        public ArrayType ArrayType { get; set; }

        public string MimicTypeTable
        {
            get { return GetMimicPart(0); }
        }

        public string MimicTypeColumn
        {
            get { return GetMimicPart(1); }
        }

        private string GetMimicPart(int index)
        {
            if (IsMimicType)
            {
                string[] parts = Type.Split(new[] { '.' });
                if (parts.Length > 1)
                {
                    return parts[index];
                }
            }
            return null;
        }

        private ConcurrentDictionary<string, VariableDefinition> _recordElements;
        public ConcurrentDictionary<string, VariableDefinition> RecordElements
        {
            get
            {
                if (_recordElements == null)
                    _recordElements = new ConcurrentDictionary<string, VariableDefinition>();
                return _recordElements;
            }
        }

        public VariableDefinition Clone()
        {
            VariableDefinition ret = new VariableDefinition
            {
                Name = this.Name,
                Type = this.Type,
                IsMimicType = this.IsMimicType,
                IsRecordType = this.IsRecordType,
                ArrayType = this.ArrayType,
                Position = this.Position,
                ColumnNumber = this.ColumnNumber,
                LineNumber = this.LineNumber,
                ContainingFile = this.ContainingFile
            };
            foreach (var recEle in RecordElements)
                ret.RecordElements.AddOrUpdate(recEle.Key, recEle.Value.Clone(), (x, y) => recEle.Value.Clone());
            return ret;
        }

        public void CloneContents(VariableDefinition clone)
        {
            if(string.IsNullOrWhiteSpace(Name))
                Name = clone.Name;
            if (string.IsNullOrWhiteSpace(Type))
                Type = clone.Type;
            IsMimicType = clone.IsMimicType;
            IsRecordType = clone.IsRecordType;
            ArrayType = clone.ArrayType;
            if(Position <= 0)
                Position = clone.Position;
            if(ColumnNumber <= 0)
                ColumnNumber = clone.ColumnNumber;
            if (LineNumber <= 0)
                LineNumber = clone.LineNumber;
            if(string.IsNullOrWhiteSpace(ContainingFile))
                ContainingFile = clone.ContainingFile;
            foreach (var recEle in clone.RecordElements)
                RecordElements.AddOrUpdate(recEle.Key, recEle.Value.Clone(), (x, y) => recEle.Value.Clone());
        }
    }
}
