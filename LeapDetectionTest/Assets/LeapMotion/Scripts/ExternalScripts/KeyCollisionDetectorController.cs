using UnityEngine;
using System.Collections;
using System;
using Leap.Unity.Attributes;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using SpellingCorrector;
namespace Leap.Unity {

	public class KeyCollisionDetectorController : MonoBehaviour {

		[Tooltip("Drag the display text to here")]
		public TextMesh OutputTextMesh;

		private int textLength = 0;

		private bool isCaps = false;
		private bool isShift = false;
		private bool isActive = false;

		//Swapping
		private bool swapStatus;
		List<string> vals;
		List<string> shiftVals;

		Dictionary<string, string> swapMap = new Dictionary<string, string>	{
					{  "Q", "1" },
					{  "W", "2" },
					{  "E", "3" },
					{  "R", "4" },
					{  "T", "5" },
					{  "Y", "6" },
					{  "U", "7" },
					{  "I", "8" },
					{  "O", "9" },
					{  "P", "0" },
					{  "A", "!" },
					{  "S", "@" },
					{  "D", "#" },
					{  "F", "$" },
					{  "G", "%" },
					{  "H", "^" },
					{  "J", "&" },
					{  "K", "(" },
					{  "L", ")" },
					{  "Z", "*" },
					{  "X", "+" },
					{  "C", "-" },
					{  "V", "=" },
					{  "B", "\"" },
					{  "N", "_" },
					{  "M", ";" },
					{  ",", ":" },
					{  ".", "?" },
		} ;


		//Autocorrect
    private Spelling spelling;



		public bool getCaps(){ return isCaps; }
		public void setCaps(bool newVal){ isCaps = newVal; }

		public bool getShift(){ return isShift; }
		public void setShift(bool newVal){ isShift = newVal; }

		public bool getActive(){ return isActive; }

		public void activate(){
			isActive = true;
		}
		public void deactivate(){
			isActive = false;
		}


		public void Start(){
			spelling = new Spelling();
		}

		public void Awake(){
			vals = new List<string> ();
			shiftVals = new List<string> ();
			swapStatus = false;
		}


		//outputs the character/string to the text shown to user
		public void outputText(string KeyValue, string KeyShiftValue){
			if (this.getCaps() || this.getShift()){
				OutputTextMesh.text += KeyShiftValue;
				if (this.getShift()){
					this.setShift(false);
				}
			}
			else{

				if(KeyValue == " "){
					String[] spl= OutputTextMesh.text.Split(' '); //splits output string by spaces
					OutputTextMesh.text = OutputTextMesh.text.Substring(0, OutputTextMesh.text.Length - spl[spl.Length - 1].Length); //eliminates last word
					//Debug.Log(OutputTextMesh.text);
					OutputTextMesh.text += spelling.Correct(spl[spl.Length - 1]); //adds in corrected last word
					OutputTextMesh.text += KeyValue; //add in space
				}else{
				OutputTextMesh.text += KeyValue;
				}

			}
			textLength ++;
		}

		//performs function of enterKey
		public void enterKey(){
			textLength = 0;
			OutputTextMesh.text += "\n";
		}

		//performs function of deleteKey
		public void deleteKey(){
			string temp = OutputTextMesh.text;
			OutputTextMesh.text = temp.Substring(0, temp.Length - 1);
		}

		//swaps keyboard to alternate symbols
		//Important precondition: child 0 is the Text, child 1 is the Key.
		public void swap () {
			if (!swapStatus) {
				//Gets the keyboard, then its all children keys
				foreach (Transform child in transform.GetChild (0)) {
					TextMesh textMesh = child.GetChild (0).GetChild (0).GetComponent<TextMesh> ();
					KeyCollisionDetector detector = child.GetChild(0).GetChild(1).GetComponent<KeyCollisionDetector>();
					//GetComponentsInChildren gets self as well, which we will skip
					if (detector == null) continue;
					vals.Add (detector.KeyValue);
					shiftVals.Add(detector.KeyShiftValue);
					if (swapMap.ContainsKey (textMesh.text)) {
						string prevText = textMesh.text;
						textMesh.text = swapMap [prevText];
						detector.KeyValue = swapMap [prevText];
						//shift has no effect in alternate keyboard, so same key value
						detector.KeyShiftValue = detector.KeyValue;
					}
				}
				swapStatus = true;
			} else {
				//The two lists keep track of all the keys in the order they
				//were replaced, so this goes and reverses replacement the same way
				int i = 0;
				foreach (Transform child in transform.GetChild (0)) {
					TextMesh textMesh = child.GetChild (0).GetChild (0).GetComponent<TextMesh> ();
					KeyCollisionDetector detector = child.GetChild(0).GetChild(1).GetComponent<KeyCollisionDetector>();
					if (detector == null) continue;
					if(detector.isNormalKey){ //ignores special keys (enter, shift, ...)
						textMesh.text = shiftVals [i];
						detector.KeyValue = vals [i];
						detector.KeyShiftValue = shiftVals [i];
					}
					i++;
				}
				swapStatus = false;
			}
		}


	}
}


//Autocorrect TODO move out of here?
namespace SpellingCorrector
{
	/// <summary>
	/// Conversion from http://norvig.com/spell-correct.html by C.Small
	/// </summary>
	public class Spelling
	{
		//private Dictionary<string, double> _bigrams = new Dictionary<string, double>();
		private Dictionary<string, double> _unigrams = new Dictionary<string, double>();
		private static Regex _wordRegex = new Regex("[a-z]+", RegexOptions.Compiled);
		private Dictionary<string, double> _transProb = new Dictionary<string, double>();

		public Spelling()
		{
			string fileContent = File.ReadAllText("Assets/big.txt");
			string transProbText = File.ReadAllText("Assets/count_1edit.txt");
			List<string> wordList = fileContent.Split(new string[] {"\n"} , StringSplitOptions.RemoveEmptyEntries).ToList();
			List<string> transProbList = transProbText.Split(new string[] {"\n"} , StringSplitOptions.RemoveEmptyEntries).ToList();

			//CONSTRUCT UNIGRAM & BIGRAM
			for (int k = 0; k < wordList.Count(); k++)
			{
				List<string> vocabs = wordList[k].Split(new string[] {" "}, StringSplitOptions.RemoveEmptyEntries).ToList();
				for (int i = 0; i < vocabs.Count(); i++)
				{
					string trimmedWord = vocabs[i].Trim().ToLower();
					//defines unigram prob dictionary
					if (_wordRegex.IsMatch(trimmedWord))
					{
						//ADD TO UNIGRAM
						if (_unigrams.ContainsKey(trimmedWord))
						{
							_unigrams[trimmedWord] += 1.00/wordList.Count; //keeps it probability
						}
						else
						{
							_unigrams.Add(trimmedWord, 1.00/wordList.Count);
						}
					}
				}
			}
            Debug.Log("number of keys in unigram is " + _unigrams.Keys.Count.ToString());

			//CONSTRUCT TRANSITION PROB
			double transProbSum = 0;
			foreach (var trans in transProbList)
			{
				//split each line of transprob into transition and its probability
				string[] entry = trans.Split(new string[] {" ", "	"} , StringSplitOptions.RemoveEmptyEntries);

				//if parse was successful, add it to _transProb
				int number;
				if(!_transProb.ContainsKey(entry[0]) && int.TryParse(entry[1], out number))
				{
					transProbSum += Convert.ToInt32(entry[1]);
					_transProb.Add(entry[0], Convert.ToInt32(entry[1]));
				}
			}
			List<string> keys = new List<string> (_transProb.Keys);
			foreach (var trans in keys)
			{
				_transProb[trans] /= transProbSum;
			}
		}

		public string Correct(string word)
		{
			if (string.IsNullOrEmpty(word))
				return word;

			word = word.ToLower();

			if (_unigrams.ContainsKey(word))
				return word;

			List<Tuple<string, string>> list = Edits(word);
			Dictionary<string, double> candidates = new Dictionary<string, double>(); //Organized as alternative -> score

			foreach (Tuple<string, string> pair in list)
			{
				string wordVariation = pair.Item1;
				string trans = pair.Item2;

				//wordVariation already in candidate
				if(candidates.ContainsKey(wordVariation))
				{
					continue;
				}

				//this wordVariation is not in the dictionary or transprob is not in dictionary
				if (!_unigrams.ContainsKey(wordVariation) || !_transProb.ContainsKey(trans))
				{
					candidates.Add(wordVariation, 0.0);
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
						candidates.Add(wordVariation, _transProb[trans]*_unigrams[wordVariation]);
					}
				}
			}

			string result = (candidates.Count > 0) ? candidates.OrderByDescending(x => x.Value).First().Key : word;

			Debug.Log("Result is " + result);
			return result;
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
					var pair = new Tuple<string,string>(a + b.Substring(1),
						last +b[0] + "|" + last);
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
					var pair = new Tuple<string,string>(a + b[1] + b[0] + b.Substring(2),
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
						var pair = new Tuple<string,string>(a + c + b.Substring(1),
						 b.Substring(0) +"|" + c);
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
					var pair = new Tuple<string,string>(a + c + b,
						last + "|" + last + c);
					inserts.Add(pair);
				}
			}

			return deletes.Union(transposes).Union(replaces).Union(inserts).ToList();
		}

	}
}

public class Tuple<T,U>
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
