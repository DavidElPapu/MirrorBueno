using UnityEngine;
using UnityEngine.UI;

public class ColorButton : MonoBehaviour
{
    public PlayerInfo info;

    public void SetInfoColor()
    {
        info.SetColor(gameObject.GetComponent<Image>().color);
    }
}
