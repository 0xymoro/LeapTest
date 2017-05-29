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
        public Spelling spelling;



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
					String[] spl= OutputTextMesh.text.Split(new char[] {' ', '\n'}); //splits output string by spaces
					OutputTextMesh.text = OutputTextMesh.text.Substring(0, OutputTextMesh.text.Length - spl[spl.Length - 1].Length); //eliminates last word
                                                                                                                                     //Debug.Log(OutputTextMesh.text);
                    string lastWord = ">";
                    if (spl.Length > 1)
                    {
                        lastWord = spl[spl.Length - 2];
                    }
					OutputTextMesh.text += spelling.Correct(spl[spl.Length - 1], lastWord); //adds in corrected last word
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
            if (temp.Length == 0) return;
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
