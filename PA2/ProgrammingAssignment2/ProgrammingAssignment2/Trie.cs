using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace ProgrammingAssignment2
{
    public class Trie
    {
        private static int length;
        private int index;
        private TrieNode root;

        public Trie() 
        {
            length = 10;
            index = -1;
            root = new TrieNode();
        }

        public void insertLine(string line)
        {
            TrieNode node = root;
            for (int i = 0; i < line.Length; i++)
            {
                char letter = line[i];
                TrieNode next;
                if (!node.Edges.TryGetValue(letter, out next))
                {
                    next = new TrieNode();
                    if (i + 1 == line.Length)
                    {
                        next.Word = line;
                    }
                    node.Edges.Add(letter, next);
                }
                node = next;
            }
        }

        public string[] getSuggestions(string word)
        {
            List<string> results = new List<string>();
            results = traverseTrie(word, root, results);
            string[] json = results.ToArray();
            return json;
        }

        private List<string> traverseTrie(string userInput, TrieNode element, List<string> results)
        {
            if (results.Count == length)
                return results;
            else
            {// Runs recursively up to the length of user input
                if (userInput.Length != 0)
                {
                    TrieNode value;
                    if (element.Edges.TryGetValue(userInput[0], out value))
                    {
                        TrieNode next = value;
                        return traverseTrie(userInput.Substring(1), next, results);
                    }
                    else return results;
                }
                // Suggests new things here
                else
                {
                    TrieNode value;
                    char[] keys = element.Edges.Keys.ToArray();
                    for (int i = 0; i < keys.Length; i++)
                    {
                        char nextLetter = (keys[i]);
                        if (element.Edges.TryGetValue(nextLetter, out value))
                        {
                            if (value.IsTerminal && results.Count < length)
                                results.Add(value.Word);
                            results = traverseTrie(userInput, value, results);
                        }
                    }
                    return results;
                }
            }
            
        }
    }
}