using TMPro;
using UnityEngine;

public class InfoManager : MonoBehaviour
{
    public ServerInfo info;
    public TextMeshProUGUI playerText;

    private void Start()
    {
        info.PlayerCapacity = 2;
        playerText.text = info.PlayerCapacity.ToString();
    }

    public void ChangeServerName(string newName)
    {
        info.Name = newName;
    }
    public void ChangeCapacity(bool add)
    {
        if (add)
        {
            info.PlayerCapacity++;
            playerText.text = info.PlayerCapacity.ToString();
        }
        else
        {
            if(info.PlayerCapacity > 2)
            {
                info.PlayerCapacity--;
                playerText.text = info.PlayerCapacity.ToString();
            }
            else
            {
                Debug.Log("No se puede");
            }
        }
    }

    public void StartHost()
    {
        if(info.Name != string.Empty)
        {
            Debug.Log("Start Server!!");
            NLSNetworkManager.singleton.StartHost();
            gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("No hay nombre");
        }
    }
}
