using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace logic
{
    public class Token
    {
        
        public TokenType Type { get; private set; }
        public string Name { get; private set; }
        public Token(TokenType type)
        {
            Type = type;
        }
        //za f-q
        public Token(string name)
        {
            Type = TokenType.Function;
            Name = name;
        }
        //public int type;
    }
    public enum TokenType
    { 
        None=0,
        //skobi
        OpenBracket,
        CloseBracket,
        //operatori
        Or,
        And,
        Not,
        
        //operand
        Operand,
        //f-iq
        Function
    }
}
