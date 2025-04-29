using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ServerItem : MonoBehaviour
{
    [SerializeField]
    private Button joinButton;
    public TextMeshProUGUI nameLabel;
    public TextMeshProUGUI ipLabel;
    private string address = string.Empty;
    
    public void SetServerLabel(string nameText, string ipText)
    {
        nameLabel.text = nameText;
        ipLabel.text = ipText;
    }

    public Button GetJoinButton()
    {
        return joinButton;
    }
}
