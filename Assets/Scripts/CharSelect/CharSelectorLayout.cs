using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// TODO: Eventually this class will show and hide the 3rd and 4th board based on the set room size.
/// Haven't implemented that yet though.
/// </summary>
public class CharSelectorLayout : MonoBehaviour
{
    [SerializeField] private CharSelector[] _selectors;
    public CharSelector[] selectors => _selectors;

    public CharSelector GetCharSelectorByIndex(int boardIndex) {
        return _selectors[boardIndex];
    }
}
