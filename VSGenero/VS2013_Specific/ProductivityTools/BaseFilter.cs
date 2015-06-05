using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.VS2013Plus.ProductivityTools
{
    internal class BaseFilter
    {
        // Fields
        protected int position;
        protected ITextSnapshot snapshot;

        // Methods
        public BaseFilter(ITextSnapshot snapshot)
        {
            this.snapshot = snapshot;
            this.position = -1;
        }

        public bool PeekEof(int position)
        {
            return (this.position + position) >= snapshot.Length;
        }

        public char PeekNextChar()
        {
            return this.PeekNextChar(1);
        }

        public char PeekNextChar(int offset)
        {
            int num = this.position + offset;
            if ((0 <= num) && (num < this.snapshot.Length))
            {
                return this.snapshot[num];
            }
            return char.MaxValue;
        }

        public char PeekPrevChar()
        {
            return this.PeekPrevChar(1);
        }

        public char PeekPrevChar(int offset)
        {
            int num = this.position - offset;
            if ((0 <= num) && (num < this.snapshot.Length))
            {
                return this.snapshot[num];
            }
            return char.MinValue;
        }

        // Properties
        public char Character
        {
            get
            {
                return this.snapshot[this.position];
            }
        }

        public char LowerCharacter
        {
            get
            {
                return char.ToLower(this.snapshot[this.position]);
            }
        }

        public int Position
        {
            get
            {
                return this.position;
            }
            set
            {
                this.position = value;
            }
        }

        public void IncrementPosition(int increment)
        {
            Position = Position + increment;
        }
    }

}
