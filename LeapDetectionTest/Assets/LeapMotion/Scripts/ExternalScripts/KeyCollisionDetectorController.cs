using UnityEngine;
using System.Collections;
using System;
using Leap.Unity.Attributes;

namespace Leap.Unity {

	public class KeyCollisionDetectorController : MonoBehaviour {

		[Tooltip("Drag the display text to here")]
		public TextMesh OutputTextMesh;

		private int textLength = 0;
		private static int NEWLINE_THRESHOLD = 15;

		private bool isCaps = false;
		private bool isShift = false;
		private bool isActive = false;
		//private int activeID = -1;




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
				OutputTextMesh.text += KeyValue;
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
