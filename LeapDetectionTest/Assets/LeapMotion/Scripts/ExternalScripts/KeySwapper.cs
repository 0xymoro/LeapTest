using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Leap.Unity;

public class KeySwapper : MonoBehaviour {
	private bool status;
	List<string> vals;
	List<string> shiftVals;

	Dictionary<string, string> maps = new Dictionary<string, string>	{
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

	void Awake(){
		vals = new List<string> ();
		shiftVals = new List<string> ();
		status = false;
	}


	//Important precondition: child 0 is the Text, child 1 is the Key.
	void Swap () {
		if (!status) {
			foreach (Transform child in transform) {
				TextMesh textMesh = child.GetChild (0).GetChild (0).GetComponent<TextMesh> ();
				KeyCollisionDetector detector = child.GetChild(0).GetChild(1).GetComponent<KeyCollisionDetector>();
				vals.Add (detector.KeyValue);
				shiftVals.Add(detector.KeyShiftValue);
				if (maps.ContainsKey (textMesh.text)) {
					string prevText = textMesh.text;
					textMesh.text = maps [prevText];
					detector.KeyValue = maps [prevText];
					//shift has no effect in alternate keyboard, so same key value
					detector.KeyShiftValue = detector.KeyValue;
				}
			}
			status = true;
		} else {
			//The two lists keep track of all the keys in the order they
			//were replaced, so this goes and reverses replacement the same way
			int i = 0;
			foreach (Transform child in transform) {
				TextMesh textMesh = child.GetChild (0).GetChild (0).GetComponent<TextMesh> ();
				KeyCollisionDetector detector = child.GetChild(0).GetChild(1).GetComponent<KeyCollisionDetector>();
				if(detector.isNormalKey){ //ignores special keys (enter, shift, ...)
					textMesh.text = shiftVals [i];
					detector.KeyValue = vals [i];
					detector.KeyShiftValue = shiftVals [i];
				}
				i++;
			}
			status = false;
		}
	}

}
