using UnityEngine;

[CreateAssetMenu(menuName = "Game/MapConfig")]
public class MapConfig_SO : ScriptableObject
{
    public int mapIndex;
    public string mapName;
    public LevelConfig_SO[] levels; // ordered 0..N-1
}
