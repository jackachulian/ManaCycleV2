using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Replay {
    public class ReplayData {
        public BattleData battleData;
        public ReplayPlayer[] replayPlayers;

        public ReplayPieceEvent[] pieceEvents;
        public ReplaySpellcastEvent[] spellcastEvents;

        public ReplayData(GameManager gameManager) {
            battleData = GameManager.Instance.battleData;

            replayPlayers = new ReplayPlayer[gameManager.playerManager.players.Count];
            for (int i = 0; i < replayPlayers.Length; i++) {
                Player player = gameManager.playerManager.players[i];
                replayPlayers[i] = new ReplayPlayer();
                replayPlayers[i].battlerId = player.battler.battlerId;
            }
        }
    }


    [System.Serializable]
    public class ReplayPlayer {
        public string battlerId;
    }

    public enum SpellcastEventType {
        START,
        CLEAR,
        CASCADE_END,
        CHAIN_END
    }

    [System.Serializable]
    public class ReplaySpellcastEvent {
        
        public SpellcastEventType eventType;

        /// <summary>
        /// The board that this event occurred on - the index of the board that will be passed to Evaluate()
        /// </summary>
        public int boardIndex;

        /// <summary>
        /// Time in seconds since the match began when this action occurred
        /// </summary>
        public float time;

        public ReplaySpellcastEvent(SpellcastEventType eventType, int boardIndex) {
            this.eventType = eventType;
            this.boardIndex = boardIndex;
        }
    }


    public enum PieceEventType {
        MOVE,
        PLACE
    }

    [System.Serializable]
    public class ReplayPieceEvent {
        
        public PieceEventType eventType;

        /// <summary>
        /// The board that this event occurred on - the index of the board that will be passed to Evaluate()
        /// </summary>
        public int boardIndex;

        /// <summary>
        /// Time in seconds since the match began when this action occurred
        /// </summary>
        public float time;

        // new orientation of the piece
        public Vector2Int position;
        public int rotation;

        public ReplayPieceEvent(PieceEventType eventType, int boardIndex, Vector2Int position, int rotation) {
            this.eventType = eventType;
            this.boardIndex = boardIndex;
            this.position = position;
            this.rotation = rotation;

            this.time = BattleManager.Instance.battleTime;
        }
    }
}