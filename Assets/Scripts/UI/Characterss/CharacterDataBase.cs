using UnityEngine;

[CreateAssetMenu(fileName = "CharacterDataBase", menuName = "Scriptable Objects/CharacterDataBase")]
public class CharacterDataBase : ScriptableObject
{
    [SerializeField] public Character[] character;

    public int characterCount => character.Length;

    public Character GetCharacter(int index)
    {
        return character[index];
    }
}
