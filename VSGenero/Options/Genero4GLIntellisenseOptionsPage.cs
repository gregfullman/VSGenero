using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using VSGenero.EditorExtensions.Intellisense;

namespace VSGenero.Options
{
    [ComVisible(true)]
    public class Genero4GLIntellisenseOptionsPage : GeneroDialogPage
    {
        private bool _enterCommitsIntellisense, _addNewLineAtEndOfFullyTypedWord, _spaceCommitsIntellisense, _preSelectMRU;
        private bool _spacebarShowsCompletionList = true;   // set this for now
        private Genero4GLIntellisenseOptionsControl _window;
        private const string _defaultCompletionChars = "{}[]().,:;+-*/%&|^~=<>#'\"\\";
        private string _completionCommittedBy;
        private CompletionAnalysisType _analysisType;

        public Genero4GLIntellisenseOptionsPage() : base("Intellisense")
        {
        }

        // replace the default UI of the dialog page w/ our own UI.
        protected override System.Windows.Forms.IWin32Window Window
        {
            get
            {
                if (_window == null)
                {
                    _window = new Genero4GLIntellisenseOptionsControl();
                }
                return _window;
            }
        }

        public string CompletionCommittedBy
        {
            get { return _completionCommittedBy; }
            set { _completionCommittedBy = value; }
        }

        public bool SpaceCommitsIntellisense
        {
            get { return _spaceCommitsIntellisense; }
            set { _spaceCommitsIntellisense = value; }
        }

        public bool AddNewLineAtEndOfFullyTypedWord
        {
            get { return _addNewLineAtEndOfFullyTypedWord; }
            set { _addNewLineAtEndOfFullyTypedWord = value; }
        }

        public bool PreSelectMRU
        {
            get { return _preSelectMRU; }
            set { _preSelectMRU = value; }
        }

        public CompletionAnalysisType AnalysisType
        {
            get { return _analysisType; }
            set { _analysisType = value; }
        }

        public override void ResetSettings()
        {
            _enterCommitsIntellisense = true;
            _addNewLineAtEndOfFullyTypedWord = false;
            _completionCommittedBy = _defaultCompletionChars;
            _spaceCommitsIntellisense = true;
            _spacebarShowsCompletionList = true;
            _preSelectMRU = true;
            _analysisType = CompletionAnalysisType.Context;
        }
        
        private const string CompletionCommittedBySetting = "CompletionCommittedBy";
        private const string EnterCommitsSetting = "EnterCommits";
        private const string NewLineAtEndOfWordSetting = "NewLineAtEndOfWord";
        private const string SpaceCommitsSetting = "SpaceCommits";
        private const string SpacebarShowsCompletionListSetting = "SpacebarShowsCompletionList";
        private const string PreSelectMRUSetting = "PreSelectMRU";
        private const string CompletionAnalysisTypeSetting = "ContextAnalysisType";

        public override void LoadSettingsFromStorage()
        {
            _enterCommitsIntellisense = LoadBool(EnterCommitsSetting) ?? true;
            _addNewLineAtEndOfFullyTypedWord = LoadBool(NewLineAtEndOfWordSetting) ?? false;
            _completionCommittedBy = LoadString(CompletionCommittedBySetting) ?? _defaultCompletionChars;
            _spaceCommitsIntellisense = LoadBool(SpaceCommitsSetting) ?? true;
            _spacebarShowsCompletionList = LoadBool(SpacebarShowsCompletionListSetting) ?? true;
            _preSelectMRU = LoadBool(PreSelectMRUSetting) ?? true;
            _analysisType = LoadEnum<CompletionAnalysisType>(CompletionAnalysisTypeSetting) ?? CompletionAnalysisType.Context;
        }

        public override void SaveSettingsToStorage()
        {
            SaveBool(EnterCommitsSetting, _enterCommitsIntellisense);
            SaveBool(NewLineAtEndOfWordSetting, _addNewLineAtEndOfFullyTypedWord);
            SaveString(CompletionCommittedBySetting, _completionCommittedBy);
            SaveBool(SpaceCommitsSetting, _spaceCommitsIntellisense);
            SaveBool(SpacebarShowsCompletionListSetting, _spacebarShowsCompletionList);
            SaveBool(PreSelectMRUSetting, _preSelectMRU);
            SaveEnum<CompletionAnalysisType>(CompletionAnalysisTypeSetting, _analysisType);
        }
    }
}
