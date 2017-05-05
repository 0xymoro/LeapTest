using UnityEngine;
using System.Collections;
using System;
using Leap.Unity.Attributes;

namespace Leap.Unity {

	public class KeyCollisionDetector : MonoBehaviour {
		// [Tooltip("Key's ID")]
		// public int KeyID = 0;

		[Tooltip("The value associated with the key")]
		public string KeyValue = "";

		[Tooltip("The value associated with the key's shift")]
		public string KeyShiftValue = "";

		[Tooltip("Delay of input after each key is pressed")]
		public float KeyDelay = 0.2f;

		public bool isNormalKey = true;
		public bool isCapsKey = false;
		public bool isShiftKey = false;
		public bool isEnterKey = false;
		public bool isDeleteKey = false;


		private KeyCollisionDetectorController controller;



		void Awake (){
			//BE VERY SPECIFIC WITH THE PATH HERE!!!
			controller = GameObject.Find("/KeysController").GetComponent<KeyCollisionDetectorController>();
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

		private bool IsFingerTip(Collider other)
		{
			return other.transform.name == "bone3";
		}


		//OnCollisionStay as it needs to keep track of all fingers that might
		//be touching the key in order to find the one that's bent (clicked)
		void OnCollisionEnter(Collision other){
			Collider collisionObject = other.gameObject.GetComponent<Collider>();
			if (IsHand(collisionObject) && IsFingerTip(collisionObject) &&
			    !controller.getActive()){
				registerKey();
				controller.activate();
				StartCoroutine(DeactivateAfterDelay());
			}
		}


		IEnumerator DeactivateAfterDelay(){
			yield return new WaitForSeconds(KeyDelay);
			controller.deactivate();
		}

		void registerKey(){
			if (isNormalKey){
				controller.outputText(KeyValue, KeyShiftValue);
			}
			else if (isCapsKey){
				controller.setCaps(!controller.getCaps());
			}
			else if (isShiftKey){
				controller.setShift(!controller.getShift());
			}
			else if (isEnterKey){
				controller.enterKey();
			}
			else if (isDeleteKey){
				controller.deleteKey();
			}
		}


	}
}
