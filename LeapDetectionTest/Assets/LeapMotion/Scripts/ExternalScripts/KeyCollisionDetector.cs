using UnityEngine;
using System.Collections;
using System;
using Leap.Unity.Attributes;

namespace Leap.Unity {

	public class KeyCollisionDetector : MonoBehaviour {
		[Tooltip("The value associated with the key")]
		public string KeyValue = " ";

		[Tooltip("Drag the display text to here")]
		public TextMesh OutputTextMesh;
		private int textLength = 0;
		private static int NEWLINE_THRESHOLD = 30;


		private ClickDetector leftClickDetector;
		private ClickDetector rightClickDetector;


		void Awake (){
			//BE VERY SPECIFIC WITH THE PATH HERE!!!
			leftClickDetector = GameObject.Find("/LMHeadMountedRig/HandModels/CapsuleHand_L/LeftDetector").GetComponent<ClickDetector>();
			rightClickDetector = GameObject.Find("/LMHeadMountedRig/HandModels/CapsuleHand_R/RightDetector").GetComponent<ClickDetector>();
		}

		// Use this for initialization
		void Start () {

		}

		// Update is called once per frame
		void Update () {

		}

		private bool IsHand(Collider other)
		{
		  if (other.transform.parent && other.transform.parent.parent &&
			other.transform.parent.parent.GetComponent<HandModel>())
		    return true;
		  else
		    return false;
		}

		private int whichFinger(Collider hand){
			string fingerName = hand.transform.parent.name;
			if (fingerName == "thumb"){
				return 0;
			}
			else if (fingerName == "index"){
				return 1;
			}
			else if (fingerName == "middle"){
				return 2;
			}
			else if (fingerName == "ring"){
				return 3;
			}
			else if (fingerName == "pinky"){
				return 4;
			}
			else return -1;
		}

		private int whichHand(Collider hand){
			string handName = hand.transform.parent.parent.name;
			if (handName == "RigidRoundHand_L(Clone)"){
				return 0; //left hand
			}
			else if (handName == "RigidRoundHand_R(Clone)"){
				return 1; //right hand
			}
			return -1;
		}

		//OnCollisionStay as it needs to keep track of all fingers that might
		//be touching the key in order to find the one that's bent (clicked)
		void OnCollisionStay(Collision other){
			Collider collisionObject = other.gameObject.GetComponent<Collider>();
			if (IsHand(collisionObject)){
				int fingerID = whichFinger(collisionObject);
				int handID = whichHand(collisionObject);

				if (handID == 0){
					//left hand clicked
					if (leftClickDetector.getFingerClicked() == fingerID &&
							leftClickDetector.getRegistered() == false){
						outputText(KeyValue); //TODO
						//Debug.Log(KeyValue);
						leftClickDetector.setRegistered(true);
					}
				}
				else{
					//right hand clicked
					if (rightClickDetector.getFingerClicked() == fingerID &&
							rightClickDetector.getRegistered() == false){
						outputText(KeyValue); //TODO
						rightClickDetector.setRegistered(true);
					}
				}
			}
		}

		//outputs the character/string to the text shown to user
		void outputText(string keyValue){
			if (textLength >= NEWLINE_THRESHOLD){
				textLength = 0;
				OutputTextMesh.text += "\n";
			}
			OutputTextMesh.text += keyValue;
			textLength ++;
		}

	}
}
