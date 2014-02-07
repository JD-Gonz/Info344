using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProgrammingAssignment2
{
    public class TrieNode
    {
            public string Word;
            public bool IsTerminal { get { return Word != null; } }
            public Dictionary<char, TrieNode> Edges = new Dictionary<char, TrieNode>();
    }
}