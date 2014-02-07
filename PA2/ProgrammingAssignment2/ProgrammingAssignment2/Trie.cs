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

        public TrieNode Root = new TrieNode();

        public Trie() { }

        public Trie(string[] words)
        {
            for (int i = 0; i < words.Length; i++)
            {
                string line = words[i];
                TrieNode node = Root;
                for (int j = 0; j < line.Length; j++)
                {
                    char letter = line[j];
                    TrieNode next;
                    if (!node.Edges.TryGetValue(letter, out next))
                    {
                        next = new TrieNode();
                        if (j + 1 == line.Length)
                        {
                            next.Word = line;
                        }
                        node.Edges.Add(letter, next);
                    }
                    node = next;
                }
            }
        }

        public string formatLine(string line)
        {
            string reconstructedline = "";
            foreach (char letter in line)
            {
                if (letter == '_')
                    reconstructedline += " ";
                else
                    reconstructedline += letter;
            }
            return reconstructedline;
        }

        public string[] traverseTrie(string userInput, TrieNode element, int maxlength)
        {
            length = maxlength;
            List<string> results = new List<string>();
            results = traverseTrieHelper(userInput, element, results);
            String[] array = results.ToArray();
            return array;
        }

        private List<string> traverseTrieHelper(string userInput, TrieNode element, List<string> results)
        {
            // Runs recursively up to the length of user input
            if (userInput.Length > 1)
            {
                TrieNode value;
                if (element.Edges.TryGetValue(Char.ToUpper(userInput[0]), out value) || element.Edges.TryGetValue(Char.ToLower(userInput[0]), out value))
                {
                    return traverseTrieHelper(userInput.Substring(1), value, results);
                }
                else 
                    return results;
            }
             else
                {
                    TrieNode value;
                    if (element.Edges.TryGetValue(userInput[0], out value) && value.IsTerminal)
                    {
                        results.Add(value.Word);
                        return traverseTrieHelper(userInput, value, results);
                    }
                    else
                        return results;
                }
        }
    }
}