using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Options
{
    [ComVisible(true)]
    public class Genero4GLIntellisenseOptionsPage : GeneroDialogPage
    {
        private bool _enterCommitsIntellisense, _addNewLineAtEndOfFullyTypedWord, _showCompletionList, _spaceCommitsIntellisense, _preSelectMRU;
        private bool _spacebarShowsCompletionList = true;   // set this for now
        private Genero4GLIntellisenseOptionsControl _window;
        private const string _defaultCompletionChars = "{}[]().,:;+-*/%&|^~=<>#'\"\\";
        private string _completionCommittedBy;

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

        public bool EnterCommitsIntellisense
        {
            get { return _enterCommitsIntellisense; }
            set { _enterCommitsIntellisense = value; }
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

        public bool ShowCompletionList
        {
            get { return _showCompletionList; }
            set { _showCompletionList = value; }
        }

        public bool SpacebarShowsCompletionList
        {
            get { return _spacebarShowsCompletionList; }
            set { _spacebarShowsCompletionList = value; }
        }

        public override void ResetSettings()
        {
            _enterCommitsIntellisense = true;
            _addNewLineAtEndOfFullyTypedWord = false;
            _completionCommittedBy = _defaultCompletionChars;
            _spaceCommitsIntellisense = true;
            _showCompletionList = true;
            _spacebarShowsCompletionList = true;
            _preSelectMRU = true;
        }
        
        private const string CompletionCommittedBySetting = "CompletionCommittedBy";
        private const string EnterCommitsSetting = "EnterCommits";
        private const string NewLineAtEndOfWordSetting = "NewLineAtEndOfWord";
        private const string SpaceCommitsSetting = "SpaceCommits";
        private const string ShowCompletionListSetting = "ShowCompletionList";
        private const string SpacebarShowsCompletionListSetting = "SpacebarShowsCompletionList";
        private const string PreSelectMRUSetting = "PreSelectMRU";

        public override void LoadSettingsFromStorage()
        {
            _enterCommitsIntellisense = LoadBool(EnterCommitsSetting) ?? true;
            _addNewLineAtEndOfFullyTypedWord = LoadBool(NewLineAtEndOfWordSetting) ?? false;
            _completionCommittedBy = LoadString(CompletionCommittedBySetting) ?? _defaultCompletionChars;
            _spaceCommitsIntellisense = LoadBool(SpaceCommitsSetting) ?? true;
            _showCompletionList = LoadBool(ShowCompletionListSetting) ?? true;
            _spacebarShowsCompletionList = LoadBool(SpacebarShowsCompletionListSetting) ?? true;
            _preSelectMRU = LoadBool(PreSelectMRUSetting) ?? true;

        }

        public override void SaveSettingsToStorage()
        {
            SaveBool(EnterCommitsSetting, _enterCommitsIntellisense);
            SaveBool(NewLineAtEndOfWordSetting, _addNewLineAtEndOfFullyTypedWord);
            SaveString(CompletionCommittedBySetting, _completionCommittedBy);
            SaveBool(SpaceCommitsSetting, _spaceCommitsIntellisense);
            SaveBool(ShowCompletionListSetting, _showCompletionList);
            SaveBool(SpacebarShowsCompletionListSetting, _spacebarShowsCompletionList);
            SaveBool(PreSelectMRUSetting, _preSelectMRU);
        }
    }
}
