using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJManager.Core.Others
{
    public enum ComparativeType
    {
        None,
        Equals,
        NotEquals,
        GreaterThan,
        GreaterOrEquals,
        LessThan,
        LessOrEquals
    }

    public class ComparativeItem
    {
        public ComparativeType Type { get; }
        public string Symbol { get; }
        public string Description { get; }

        public ComparativeItem(ComparativeType type, string symbol, string description)
        {
            Type = type;
            Symbol = symbol;
            Description = description;
        }

        public override string ToString() => $"{Symbol} {Description}";
    }

}
