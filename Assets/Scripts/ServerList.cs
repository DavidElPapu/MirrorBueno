using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class ServerList : MonoBehaviour
{
    public ServerInfo[] servers = new ServerInfo[10];

    public GameObject[] spawned = new GameObject[10];

    public bool hasSpawned = false;

    [SerializeField]
    private string masterServerUrl = "https://20.190.46.42:8889/List";
    [SerializeField]
    private string communicationKey = "NodeListServerDefaultKey";

    private int refreshInterval = 30;

    public GameObject listElementPrefab;

    private bool isBusy = false;

    private WWWForm unityRequestForm;
    private List<NodeListServerEntry> listServerEntries = new List<NodeListServerEntry>();

    private void Awake()
    {
        unityRequestForm = new WWWForm();
        unityRequestForm.AddField("serverKey", communicationKey);

        if (string.IsNullOrEmpty(communicationKey))
        {
            enabled = true;
        }
    }

    private void Start()
    {
        RefreshList();
        if (refreshInterval > 0)
        {
            InvokeRepeating(nameof(RefreshList), Time.realtimeSinceStartup + refreshInterval, refreshInterval);
        }
    }

    public void RefreshList()
    {
        if (isBusy) return;
        StartCoroutine(RefreshServerList());
    }

    private IEnumerator RefreshServerList()
    {
        using(UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Post(masterServerUrl, unityRequestForm))
        {
            isBusy = true;

            yield return www.SendWebRequest();

            if (www.responseCode == 200)
            {
                NodeListServerResponse response = JsonUtility.FromJson<NodeListServerResponse>(www.downloadHandler.text.Trim());
            }
        }
    }
}
