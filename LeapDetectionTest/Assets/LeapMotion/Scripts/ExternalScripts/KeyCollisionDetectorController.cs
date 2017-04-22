using UnityEngine;
using System.Collections;
using System;
using Leap.Unity.Attributes;

namespace Leap.Unity {

	public class KeyCollisionDetectorController : MonoBehaviour {



		private bool isCaps = false;
		private bool isShift = false;



		public bool getCaps(){ return isCaps; }
		public void setCaps(bool newVal){ isCaps = newVal; }

		public bool getShift(){ return isShift; }
		public void setShift(bool newVal){ isShift = newVal; }

	}
}
