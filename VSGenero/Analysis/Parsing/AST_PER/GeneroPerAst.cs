using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST_PER
{
    public class GeneroPerAst : GeneroAst
    {
        public GeneroPerAst(AstNodePer body, int[] lineLocations, GeneroLanguageVersion langVersion = GeneroLanguageVersion.None, IProjectEntry projEntry = null, string filename = null)
            : base(body, lineLocations, langVersion, projEntry, filename)
        {
        }

        public override void CheckForErrors(Action<string, int, int> errorFunc)
        {
        }

        public override IEnumerable<MemberResult> GetAllAvailableMembersByIndex(int index, IReverseTokenizer revTokenizer, out bool includePublicFunctions, out bool includeDatabaseTables, GetMemberOptions options = GetMemberOptions.IntersectMultipleResults)
        {
            includePublicFunctions = false;
            includeDatabaseTables = false;
            return new MemberResult[0];
        }

        public override IEnumerable<MemberResult> GetContextMembers(int index, IReverseTokenizer revTokenizer, IFunctionInformationProvider functionProvider, IDatabaseInformationProvider databaseProvider, IProgramFileProvider programFileProvider, out bool includePublicFunctions, out bool includeDatabaseTables, string contextStr, GetMemberOptions options = GetMemberOptions.IntersectMultipleResults)
        {
            includePublicFunctions = false;
            includeDatabaseTables = false;
            return new MemberResult[0];
        }

        public override IEnumerable<MemberResult> GetDefinedMembers(int index, AstMemberType memberType)
        {
            return new MemberResult[0];
        }

        public override IEnumerable<IFunctionResult> GetSignaturesByIndex(string exprText, int index, IReverseTokenizer revTokenizer, IFunctionInformationProvider functionProvider)
        {
            return new IFunctionResult[0];
        }

        public override IAnalysisResult GetValueByIndex(string exprText, int index, IFunctionInformationProvider functionProvider, IDatabaseInformationProvider databaseProvider, IProgramFileProvider programFileProvider, bool isFunctionCallOrDefinition, out bool isDeferred, out IGeneroProject definingProject, out IProjectEntry projectEntry, FunctionProviderSearchMode searchInFunctionProvider = FunctionProviderSearchMode.NoSearch)
        {
            definingProject = null;
            projectEntry = null;
            isDeferred = false;
            return null;
        }

        public override IEnumerable<IAnalysisVariable> GetVariablesByIndex(string exprText, int index, IFunctionInformationProvider functionProvider, IDatabaseInformationProvider databaseProvider, IProgramFileProvider programFileProvider, bool isFunctionCallOrDefinition)
        {
            return new IAnalysisVariable[0];
        }
    }
}
