using System.Collections;
using UnityEngine;
using SpellingCorrector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

public class NLP_bigram : MonoBehaviour
{


    // Use this for initialization
    void Start()
    {

        Spelling spelling = new Spelling();
        float t = Time.realtimeSinceStartup;
        string sentence = "Saample sentince is goone liek this"; // sees speed instead of spelled (see notes on norvig.com)
        string correction = "";
        string[] sentences = sentence.Split(' ');
        for (int i = 0; i < sentence.Split(' ').Length; i++)
        {
            string prevItem = ">";
            if (i > 0)
            {
                prevItem = sentences[i - 1];
            }
            string item = sentences[i];
            correction += " " + spelling.Correct(item, prevItem);
        }
        Debug.Log("Did you mean:" + correction);
        Debug.Log(Time.realtimeSinceStartup - t);


    }

    // Update is called once per frame
    void Update()
    {

    }


}

namespace SpellingCorrector
{

    public class Spelling
    {
        private Dictionary<string, double> _bigrams = new Dictionary<string, double>();
        private Dictionary<string, double> _unigrams = new Dictionary<string, double>();
        private static Regex _wordRegex = new Regex("[a-z]+", RegexOptions.Compiled);
        private Dictionary<string, double> _transProb = new Dictionary<string, double>();


        public Spelling()
        {
            string fileContent = File.ReadAllText("Assets/LeapMotion/Scripts/ExternalScripts/NLP/big.txt");
            string transProbText = File.ReadAllText("Assets/LeapMotion/Scripts/ExternalScripts/NLP/count_1edit.txt");
            List<string> wordList = fileContent.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            List<string> transProbList = transProbText.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();

            //CONSTRUCT UNIGRAM & BIGRAM
            for (int k = 0; k < wordList.Count(); k++)
            {
                List<string> vocabs = wordList[k].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList();
                for (int i = 0; i < vocabs.Count(); i++)
                {
                    string trimmedWord = vocabs[i].Trim().ToLower();
                    //defines unigram prob dictionary
                    if (_wordRegex.IsMatch(trimmedWord))
                    {
                        //ADD TO UNIGRAM
                        if (_unigrams.ContainsKey(trimmedWord))
                        {
                            _unigrams[trimmedWord] += 1.00; //keeps it probability
                        }
                        else
                        {
                            _unigrams.Add(trimmedWord, 1.00);
                        }


                        //CONSTRUCT BIGRAM
                        string key = "> " + trimmedWord;
                        if (i > 0) //not the first word
                        {
                            key = vocabs[i - 1].Trim().ToLower() + " " + trimmedWord;
                        }

                        if (_bigrams.ContainsKey(key))
                        {
                            _bigrams[key] += 1.00;  //keeps it probability
                        }
                        else
                        {
                            _bigrams.Add(key, 1.00);
                        }
                    }

                }

            }

            //CONSTRUCT TRANSITION PROB
            double transProbSum = 0;
            foreach (var trans in transProbList)
            {
                //split each line of transprob into transition and its probability
                string[] entry = trans.Split(new string[] { " ", "	" }, StringSplitOptions.RemoveEmptyEntries);

                //if parse was successful, add it to _transProb
                int number;
                if (!_transProb.ContainsKey(entry[0]) && int.TryParse(entry[1], out number))
                {
                    transProbSum += Convert.ToInt32(entry[1]);
                    _transProb.Add(entry[0], Convert.ToInt32(entry[1]));
                }
            }
            List<string> keys = new List<string>(_transProb.Keys);
            foreach (var trans in keys)
            {
                _transProb[trans] /= transProbSum;
            }
        }

        public string Correct(string word, string lastWord)
        {
            if (string.IsNullOrEmpty(word))
                return word;

            word = word.ToLower();
            lastWord = lastWord.ToLower();

            // known()
            if (_unigrams.ContainsKey(word))
                return word;

            List<Tuple<string, string>> list = Edits(word);
            Dictionary<string, double> candidates = new Dictionary<string, double>(); //Organized as alternative -> score

            foreach (Tuple<string, string> pair in list)
            {
                string wordVariation = pair.Item1;
                string trans = pair.Item2;

                //wordVariation already in candidate
                if (candidates.ContainsKey(wordVariation))
                {
                    continue;
                }

                //this wordVariation is not in the dictionary or transprob is not in dictionary
                if (!_unigrams.ContainsKey(wordVariation))
                {
                    candidates.Add(wordVariation, 0.0);
                }
                else if (lastWord != "<" && !_bigrams.ContainsKey(lastWord + " " + wordVariation))
                {
                    double eta = 1 / _bigrams.Count() + 0.3 * _unigrams[wordVariation] / _unigrams.Count();
                    candidates.Add(wordVariation, eta);
                }
                //this wordVariation EXISTS in the dictionary
                else
                {
                    if (!_transProb.ContainsKey(trans)) //this translation itself is very unlikely
                    {
                        candidates.Add(wordVariation, 0.0);
                    }
                    else //entry exists in dict, and transition is likely! calculate score.
                    {
                        double score = _bigrams[lastWord + ' ' + wordVariation] / _bigrams.Count() +
                            //stupid backoff
                            0.3 * _unigrams[wordVariation] / _unigrams.Count();
                        Debug.Log("Score for " + lastWord + " " + wordVariation + " is " + score.ToString());
                        candidates.Add(wordVariation, score);
                    }
                }
            }

            // SECONDARY BRANCH, IGNORE THIS FOR NOW
            // when none of the options have been registered in the dictionary
            /*
			foreach (string item in list)
			{
				foreach (string wordVariation in Edits(item)) //WOW WTF IS THIS(BRANCH ONCE MORE?)
				{
					if (_unigrams.ContainsKey(wordVariation) && !candidates.ContainsKey(wordVariation))
						candidates.Add(wordVariation, _unigrams[wordVariation]);
				}
			}*/

            return (candidates.Count > 0) ? candidates.OrderByDescending(x => x.Value).First().Key : word;
        }


        //EDIT MODELS 
        //EACH ENTRY: (transition pattern, result)
        private List<Tuple<string, string>> Edits(string word)
        {
            var splits = new List<Tuple<string, string>>();
            var transposes = new List<Tuple<string, string>>();
            var deletes = new List<Tuple<string, string>>();
            var replaces = new List<Tuple<string, string>>();
            var inserts = new List<Tuple<string, string>>();

            // Splits
            for (int i = 0; i < word.Length; i++)
            {
                var tuple = new Tuple<string, string>(word.Substring(0, i), word.Substring(i));
                splits.Add(tuple);
            }

            // Deletes
            for (int i = 0; i < splits.Count; i++)
            {
                string a = splits[i].Item1;
                string b = splits[i].Item2;
                if (!string.IsNullOrEmpty(b))
                {
                    string last = ">";
                    if (a.Length > 0)
                    {
                        last = a.Last().ToString();
                    }
                    var pair = new Tuple<string, string>(a + b.Substring(1),
                        last + b[0] + "|" + last);
                    deletes.Add(pair);
                }
            }

            // Transposes
            for (int i = 0; i < splits.Count; i++)
            {
                string a = splits[i].Item1;
                string b = splits[i].Item2;
                if (b.Length > 1)
                {
                    var pair = new Tuple<string, string>(a + b[1] + b[0] + b.Substring(2),
                        b[0] + b[1] + "|" + b[1] + b[0]);
                    transposes.Add(pair);
                }
            }

            // Replaces
            for (int i = 0; i < splits.Count; i++)
            {
                string a = splits[i].Item1;
                string b = splits[i].Item2;
                if (!string.IsNullOrEmpty(b))
                {
                    for (char c = 'a'; c <= 'z'; c++)
                    {
                        var pair = new Tuple<string, string>(a + c + b.Substring(1),
                            b.Substring(0) + "|" + c);
                        replaces.Add(pair);
                    }
                }
            }

            // Inserts
            for (int i = 0; i < splits.Count; i++)
            {
                string a = splits[i].Item1;
                string b = splits[i].Item2;
                for (char c = 'a'; c <= 'z'; c++)
                {
                    string last = ">";
                    if (a.Length > 0)
                    {
                        last = a.Last().ToString();
                    }
                    var pair = new Tuple<string, string>(a + c + b,
                        last + "|" + last + c);
                    inserts.Add(pair);
                }
            }

            return deletes.Union(transposes).Union(replaces).Union(inserts).ToList();
        }

    }
}

public class Tuple<T, U>
{
    public T Item1 { get; private set; }
    public U Item2 { get; private set; }

    public Tuple(T item1, U item2)
    {
        Item1 = item1;
        Item2 = item2;
    }
}

public static class Tuple
{
    public static Tuple<T, U> Create<T, U>(T item1, U item2)
    {
        return new Tuple<T, U>(item1, item2);
    }
}