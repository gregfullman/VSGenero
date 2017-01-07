using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing
{
    public abstract class GeneroAst : ILocationResolver
    {
        internal IFunctionInformationProvider _functionProvider;
        internal IDatabaseInformationProvider _databaseProvider;
        internal IProgramFileProvider _programFileProvider;

        protected readonly GeneroLanguageVersion _langVersion;
        internal readonly int[] _lineLocations;
        protected readonly IProjectEntry _projEntry;
        protected readonly string _filename;
        protected readonly AstNode _body;

        protected readonly Dictionary<AstNode, Dictionary<object, object>> _attributes = new Dictionary<AstNode, Dictionary<object, object>>();

        protected GeneroAst(AstNode body,
                            int[] lineLocations,
                            GeneroLanguageVersion langVersion = GeneroLanguageVersion.None,
                            IProjectEntry projEntry = null,
                            string filename = null)
        {
            if (body == null)
            {
                throw new ArgumentNullException("body");
            }
            _langVersion = langVersion;
            _body = body;
            _lineLocations = lineLocations;
            _projEntry = projEntry;
        }

        public AstNode Body
        {
            get { return _body; }
        }

        public GeneroLanguageVersion LanguageVersion
        {
            get { return _langVersion; }
        }

        public string Filepath
        {
            get
            {
                if (_projEntry != null)
                    return _projEntry.FilePath;
                return _filename;
            }
        }

        public IGeneroProjectEntry ProjectEntry
        {
            get
            {
                return _projEntry as IGeneroProjectEntry;
            }
        }


        internal bool TryGetAttribute(AstNode node, object key, out object value)
        {
            Dictionary<object, object> nodeAttrs;
            if (_attributes.TryGetValue(node, out nodeAttrs))
            {
                return nodeAttrs.TryGetValue(key, out value);
            }
            else
            {
                value = null;
            }
            return false;
        }

        internal void SetAttribute(AstNode node, object key, object value)
        {
            Dictionary<object, object> nodeAttrs;
            if (!_attributes.TryGetValue(node, out nodeAttrs))
            {
                nodeAttrs = _attributes[node] = new Dictionary<object, object>();
            }
            nodeAttrs[key] = value;
        }

        /// <summary>
        /// Copies attributes that apply to one node and makes them available for the other node.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public void CopyAttributes(AstNode from, AstNode to)
        {
            Dictionary<object, object> nodeAttrs;
            if (_attributes.TryGetValue(from, out nodeAttrs))
            {
                _attributes[to] = new Dictionary<object, object>(nodeAttrs);
            }
        }

        internal SourceLocation IndexToLocation(int index)
        {
            if (index == -1)
            {
                return SourceLocation.Invalid;
            }

            var locs = _lineLocations;
            int match = Array.BinarySearch(locs, index);
            if (match < 0)
            {
                // If our index = -1, it means we're on the first line.
                if (match == -1)
                {
                    return new SourceLocation(index, 1, index + 1);
                }

                // If we couldn't find an exact match for this line number, get the nearest
                // matching line number less than this one
                match = ~match - 1;
            }
            return new SourceLocation(index, match + 2, index - locs[match] + 1);
        }

        internal int LineNumberToIndex(int lineNumber)
        {
            return _lineLocations[lineNumber];
        }

        public LocationInfo ResolveLocation(IProjectEntry entry, object location)
        {
            IAnalysisResult result = location as IAnalysisResult;
            if (result != null)
            {
                var locIndex = result.LocationIndex;
                var loc = IndexToLocation(locIndex);
                return new LocationInfo(entry, loc.Line, loc.Column, locIndex);
            }
            return null;
        }

        protected LocationInfo ResolveLocationInternal(IGeneroProject project, IProjectEntry projectEntry, object location)
        {
            IAnalysisResult result = location as IAnalysisResult;
            if (result != null)
            {
                IProjectEntry projEntry = projectEntry;
                IAnalysisResult trueRes = result;
                if (projEntry == null)
                {
                    if (project is GeneroProject)
                    {
                        trueRes = (project as GeneroProject).GetMemberOfType(result.Name, this, true, true, true, true, out projEntry);
                    }
                }

                if (projEntry != null && projEntry is IGeneroProjectEntry)
                {
                    var ast = (projEntry as IGeneroProjectEntry).Analysis;
                    var locIndex = trueRes.LocationIndex;
                    var loc = ast.IndexToLocation(locIndex);
                    return new LocationInfo(projEntry, loc.Line, loc.Column, locIndex);
                }
            }
            return null;
        }

        public LocationInfo ResolveLocation(object location)
        {
            LocationInfo locInfo = null;
            if (location is IAnalysisResult)
            {
                locInfo = (location as IAnalysisResult).Location;
            }
            else if (location is LocationInfo)
            {
                locInfo = location as LocationInfo;
            }
               
            if(locInfo != null)
            {
                // are the line and column filled in?
                if(locInfo.Index > 0 && (locInfo.Line <= 0 || locInfo.Column <= 0))
                {
                    var loc = IndexToLocation(locInfo.Index);
                    return _projEntry == null ?
                        new LocationInfo(_filename, loc.Line, loc.Column, locInfo.Index) :
                        new LocationInfo(_projEntry, loc.Line, loc.Column, locInfo.Index);
                }
                return locInfo;
            }
            else if(location is IAnalysisResult)
            {
                var locIndex = (location as IAnalysisResult).LocationIndex;
                var loc = IndexToLocation(locIndex);
                return _projEntry == null ?
                    new LocationInfo(_filename, loc.Line, loc.Column, locIndex) :
                    new LocationInfo(_projEntry, loc.Line, loc.Column, locIndex);
            }

            return null;
        }

        protected static AstNode GetContainingNode(AstNode currentNode, int index)
        {
            AstNode containingNode = null;
            if (currentNode.Children.Count > 0)
            {
                List<int> keys = currentNode.Children.Select(x => x.Key).ToList();
                int searchIndex = keys.BinarySearch(index);
                if (searchIndex < 0)
                {
                    searchIndex = ~searchIndex;
                    if (searchIndex > 0)
                        searchIndex--;
                }

                int key = keys[searchIndex];

                // TODO: need to handle multiple results of the same name
                containingNode = currentNode.Children[key];
            }
            return containingNode;
        }

        public abstract IEnumerable<MemberResult> GetDefinedMembers(int index, AstMemberType memberType);

        public abstract IAnalysisResult GetValueByIndex(string exprText, int index, IFunctionInformationProvider functionProvider,
                                               IDatabaseInformationProvider databaseProvider, IProgramFileProvider programFileProvider,
                                               bool isFunctionCallOrDefinition, out bool isDeferred,
                                               out IGeneroProject definingProject, out IProjectEntry projectEntry,
                                               FunctionProviderSearchMode searchInFunctionProvider = FunctionProviderSearchMode.NoSearch);

        public abstract IEnumerable<IAnalysisVariable> GetVariablesByIndex(string exprText, int index, IFunctionInformationProvider functionProvider,
                                                                  IDatabaseInformationProvider databaseProvider, IProgramFileProvider programFileProvider,
                                                                  bool isFunctionCallOrDefinition);

        public abstract IEnumerable<IFunctionResult> GetSignaturesByIndex(string exprText, int index, IReverseTokenizer revTokenizer, IFunctionInformationProvider functionProvider);

        public abstract IEnumerable<MemberResult> GetContextMembers(int index, IReverseTokenizer revTokenizer, IFunctionInformationProvider functionProvider,
                                                           IDatabaseInformationProvider databaseProvider, IProgramFileProvider programFileProvider,
                                                           out bool includePublicFunctions, out bool includeDatabaseTables, string contextStr,
                                                           GetMemberOptions options = GetMemberOptions.IntersectMultipleResults);

        public abstract IEnumerable<MemberResult> GetAllAvailableMembersByIndex(int index, IReverseTokenizer revTokenizer,
                                                                        out bool includePublicFunctions, out bool includeDatabaseTables,
                                                                        GetMemberOptions options = GetMemberOptions.IntersectMultipleResults);

        public abstract void CheckForErrors(Action<string, int, int> errorFunc);
    }
}
