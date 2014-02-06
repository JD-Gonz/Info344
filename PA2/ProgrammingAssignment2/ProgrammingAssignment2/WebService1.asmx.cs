using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Data.SqlClient;
using System.IO;
using Microsoft.WindowsAzure.Storage.Table;
using System.Web.Script.Services;
using System.Web.Script.Serialization;

namespace ProgrammingAssignment2
{
    /// <summary>
    /// Summary description for WebService1
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class WebService1 : System.Web.Services.WebService
    {

        public static Trie library;
        private static int characterIndex = -1;
        private static int maxlength = 10;

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string querySuggestions(string word)
        {
            string[] json = new string[10];
            for (int i = 0; i < maxlength; i++)
            {
                json[i] = getSuggestion(word);
            }
            return new JavaScriptSerializer().Serialize(json);
        }

        public class Trie
        {
            public class Node
            {
                public string Word;
                public bool IsTerminal { get { return Word != null; } }
                public Dictionary<char, Node> Edges = new Dictionary<char, Node>();
            }

            public Node Root = new Node();

            public Trie(string[] words)
            {
                for (int i = 0; i < words.Length; i++)
                {
                    string word = words[i];
                    Node node = Root;
                    for (int index = 0; index < word.Length; index++)
                    {
                        char letter = word[index];
                        Node next;
                        if (!node.Edges.TryGetValue(letter, out next))
                        {
                            next = new Node();
                            if (index + 1 == word.Length)
                            {
                                next.Word = word;
                            }
                            node.Edges.Add(letter, next);
                        }
                        node = next;
                    }
                }
            }
        }

        public string invalidCharacterFilter(string filter)
        {
            string reconstructedWord = "";
            foreach (char ascii in filter)
            {
                bool isAscii = true;
                if (ascii != 32 && (ascii < 65 || ascii > 91) && (ascii < 97 || ascii > 122))
                    isAscii = false;

                if (isAscii)
                    reconstructedWord += ascii;
            }
            return reconstructedWord;
        }

        [WebMethod]
        public void populateTrie(string filepath)
        {
            List<string> wordlibrary = new List<string>();
            using (StreamReader sr = new StreamReader(filepath))
            {
                while (sr.EndOfStream == false)
                {
                    string word = sr.ReadLine();
                    for (int i = 0; i < word.Length; i++)
                    {
                        word = invalidCharacterFilter(word);
                    }
                    wordlibrary.Add(word);
                }
            }
            library = new Trie(wordlibrary.ToArray());
        }

        private string getSuggestion(string word)
        {
            if (library.Root == null)
                return "";

            if (library.Root.IsTerminal)
                return word;

            Trie.Node suggestion = traverseTrie(word, library.Root);
            if (suggestion != null)
                return suggestion.Word;
            else return "No results found.";
        }

        private char getAlphabeticalKey(Trie.Node element)
        {
            char[] array = element.Edges.Keys.ToArray();

            if (array.Length < 10)
                maxlength = array.Length;
            else maxlength = 10;
            if (characterIndex == maxlength)
                characterIndex = -1;
            characterIndex++;
            return array[characterIndex];
        }

        private Trie.Node traverseTrie(string userInput, Trie.Node element)
        {
            if (element.IsTerminal)
                return element;
            else
            {
                // Runs recursively up to the length of user input
                if (userInput.Length != 0)
                {
                    Trie.Node value;
                    if (element.Edges.TryGetValue(userInput[0], out value))
                    {
                        Trie.Node next = value;
                        return traverseTrie(userInput.Substring(1), next);
                    }
                    else return value;
                }
                // Suggests new things here
                else
                {
                    Trie.Node value;
                    char nextLetter = getAlphabeticalKey(element);
                    if (element.Edges.TryGetValue(nextLetter, out value))
                    {
                        return traverseTrie(userInput, value);
                    }
                    else return value;
                }
            }
        }

        [WebMethod]
        public string downloadBlob()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("pa2");
            if (container.Exists())
            {
                string file = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToString();
                foreach (IListBlobItem item in container.ListBlobs(null, false))
                {
                    if (item.GetType() == typeof(CloudBlockBlob))
                    {
                        CloudBlockBlob blob = (CloudBlockBlob)item;
                        using (var fs = new FileStream(file + "\\blob.txt", FileMode.OpenOrCreate))
                        {
                            blob.DownloadToStream(fs);
                        }
                    }
                }
                return file;
            }
            else return "failure";
        }
    }
}
