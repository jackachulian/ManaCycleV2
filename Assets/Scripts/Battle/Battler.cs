using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class Battler : MonoBehaviour {
    /// <summary>
    /// Use this to grab the portraitOffset that should be applied in various areas of the game
    /// </summary>
    [SerializeField] private SpriteRenderer portraitSpriteRenderer;


    /// <summary>
    /// The id this battler may be identified with by achievmeents, etc
    /// </summary>
    [SerializeField] private string _battlerId;
    public string battlerId => _battlerId;


    /// <summary>
    /// name displayed on char select and other areas
    /// </summary>
    [SerializeField] private string _displayName; // TODO: localize
    public string displayName => _displayName;


    // used for the char select icon
    [SerializeField] private Material _gradientMat;
    public Material gradientMat => _gradientMat;


    // colors used in various ui
    [SerializeField] private Color _mainColor = Color.white;
    public Color mainColor => _mainColor;

    [SerializeField] private Color _altColor = Color.black;
    public Color altColor => _altColor;


    // crossover logo for char select
    [SerializeField] private Sprite _gameLogo;
    public Sprite gameLogo => _gameLogo;


    /// <summary>
    /// Contains all properties and functions related to this battler's in-battle unique abilities.
    /// </summary>
    [SerializeField] private BattlerAbility battlerAbility;


    public Vector2 portraitOffset => portraitSpriteRenderer.transform.localPosition;
    public Sprite sprite => portraitSpriteRenderer.sprite;
}