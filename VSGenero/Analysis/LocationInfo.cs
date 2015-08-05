using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis
{
    public class LocationInfo : IEquatable<LocationInfo>, ILocationResolver {
        private readonly int _line, _column, _index;
        private readonly IProjectEntry _entry;
        private readonly string _filename;
        internal static LocationInfo[] Empty = new LocationInfo[0];

        private static readonly IEqualityComparer<LocationInfo> _fullComparer = new FullLocationComparer();

        public LocationInfo(string filename, int line, int column, int index)
        {
            _filename = filename;
            _line = line;
            _column = column;
            _index = index;
        }

        public LocationInfo(string filename, int index)
        {
            _filename = filename;
            _index = index;
        }

        internal LocationInfo(IProjectEntry entry, int line, int column, int index) {
            _entry = entry;
            _line = line;
            _column = column;
            _index = index;
        }

        public IProjectEntry ProjectEntry {
            get {
                return _entry;
            }
        }

        public string FilePath {
            get 
            { 
                return _entry == null ? _filename : _entry.FilePath; 
            }
        }

        public int Line {
            get { return _line; }
        }

        public int Column {
            get {
                return _column;
            }
        }

        public int Index { get { return _index; } }

        public override bool Equals(object obj) {
            LocationInfo other = obj as LocationInfo;
            if (other != null) {
                return Equals(other);
            }
            return false;
        }

        public override int GetHashCode() 
        {
            int hashCode = Line.GetHashCode();
            if(ProjectEntry != null)
                hashCode ^= ProjectEntry.GetHashCode();
            return hashCode;
        }

        public bool Equals(LocationInfo other) {
            // currently we filter only to line & file - so we'll only show 1 ref per each line
            // This works nicely for get and call which can both add refs and when they're broken
            // apart you still see both refs, but when they're together you only see 1.
            return Line == other.Line &&
                ProjectEntry == other.ProjectEntry;
        }

        /// <summary>
        /// Provides an IEqualityComparer that compares line, column and project entries.  By
        /// default locations are equaitable based upon only line/project entry.
        /// </summary>
        public static IEqualityComparer<LocationInfo> FullComparer {
            get{
                return _fullComparer;
            }
        }

        sealed class FullLocationComparer : IEqualityComparer<LocationInfo> {
            public bool Equals(LocationInfo x, LocationInfo y) {
                return x.Line == y.Line &&
                    x.Column == y.Column &&
                    x._filename == y._filename &&
                    x.ProjectEntry == y.ProjectEntry;
            }

            public int GetHashCode(LocationInfo obj) 
            {
                int hashCode = obj.Line.GetHashCode() ^ obj.Column.GetHashCode();
                if(obj.ProjectEntry != null)
                    hashCode ^= obj.ProjectEntry.GetHashCode();
                return hashCode;
            }
        }

        #region ILocationResolver Members

        LocationInfo ILocationResolver.ResolveLocation(IProjectEntry project, object location) {
            return this;
        }

        #endregion
    }
}
