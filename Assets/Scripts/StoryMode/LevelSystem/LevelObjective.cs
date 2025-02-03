using System;
using UnityEngine;

namespace LevelSystem.Objectives {
    [System.Serializable]
    public abstract class LevelObjective {
        /// <summary>
        /// Returns the string that shows in the level details menu in level select.
        /// </summary>
        /// <returns></returns>
        public abstract string GetDetailsString();

        /// <summary>
        /// Returns the string that will show in the battle objectives list.
        /// </summary>
        /// <param name="board"></param>
        /// <returns></returns>
        public abstract string GetProgressString(Board board);
        
        /// <summary>
        /// Get the completion progress, as a percentage.
        /// If returns a number 1 or greater, this objective's condition has been fulfilled.
        /// </summary>
        /// <param name="board"></param>
        /// <returns></returns>
        public abstract float GetProgress(Board board);

        /// <summary>
        /// Called by the objectives list. Will let objectives listen to the board to know when they should be updated visually.
        /// </summary>
        public abstract void ListenToBoard(Board board);
        public abstract void StopListeningToBoard(Board board);

        /// <summary>
        /// Should be invoked whenever this objective's text / fulfillment condition changes.
        /// </summary>
        public event Action onUpdated; 

        public void NotifyConditionUpdated() {
            onUpdated?.Invoke();
        }
    }
}