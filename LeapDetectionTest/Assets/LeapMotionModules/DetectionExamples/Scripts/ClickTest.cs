using UnityEngine;
using System.Collections.Generic;

namespace Leap.Unity.DetectionExamples {

  public class ClickTest : MonoBehaviour {

    [Tooltip("Each pinch detector can draw one line at a time.")]
    [SerializeField]
    private ClickDetector[] _clickDetectors;


    void Awake() {
      if (_clickDetectors.Length == 0) {
        Debug.LogWarning("ERROR clickDetector's length is 0.");
      }
    }

    void Start() {

    }

    void Update() {
      for (int i = 0; i < _clickDetectors.Length; i++) {
        var detector = _clickDetectors[i];

        if (detector.IsActive && !detector.getRegistered()) {
          //Testing if clicks can be registered
          Debug.Log(i + " " + detector.getFingerClicked());
          //don't log anymore if already logged
          detector.setRegistered(true);
          //drawState.BeginNewLine();
        }
      }
    }
  }
}
