using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSGenero.Analysis.Parsing.AST_4GL;

namespace VSGenero.Analysis.Parsing.Schema
{
    public class SchemaTable : IDbTableResult
    {
        private readonly DatabaseTableType _type = DatabaseTableType.Table; // TODO: needs to go away

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

        #region IDbTableResult implementation

        public DatabaseTableType TableType
        {
            get { return _type; }
        }

        public string Scope
        {
            get
            {
                return string.Format("(database {0})", (_type == DatabaseTableType.Table ? "table" : "view"));
            }
            set
            {
            }
        }

        public string Documentation
        {
            get
            {
                return string.Format("{0} {1}", Scope, Name);
            }
        }

        public int LocationIndex
        {
            get { return -1; }
        }

        public LocationInfo Location { get { return null; } }

        public bool CanGetValueFromDebugger
        {
            get { return false; }
        }

        public bool IsPublic { get { return true; } }

        public string Typename
        {
            get { return null; }
        }

        public bool HasChildFunctions(Genero4glAst ast)
        {
            return false;
        }

        public IAnalysisResult GetMember(string name, Genero4glAst ast, out IGeneroProject definingProject, out IProjectEntry projectEntry, bool function)
        {
            definingProject = null;
            projectEntry = null;
            SchemaColumn column = null;
            _columns.TryGetValue(name, out column);
            return column;
        }

        public IEnumerable<MemberResult> GetMembers(Genero4glAst ast, MemberType memberType, bool function)
        {
            List<MemberResult> dot = new List<MemberResult> { new MemberResult("*", GeneroMemberType.DbColumn, ast) };
            return dot.Union(_columns.Values.Select(x => new MemberResult(x.Name, x, GeneroMemberType.DbColumn, ast)));
        }

        #endregion
    }
}
