using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Replay;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Replay {
    public class ReplayManager : MonoBehaviour {
        public static readonly bool useBinaryMode = true;

        /// <summary>
        /// Current replay data being recorded to or replaying from depending on game connection type (Replay = replaying, non replay = recording)
        /// </summary>
        public ReplayData replayData {get; private set;}

        /// <summary>
        /// Events that are being recorded
        /// </summary>
        private List<Replayable> events;
        private List<float> eventTiming;


        private bool replaying = false;
        private int eventIndex = 0;


        [SerializeField] private InputActionReference fastForwardInputReference;
        bool fastForwarding;


        void Awake() {
            if (!BattleManager.Instance) {
                Debug.LogError("Replay manager needs a battlemanager to record!");
                return;
            }

            BattleManager.Instance.onBattleInitialized += OnBattleInitialized;

            
        }

        void OnEnable() {
            fastForwardInputReference.action.started += StartFastForward;
            // fastForwardInputReference.action.performed += StopFastForward;
            fastForwardInputReference.action.canceled += StopFastForward;
        }

        void OnDisable() {
            fastForwardInputReference.action.started -= StartFastForward;
            // fastForwardInputReference.action.performed -= StopFastForward;
            fastForwardInputReference.action.canceled -= StopFastForward;
        }

        void Update() {
            if (!replaying) return;

            if (replayData == null) return;

            // check if the current event should be played, don't go past the bounds of the events/event timing array
            while (eventIndex < replayData.eventTiming.Length && BattleManager.Instance.battleTime >= replayData.eventTiming[eventIndex]) {
                var ev = replayData.events[eventIndex];
                ev.Replay(BattleManager.Instance);
                eventIndex++;
            }

            // stop replaying when the end of the list of events is reached.
            if (eventIndex >= replayData.eventTiming.Length) {
                replaying = false;
                return;
            }
        }

        void StartFastForward(InputAction.CallbackContext ctx) {
            Debug.Log("Fast forward started");
            fastForwarding = true;
            BattleManager.Instance.SetBattleTimeScale(8f);
        }

        void StopFastForward(InputAction.CallbackContext ctx) {
            Debug.Log("Fast forward cancelled");
            fastForwarding = false;
            BattleManager.Instance.SetBattleTimeScale(1f);
        }

        public void OnBattleInitialized() {
            if (GameManager.Instance.currentConnectionType != GameManager.GameConnectionType.Replay) {
                StartRecordingEvents();
            } else {
                // start replaying from replayData which was loaded from file
                replaying = true;
            }
        }

        /// <summary>
        /// Setup battle players and battle data and Start recording events. 
        /// </summary>
        public void StartRecordingEvents() {
            replayData = new ReplayData(GameManager.Instance);

            events = new List<Replayable>();
            eventTiming = new List<float>();

            replayData.battleData = GameManager.Instance.battleData;
            replayData.replayPlayers = new ReplayPlayer[GameManager.Instance.playerManager.players.Count];

            for (int i = 0; i < GameManager.Instance.playerManager.players.Count; i++) {
                replayData.replayPlayers[i] = new ReplayPlayer();
                
                Player player = GameManager.Instance.playerManager.players[i];
                if (player) {
                    if (player.battler) replayData.replayPlayers[i].battlerId = player.battler.battlerId;
                    replayData.replayPlayers[i].isCpu = player.isCpu;
                    replayData.replayPlayers[i].username = player.username.Value.ToString();
                }

                Board board = BattleManager.Instance.GetBoardByIndex(i);

                board.pieceManager.onPieceMoved += OnPieceMoved;
                board.pieceManager.onPiecePlaced += OnPiecePlaced;

                board.spellcastManager.onSpellcastStarted += OnSpellcastStarted;
                board.spellcastManager.onSpellcastClear += OnSpellcastClear;
                board.spellcastManager.onCascadeEnded += OnChainEnded;
                board.spellcastManager.onChainEnded += OnChainEnded;
            }

            BattleManager.Instance.onBattleEnded += SaveRecording;

            Debug.Log("Recording battle events!");
        }

        public void AddEvent(Replayable ev) {
            eventTiming.Add(BattleManager.Instance.battleTime);
            events.Add(ev);
        }

        public void SaveRecording() {
            replayData.events = events.ToArray();
            replayData.eventTiming = eventTiming.ToArray();
            SaveToFile();
        }

        public void SaveToFile(string path = "replay.data") {
            string fullPath = Path.Combine(Application.persistentDataPath, path);
            if (useBinaryMode) {
                BinaryFormatter formatter = new BinaryFormatter();
                using (FileStream stream = new FileStream(fullPath, FileMode.Create))
                {
                    formatter.Serialize(stream, replayData);
                }
            } else {
                string json = JsonUtility.ToJson(replayData, true); // Convert to JSON string
                File.WriteAllText(fullPath, json);
            }

            Debug.Log("Saved replay data to "+fullPath);
        }

        public void LoadFromFile(string path = "replay.data") {
            string fullPath = Path.Combine(Application.persistentDataPath, path);

            if (useBinaryMode) {
                BinaryFormatter formatter = new BinaryFormatter();
                using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                {
                    replayData = (ReplayData)formatter.Deserialize(stream);
                }
            } else {
                if (File.Exists(fullPath))
                {
                    string json = File.ReadAllText(fullPath);
                    replayData = JsonUtility.FromJson<ReplayData>(json);
                }
                else
                {
                    Debug.LogError("File not found!");
                    return;
                }
            }

            Debug.Log($"Loaded replay data from: {fullPath}");
        }


        [System.Serializable]
        public struct PieceMoveEvent : Replayable {
            private int boardIndex;
            private int x;
            private int y;
            private int rotation;

            public PieceMoveEvent(int boardIndex, Vector2Int position, int rotation) {
                this.boardIndex = boardIndex;
                this.x = position.x;
                this.y = position.y;
                this.rotation = rotation;
            }
            public void Replay(BattleManager battleManager)
            {
                battleManager.GetBoardByIndex(boardIndex).pieceManager.UpdateCurrentPiece(new Vector2Int(x,y), rotation);
            }
        }
        public void OnPieceMoved(Board board) {
            var piece = board.pieceManager.currentPiece;
            AddEvent(new PieceMoveEvent(board.boardIndex, piece.position, piece.rotation));
        }


        [System.Serializable]
        public struct PiecePlaceEvent : Replayable {
            private int boardIndex;

            public PiecePlaceEvent(int boardIndex) {
                this.boardIndex = boardIndex;
            }

            public void Replay(BattleManager battleManager)
            {
                battleManager.GetBoardByIndex(boardIndex).pieceManager.PlaceCurrentPiece();
            }
        }
        public void OnPiecePlaced(Board board) {
            AddEvent(new PiecePlaceEvent(board.boardIndex));
        }


        [System.Serializable]
        public struct SpellcastEvent : Replayable {
            public enum SpellcastEventType {
                START,
                CLEAR,
                CASCADE_END,
                CHAIN_END
            }

            private int boardIndex;
            private SpellcastEventType eventType;

            public SpellcastEvent(int boardIndex, SpellcastEventType eventType) {
                this.boardIndex = boardIndex;
                this.eventType = eventType;
            }

            public void Replay(BattleManager battleManager)
            {
                Board board = battleManager.GetBoardByIndex(boardIndex);

                switch (eventType) {
                    case SpellcastEventType.START:
                        board.spellcastManager.StartSpellcast();
                        break;
                    case SpellcastEventType.CLEAR:
                        board.spellcastManager.SpellcastClear();
                        break;
                    case SpellcastEventType.CASCADE_END:
                        board.spellcastManager.EndCascade();
                        break;
                    case SpellcastEventType.CHAIN_END:
                        board.spellcastManager.EndChain();
                        break;
                }
            }
        }

        public void OnSpellcastStarted(Board board) {
            AddEvent(new SpellcastEvent(board.boardIndex, SpellcastEvent.SpellcastEventType.START));
        }
        public void OnSpellcastClear(Board board) {
            AddEvent(new SpellcastEvent(board.boardIndex, SpellcastEvent.SpellcastEventType.CLEAR));
        }
        public void OnCascadeEnded(Board board) {
            AddEvent(new SpellcastEvent(board.boardIndex, SpellcastEvent.SpellcastEventType.CASCADE_END));
        }
        public void OnChainEnded(Board board) {
            AddEvent(new SpellcastEvent(board.boardIndex, SpellcastEvent.SpellcastEventType.CHAIN_END));
        }
    }
}