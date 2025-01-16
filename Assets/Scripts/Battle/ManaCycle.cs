using System.Linq;
using UnityEngine;

public class ManaCycle : MonoBehaviour
{  
    /// <summary>
    /// Extra scale applied to mana tile objects in the cycle;
    /// </summary>
    [SerializeField] private float manaScale = 1.25f;

    /// <summary>
    /// Visual distance between mana tiles (units)
    /// </summary>
    [SerializeField] private float manaSeparation = 1.5f;

    /// <summary>
    /// Transform that the mana tiles in the cycle are parented to
    /// </summary>
    [SerializeField] private Transform manaTileTransform;

    /// <summary>
    /// All visual Tiles that are displayed on the display the cycle
    /// </summary>
    public ManaTile[] tiles {get; private set;}

    /// <summary>
    /// The cycle randonly generated upon battle initialization.
    /// </summary>
    private int[] colorSequence;

    /// <summary>
    /// Is called by the BattleManager, after the BattleManager initializes and before the Boards initialize.
    /// </summary>
    public void InitializeBattle(BattleManager battleManager) {
        foreach (Transform child in manaTileTransform) {
            Destroy(child.gameObject);
        }

        var battleData = GameManager.Instance.battleData;

        // Seed is used to determine the cycle
        Random.InitState(battleData.seed);
        colorSequence = GenerateCycleColorSequence(battleData.cycleLength, battleData.cycleUniqueColors);

        tiles = new ManaTile[battleData.cycleLength];

        Debug.Log(string.Join(", ", colorSequence));

        // Create cycle color objects for each cycle color
        for (int i=0; i<battleData.cycleLength; i++)
        {
            ManaTile tile = battleManager.SpawnTile();
            tiles[i] = tile;
            tile.SetColor(colorSequence[i], false, false, battleManager.cosmetics);
            tile.transform.SetParent(manaTileTransform);
            tile.transform.localPosition = new Vector2(0, (i - (battleData.cycleLength-1)/2.0f) * -manaSeparation);
            tile.transform.localScale = new Vector2(manaScale, manaScale);
        }
    }

    // Returns the color of the sequence at the given index.
    public int GetSequenceColor(int index) {
        return colorSequence[index];
    }

    /// <summary>
    /// Get a visual tile that is part of the cycle list of colors.
    /// </summary>
    /// <returns>the tile GameObject for the given cycle index</returns>
    public ManaTile GetCycleTile(int index) {
        return tiles[index];
    }

    /// <summary>
    /// (Static) Generate a cycle color sequence, expressed as an array of integers, with the given length and amount of unique colors.
    /// </summary>
    /// <param name="cycleLength">total length of the color sequence</param>
    /// <param name="cycleUniqueColors">total amount of unique colors</param>
    public static int[] GenerateCycleColorSequence(int cycleLength, int cycleUniqueColors) {
        int[] colorSequence = new int[cycleLength];

        // Add one of each color to the list
        for (int i=0; i<cycleUniqueColors; i++)
        {
            colorSequence[i] = i;
        }

        // Add random colors until length is met
        for (int i=cycleUniqueColors; i<cycleLength; i++)
        {
            colorSequence[i] = Random.Range(0, cycleUniqueColors);
        }

        // Shuffle the list
        colorSequence = colorSequence.OrderBy(x => Random.value).ToArray();

        // For each color, check that the color below is not the same color
        for (int i=0; i<cycleLength-1; i++)
        {
            // If it is, swap the color to a random color that is not either of the colors next to it
            // If at the top, tile above is the tile at the bottom, which is the one before it
            int colorAbove = (i == 0) ? colorSequence[colorSequence.Length-1] : colorSequence[i-1];
            int colorBelow = colorSequence[i+1];

            // Keep picking a new color until it is different than the one above & below
            // don't run if cycle length and unique color amount make this impossible (need at least 3 colors for guaranteed no adjacent touching)
            while ((colorSequence[i] == colorAbove || colorSequence[i] == colorBelow) && (cycleUniqueColors > 2))
            {
                colorSequence[i] = Random.Range(0,cycleUniqueColors);
            }
        }

        return colorSequence;
    }
}