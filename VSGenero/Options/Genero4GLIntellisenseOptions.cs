using Microsoft.VisualStudio.VSCommon.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSGenero.EditorExtensions.Intellisense;

namespace VSGenero.Options
{
    public class Genero4GLIntellisenseOptions : BaseOptions
    {
        private const string _defaultCompletionChars = "{}[]().,:;+-*/%&|^~=<>#'\"\\";

        public Genero4GLIntellisenseOptions()
             : base(VSGeneroConstants.BaseRegistryKey, "Intellisense")
        {
        }

        public string CompletionCommittedBy { get { return GetValue<string>(CompletionCommittedBySetting); }}
        public bool EnterCommitsIntellisense { get { return GetValue<bool>(EnterCommitsSetting); } }
        public bool AddNewLineAtEndOfFullyTypedWord { get { return GetValue<bool>(NewLineAtEndOfWordSetting); } }
        public bool SpaceCommitsIntellisense { get { return GetValue<bool>(SpaceCommitsSetting); } }
        public bool SpaceShowsCompletionList {  get { return GetValue<bool>(SpacebarShowsCompletionListSetting); } }
        public bool PreSelectMRU { get { return GetValue<bool>(PreSelectMRUSetting); } }
        public CompletionAnalysisType AnalysisType { get { return GetValue<CompletionAnalysisType>(CompletionAnalysisTypeSetting); } }

        public const string CompletionCommittedBySetting = "CompletionCommittedBy";
        public const string EnterCommitsSetting = "EnterCommits";
        public const string NewLineAtEndOfWordSetting = "NewLineAtEndOfWord";
        public const string SpaceCommitsSetting = "SpaceCommits";
        public const string SpacebarShowsCompletionListSetting = "SpacebarShowsCompletionList";
        public const string PreSelectMRUSetting = "PreSelectMRU";
        public const string CompletionAnalysisTypeSetting = "ContextAnalysisType";

        protected override void LoadSettingsFromStorage()
        {
            LoadBool(EnterCommitsSetting, true);
            LoadBool(NewLineAtEndOfWordSetting, false);
            LoadString(CompletionCommittedBySetting, _defaultCompletionChars);
            LoadBool(SpaceCommitsSetting, true);
            LoadBool(SpacebarShowsCompletionListSetting, true);
            LoadBool(PreSelectMRUSetting, true);
            LoadEnum(CompletionAnalysisTypeSetting, CompletionAnalysisType.Context);
        }
    }
}
