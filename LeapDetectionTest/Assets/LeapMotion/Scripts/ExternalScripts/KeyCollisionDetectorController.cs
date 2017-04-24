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
		private static int NEWLINE_THRESHOLD = 15;

		private bool isCaps = false;
		private bool isShift = false;
		private bool isActive = false;
    private Spelling spelling;



		public bool getCaps(){ return isCaps; }
		public void setCaps(bool newVal){ isCaps = newVal; }

		public bool getShift(){ return isShift; }
		public void setShift(bool newVal){ isShift = newVal; }

		public bool getActive(){ return isActive; }
		//public int getActiveID(){ return activeID; }
		public void activate(){
			isActive = true;
			//activeID = keyID;
		}
		public void deactivate(){
			isActive = false;
			//activeID = -1;
		}


		public void Start(){
			spelling = new Spelling();
		}


		//outputs the character/string to the text shown to user
		public void outputText(string KeyValue, string KeyShiftValue){
			if (textLength >= NEWLINE_THRESHOLD){ //automatic enter over threshold
				textLength = 0;
				OutputTextMesh.text += "\n";
			}
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
					Debug.Log(OutputTextMesh.text);
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


	}
}

namespace SpellingCorrector
{
	/// <summary>
	/// Conversion from http://norvig.com/spell-correct.html by C.Small
	/// </summary>
	public class Spelling
	{
		private Dictionary<String, int> _dictionary = new Dictionary<String, int>();
		private static Regex _wordRegex = new Regex("[a-z]+", RegexOptions.Compiled);

		public Spelling()
		{
			string fileContent = File.ReadAllText("assets/big.txt");
			List<string> wordList = fileContent.Split(new string[] {"\n"} , StringSplitOptions.RemoveEmptyEntries).ToList();

			foreach (var word in wordList)
			{
				string trimmedWord = word.Trim().ToLower();
				if (_wordRegex.IsMatch(trimmedWord))
				{
					if (_dictionary.ContainsKey(trimmedWord))
						_dictionary[trimmedWord]++;
					else
						_dictionary.Add(trimmedWord, 1);
				}
			}
		}

		public string Correct(string word)
		{
			if (string.IsNullOrEmpty(word))
				return word;

			word = word.ToLower();

			// known()
			if (_dictionary.ContainsKey(word))
				return word;

			List<String> list = Edits(word);
			Dictionary<string, int> candidates = new Dictionary<string, int>();

			foreach (string wordVariation in list)
			{
				if (_dictionary.ContainsKey(wordVariation) && !candidates.ContainsKey(wordVariation))
					candidates.Add(wordVariation, _dictionary[wordVariation]);
			}

			if (candidates.Count > 0)
				return candidates.OrderByDescending(x => x.Value).First().Key;

			// known_edits2()
			foreach (string item in list)
			{
				foreach (string wordVariation in Edits(item))
				{
					if (_dictionary.ContainsKey(wordVariation) && !candidates.ContainsKey(wordVariation))
						candidates.Add(wordVariation, _dictionary[wordVariation]);
				}
			}

			return (candidates.Count > 0) ? candidates.OrderByDescending(x => x.Value).First().Key : word;
		}

		private List<string> Edits(string word)
		{
			var splits = new List<Tuple<string, string>>();
			var transposes = new List<string>();
			var deletes = new List<string>();
			var replaces = new List<string>();
			var inserts = new List<string>();

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
					deletes.Add(a + b.Substring(1));
				}
			}

			// Transposes
			for (int i = 0; i < splits.Count; i++)
			{
				string a = splits[i].Item1;
				string b = splits[i].Item2;
				if (b.Length > 1)
				{
					transposes.Add(a + b[1] + b[0] + b.Substring(2));
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
						replaces.Add(a + c + b.Substring(1));
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
					inserts.Add(a + c + b);
				}
			}

			return deletes.Union(transposes).Union(replaces).Union(inserts).ToList();
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
}
