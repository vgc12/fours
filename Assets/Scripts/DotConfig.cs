using UnityEngine;

[CreateAssetMenu(fileName = "Dot Config", menuName = "Scriptable Objects/DotConfig")]
public sealed class DotConfig : ScriptableObject
{
    public Sprite sprite;
    public Vector3 growScale;
    public float growDuration;
    public float shrinkDuration;
    public int sortOrder;
}
