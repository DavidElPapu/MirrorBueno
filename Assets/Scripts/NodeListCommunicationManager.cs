using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

public class NodeListCommunicationManager : MonoBehaviour
{
    public static NodeListCommunicationManager singleton;

    public ServerInfo curServerInfo = new ServerInfo();

    public string AuthKey = "NodeListServerDefaultKey";
    private const string Server = "http://20.190.46.42:8889";

    public string InstanceServerId = string.Empty;

    private void Awake()
    {
        if(singleton != null && singleton != this) { Destroy(this); } else { singleton = this; }
    }

    public void AddUpdateServerEntry()
    {
        StartCoroutine(AddUpdateInternal());
    }
    public void RemoveServerEntry()
    {
        StartCoroutine(RemoveServerInternal());
    }

    private IEnumerator AddUpdateInternal()
    {
        WWWForm serverData = new WWWForm();
        serverData.AddField("serverKey", AuthKey);
        bool addingServer = false;

        if (String.IsNullOrEmpty(InstanceServerId))
        {
            addingServer = true;
        }
        else
        {
            serverData.AddField("serverUuid", InstanceServerId);
        }

        serverData.AddField("serverName", curServerInfo.Name);
        serverData.AddField("serverPort", curServerInfo.Port);
        serverData.AddField("serverPlayers", curServerInfo.PlayerCount);
        serverData.AddField("serverCapacity", curServerInfo.PlayerCapacity);
        serverData.AddField("serverExtras", curServerInfo.ExtraInfo);

        UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Post(Server + "/add", serverData);

        www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        yield return www.SendWebRequest();

        if (www.responseCode == 200) 
        {
            if (addingServer)
            {
                InstanceServerId = www.downloadHandler.text;
            }
        }
        else
        {
            Debug.LogError("Couldn't add the server");
        }

        yield break;
    }

    private IEnumerator RemoveServerInternal()
    {
        WWWForm serverData = new WWWForm();
        serverData.AddField("serverKey", AuthKey);
        serverData.AddField("serverUuid", InstanceServerId);

        using (UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Post(Server + "/remove", serverData))
        {
            yield return www.SendWebRequest();

            if (www.responseCode == 200)
            {
                Debug.Log("Servidor Removido exitosamente");
            }
            else
            {
                Debug.LogError("Couldn't remove server");
            }
        }

        yield break;
    }
}
