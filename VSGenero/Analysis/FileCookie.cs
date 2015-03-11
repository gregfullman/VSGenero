using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis
{
    public class FileCookie : IAnalysisCookie
    {
        private readonly string _path;
        private string[] _allLines;

        public FileCookie(string path)
        {
            _path = path;
        }

        public string Path
        {
            get
            {
                return _path;
            }
        }

        #region IFileCookie Members

        public string GetLine(int lineNo)
        {
            if (_allLines == null)
            {
                try
                {
                    _allLines = File.ReadAllLines(Path);
                }
                catch (IOException)
                {
                    _allLines = new string[0];
                }
            }

            if (lineNo - 1 < _allLines.Length)
            {
                return _allLines[lineNo - 1];
            }

            return String.Empty;
        }

        #endregion
    }
}
