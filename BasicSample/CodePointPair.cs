using System;

namespace BasicSample
{
    struct CodePointPair : IEquatable<CodePointPair>
    {
        public uint First { get; set; }
        public uint Second { get; set; }

        public CodePointPair(uint first, uint second)
        {
            this.First = first;
            this.Second = second;
        }

        public bool Equals(CodePointPair other)
        {
            return this.First == other.First && this.Second == other.Second;
        }

        public override bool Equals(object obj)
        {
            return obj is CodePointPair && this.Equals((CodePointPair)obj);
        }

        public override int GetHashCode()
        {
            return (int)(this.First ^ this.Second);
        }
    }
}
