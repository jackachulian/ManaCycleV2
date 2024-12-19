using System.Linq;
using UnityEngine;
using UnityEngine.Video;


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
    
    

    // (These will later depend on the level in solo mode, but static for now)
    private const int cycleLength = 7;
    private const int cycleUniqueColors = 5;

    /// <summary>
    /// All visual Tiles that are displayed on the display the cycle
    /// </summary>
    private ManaTile[] tiles;

    /// <summary>
    /// The cycle randonly generated upon battle initialization.
    /// </summary>
    private int[] cycle;

    /// <summary>
    /// Is called by the BattleManager, after the BattleManager initializes and before the Boards initialize.
    /// </summary>
    public void InitializeBattle(BattleManager battleManager) {
        foreach (Transform child in manaTileTransform) {
            Destroy(child.gameObject);
        }

        tiles = new ManaTile[cycleLength];
        cycle = new int[cycleLength];

        // Add one of each color to the list
        for (int i=0; i<cycleUniqueColors; i++)
        {
            cycle[i] = i;
        }

        // Add random colors until length is met
        for (int i=cycleUniqueColors; i<cycleLength; i++)
        {
            cycle[i] = Random.Range(0, cycleUniqueColors);
        }

        // Shuffle the list
        cycle = cycle.OrderBy(x => Random.value).ToArray();

        // For each color, check that the color below is not the same color
        for (int i=0; i<cycleLength-1; i++)
        {
            // If it is, swap the color to a random color that is not either of the colors next to it
            // If at the top, tile above is the tile at the bottom, which is the one before it
            int colorAbove = (i == 0) ? cycle[cycle.Length-1] : cycle[i-1];
            int colorBelow = cycle[i+1];

            // Keep picking a new color until it is different than the one above & below
            // don't run if cycle length and unique color amount make this impossible (need at least 3 colors for guaranteed no adjacent touching)
            while ((cycle[i] == colorAbove || cycle[i] == colorBelow) && (cycleUniqueColors > 2))
            {
                cycle[i] = Random.Range(0,cycleUniqueColors);
            }
        }

        // Create cycle color objects for each cycle color
        for (int i=0; i<cycleLength; i++)
        {
            ManaTile tile = battleManager.SpawnTile();
            tiles[i] = tile;
            tile.SetColor(cycle[i], battleManager.cosmetics);
            tile.transform.SetParent(manaTileTransform);
            tile.transform.localPosition = new Vector2(0, (i - (cycleLength-1)/2.0f) * -manaSeparation);
            tile.transform.localScale = new Vector2(manaScale, manaScale);
        }
    }
}