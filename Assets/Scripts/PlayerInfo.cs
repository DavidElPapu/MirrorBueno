using UnityEngine;

public class PlayerInfo : MonoBehaviour
{
    [SerializeField] public Color color { get; private set; } = Color.white;

    public void SetColor(Color newColor)
    {
        color = newColor;
    }
}
