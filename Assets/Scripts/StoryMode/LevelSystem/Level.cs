using UnityEngine;

[CreateAssetMenu(fileName = "Level", menuName = "ManaCycle/Level")]
public class Level : ScriptableObject {
    /// <summary>
    /// Used to identify the level in save data
    /// </summary>
    public string levelId = "world1_level1";

    /// <summary>
    /// Shown in the level UI to show this level's number in the sequence of levels and show the "world" (Map area) it is a part of
    /// </summary>
    public string levelNumber = "1-1";

    /// <summary>
    /// display name (TODO: localize)
    /// </summary>
    public string displayName = "Level";

    /// <summary>
    /// Description shown on the level details window before starting the level (TODO: localize)
    /// </summary>
    public string description = "This is a level";

    [System.Serializable]
    public class LevelPlayer {
        /// <summary>
        /// Battler that this player can use. 
        /// If list has more than one battler, player can choose from that list. 
        /// If list is empty, player can choose any battler.
        /// </summary>
        [Tooltip("Battlers that this player can use. \nIf list has more than one battler, player can choose from that list. \nIf list is empty, player can choose any battler.")]
        public Battler[] battlers;

        /// <summary>
        /// If this battler's unique spell can be used in this battle.
        /// </summary>
        [Tooltip("If this battler's unique spell can be used in this battle.")]
        public bool uniqueSpellEnabled = true;

        /// <summary>
        /// If list is not empty, this player will have these spells auto-equipped and cannot be swapped out.
        /// </summary>
        [Tooltip("If list is not empty, this player will have these spells auto-equipped and cannot be swapped out.")]
        public Spell[] fixedSpells;

        /// <summary>
        /// If this is a CPU, how fast the CPU acts.
        /// </summary>
        [Tooltip("If this is a CPU, the amount of decisions it makes per second.")]
        public float cpuSpeed = 3;
    }

    /// <summary>
    /// Each player in this battle. Index 0 is the client's player, other indexes are CPU opponents that are added to the battle.
    /// </summary>
    [Tooltip("Each player in this battle. Index 0 is the client's player, other indexes are CPU opponents that are added to the battle.")]
    public LevelPlayer[] levelPlayers;

    /// <summary>
    /// The player has this many open spell slots to equip whatever spell they want. 
    /// </summary>
    [Tooltip("Client-controlled player has this many open slots to equip whatever spells they want. The last player-selected spells will be auto-filled in these slots at the start of battle setup.")]
    public int playerOpenSpellSlots = 2;

    /// <summary>
    /// Amount of seconds the player has to beat this level in. If 0, time is unlimited.
    /// </summary>
    [Tooltip("Amount of seconds the player has to beat this level in. If 0, time is unlimited.")]
    public int timeLimit = 180;

    /// <summary>
    /// Contains battle-specific non-player-specific data used in the battle scene such as the seed selected, cycle lengths, etc.
    /// </summary>
    public BattleData battleData = new BattleData();

    /// <summary>
    /// Call this to play the battle associated with this level.
    /// Will set up a singleplayer game with this level's properties.
    /// If player setup needs to happen, will go to charselect scene.
    /// If not, will go straight into battle scene.
    /// </summary>
    public void StartLevelBattle() {
        GameManager.Instance.SetLevel(this);
        GameManager.Instance.SetBattleData(battleData);

        GameManager.Instance.StartGameHost(GameManager.GameConnectionType.Singleplayer);

        // SetupBattlers();  will be called by SingleplayerConnectionManager as the game starts

        if (IsChoiceRequired()) {
            TransitionManager.Instance.TransitionToScene("CharSelect");
        } else {
            TransitionManager.Instance.TransitionToScene("Battle");
        }
    }

    
    /// <summary>
    /// Setup the levels for a battler.
    /// Is called by SingleplayerConnectionManager when it starts listening while a level is being played.
    /// </summary>
    /// <param name="playerPrefab">player prefab to use to spawn opponent players</param>
    /// <returns>true if the player needs to choose anything. 
    /// If true upon selecting a level, will open charselect. If false, will go straight to battle.</returns>
    public bool SetupBattlers() {
        bool choiceRequired = false;

        for (int i = 0; i < levelPlayers.Length; i++) {
            LevelPlayer levelPlayer = levelPlayers[i];

            Player player;
            if (i == 0) {
                player = GameManager.Instance.playerManager.players[0];
            } else {
                // spawn the battler if non index 0
                player = GameManager.Instance.playerManager.AddCPUPlayer();
            }

            if (levelPlayer.battlers.Length == 1) {
                // set the battler to this
                player.battler = levelPlayer.battlers[0];
            } else if (levelPlayer.battlers.Length > 1) {
                // TODO: make only the players in this list selectable in the char select.
                if (i == 0) choiceRequired = true;
            } else {
                // if no battlers, player can select any battler
                if (i == 0) choiceRequired = true;
            }
        }

        return choiceRequired;
    }

    /// <summary>
    /// Will return true if the player has to choose anything (battler, spell loadout, etc).
    /// If true, playing this level's battle will go to CharSelect. If false, go straight to battle scene.
    /// </summary>
    public bool IsChoiceRequired() {
        if (levelPlayers.Length == 0) return false;

        LevelPlayer levelPlayer = levelPlayers[0];

        // if the length is exactly one, can only be the one battler.
        // (empty list means any battler, and more than one means any of the battlers in the list.)
        if (levelPlayer.battlers.Length != 1) {
            return true;
        }
            
        return false;
    }
}