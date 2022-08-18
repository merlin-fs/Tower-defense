using UnityEngine;
using System.Collections;

/** 
 * Example MonoBehaviour. Put it to a GameObject and give it some coordinates with startTweenTo();
 */
public class Example : MonoBehaviour
{
	private Tweener tweener = new Tweener();

	public void startTweenTo( Vector3 to )
	{
		if(tweener!=null)
		{
			// Sets The Position From -> To
			tweener.EaseFromTo( transform.localPosition, to, 1f, Easing.BackEaseInOut, tweenComplete);
		}
	}

	private void tweenComplete()
	{
		Debug.Log( "Tween Completed" );
	}

	void Update ()
	{
		if( tweener!=null && tweener.Animating )
		{
			// Updates the Tweener
			tweener.Update(Time.deltaTime);

			// Set the Position
			transform.localPosition = tweener.Progression;

			Debug.Log( "Progression Percentage: "+tweener.ProgressPct );
			Debug.Log( "Progression Position: "+tweener.Progression);
		}
		else
		{
			// Tween not init or its completed
		}
	}
}
