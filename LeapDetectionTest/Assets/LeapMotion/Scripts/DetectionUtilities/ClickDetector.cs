using UnityEngine;
using System.Collections;
using System;
using Leap.Unity.Attributes;

namespace Leap.Unity {

  /**
	 * Detects clicks (curled fingers)
   *
   * @since 4.1.2
   */
  public class ClickDetector : Detector {

		//Threshold for what constitutes a click vs a release
		[Tooltip("Angle that once passed, is regarded as a click")]
		[Units("radians")]
		[MinValue(0)]
		public double ClickThreshold = Math.PI*2/5;

		//Threshold for what constitutes a release after click
		[Tooltip("Angle that once passed, is regarded as a release")]
		[Units("radians")]
		[MinValue(0)]
		public double ReleaseThreshold = Math.PI*1/5;


    /**
     * The interval at which to check finger state.
     * @since 4.1.2
     */
    [Tooltip("The interval in seconds at which to check this detector's conditions.")]
    [Units("seconds")]
    [MinValue(0)]
    public float Period = .1f; //seconds

    /**
     * The IHandModel instance to observe.
     * Set automatically if not explicitly set in the editor.
     * @since 4.1.2
     */
    [AutoFind(AutoFindLocations.Parents)]
    [Tooltip("The hand model to watch. Set automatically if detector is on a hand.")]
    public IHandModel HandModel = null;


    /** How many fingers must be extended for the detector to activate. */
    [Header("Min and Max Finger Counts")]
    [Range(0,5)]
    [Tooltip("The minimum number of fingers extended.")]
    public int MinimumExtendedCount = 0;
    /** The most fingers allowed to be extended for the detector to activate. */
    [Range(0, 5)]
    [Tooltip("The maximum number of fingers extended.")]
    public int MaximumExtendedCount = 5;


		//Tells if finger is held
		private bool isHolding = false;
		public void setIsHolding(bool newVal){
			isHolding = newVal;
		}
		public bool getIsHolding(){
			return isHolding;
		}

		//tells if click has been registered once or not
		private bool registered = false;
		public void setRegistered(bool newVal){
			registered = newVal;
		}
		public bool getRegistered(){
			return registered;
		}

		//knows which finger clicked
		private int fingerClicked = -1;
		public void setFingerClicked(int newVal){
			fingerClicked = newVal;
		}
		public int getFingerClicked(){
			return fingerClicked;
		}

		//keeps track of whether finger is clicked or not
		private bool[] fingersClicked = {false, false, false, false, false};


    private IEnumerator watcherCoroutine;


    void Awake () {
      watcherCoroutine = extendedFingerWatcher();
    }

    void OnEnable () {
      StartCoroutine(watcherCoroutine);
    }

    void OnDisable () {
      StopCoroutine(watcherCoroutine);
      Deactivate();
    }

    IEnumerator extendedFingerWatcher() {
      Hand hand;
      while(true){
        bool fingerState = false;
        if(HandModel != null && HandModel.IsTracked){
          hand = HandModel.GetLeapHand();
          if(hand != null){

						//update all arrays on new hand position
						updateFingersClicked(hand);

            int extendedCount = 0;
            for (int f = 0; f < 5; f++) {
              if (!this.fingersClicked[f]) { //if is not clicked
                extendedCount++;
              }
							else{ //not extended, meaning clicked
								fingerClicked = f;
							}
            }
            fingerState = (extendedCount <= MaximumExtendedCount) &&
                          (extendedCount >= MinimumExtendedCount);
            if(HandModel.IsTracked && fingerState){
              Activate();
							//set the registered to false for first signal of input
							//so as to not set it again while click is held
							if (this.getIsHolding() == false){
								this.setIsHolding(true);
								this.setRegistered(false);
							}
            } else if(!HandModel.IsTracked || !fingerState) {
              Deactivate();
							this.setIsHolding(false);
            }
          }
        } else if(IsActive){
          Deactivate();
					this.setIsHolding(false);
        }
        yield return new WaitForSeconds(Period);
      }
    }

		private void updateFingersClicked(Hand hand){

			//TODO thumb special case due to differing anatomy in clicking than other 4


			for (int i = 1; i < 5; i++){ //for 4 fingers other than thumb
				double angleDifference = hand.Fingers[i].Direction.AngleTo(hand.Direction);
				if (this.fingersClicked[i]){ //if is clicked, check for release
					if (angleDifference < ReleaseThreshold){
						this.fingersClicked[i] = false;
					}
				}
				else{ //if not clicked, check for click
					if (angleDifference > ClickThreshold){
						this.fingersClicked[i] = true;
					}
				}
			}
		}


  }


  /** Defines the settings for comparing extended finger states */
	//No need due to definition in ExtendedFingerDetector, TODO add this
	//when not using Leap's own library
  //public enum PointingState{Extended, NotExtended, Either}
}
