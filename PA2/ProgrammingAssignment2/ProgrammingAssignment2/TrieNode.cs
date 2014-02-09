using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProgrammingAssignment2
{
    public class TrieNode
    {
            public bool IsTerminal = false;
            public Dictionary<char, TrieNode> Edges = new Dictionary<char, TrieNode>();
    }
}