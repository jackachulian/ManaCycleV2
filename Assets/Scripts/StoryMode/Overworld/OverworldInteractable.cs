using System.Linq;
using System;
using UnityEngine;

namespace StoryMode.Overworld
{
    public abstract class OverworldInteractable : MonoBehaviour
    {
        public string interactableName;
        public bool isFastTravelPoint = true;
        public OverworldInteractable[] adjacentPoints;


        public abstract void OnInteract(OverworldPlayerInteracter interacter);
        public virtual void OnInteractionRangeEntered() {}
        public virtual void OnInteractionRangeExited() {}

        /// <summary>
        /// Gets the closest interactable to this one in the given target direction.
        /// </summary>
        /// <param name="aimDir">Target direction.</param>
        /// <param name="maxAngle">Max angle the found interactable should be at. Will return null if none found.</param>
        /// <returns></returns>
        public OverworldInteractable GetAdjacentInDir(Vector3 aimDir, float maxAngle = Mathf.Infinity)
        {
            if (adjacentPoints.Length == 0) Debug.LogError("This interactable has no adjacent points!");

            // get angle of given direction
            float aimAng = Vector3.SignedAngle(Vector3.right, aimDir, Vector3.up);

            // set initial values
            OverworldInteractable closest = adjacentPoints[0];        
            float smallestTheta = Math.Abs(aimAng - Vector3.SignedAngle(
                Vector3.right,
                closest.transform.position - transform.position, 
                Vector3.up
            ));

            // loop over all serialized points and get the one with the smallest angle
            foreach (OverworldInteractable oi in adjacentPoints.Skip(0))
            {
                float theta = Math.Abs(aimAng - Vector3.Angle(Vector3.right, oi.transform.position - transform.position));
                if (theta < smallestTheta)
                {
                    closest = oi;
                    smallestTheta = theta;
                }
            }

            // Debug.Log(aimAng + ", " + Vector3.Angle(Vector3.right, closest.transform.position - transform.position));
            return (smallestTheta < maxAngle) ? closest : null;
        }
    }
}