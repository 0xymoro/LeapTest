using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;

public class BallCollisionTest : MonoBehaviour {

	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {

	}

/*
	private bool IsHand(Collider other)
	{
	  if (other.transform.parent && other.transform.parent.parent && other.transform.parent.parent.GetComponent<HandModel>())
	    return true;
	  else
	    return false;
	}
*/
	void OnCollisionEnter(Collision other)
	{
		Debug.Log("Here");
	  // if (IsHand(other))
	  // {
	  //   Debug.Log("Yay! A hand collided!");
	  // }
	}

}
