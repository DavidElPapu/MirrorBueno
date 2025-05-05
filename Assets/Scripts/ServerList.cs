using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

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
                if (response != null) 
                {
                    listServerEntries = response.servers;

                    BalancePrefabs(listServerEntries.Count, transform);
                    UpdateListElements();
                }
            }
            isBusy = false;
        }
        yield break;
    }

    public void BalancePrefabs(int amount, Transform parent)
    {
        for (int i = parent.childCount; i < amount; i++)
        {
            if (listElementPrefab != null) 
            {
                Instantiate(listElementPrefab, parent);
            }


        }
        for (int i = parent.childCount; i >= amount; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
        }
    }

    public void UpdateListElements()
    {
        for (int i = 0; i < listServerEntries.Count; i++)
        {
            if (i > transform.childCount || transform.GetChild(i) == null) 
            {
                continue;
            }

            ServerItem listItemUI = transform.GetChild(i).GetComponent<ServerItem>();

            string modifiedAddress = string.Empty;
            if (listServerEntries[i].ip.StartsWith("::ffff:"))
            {
                modifiedAddress = listServerEntries[i].ip.Replace("::ffff:", string.Empty);
            }
            else
            {
                modifiedAddress = listServerEntries[i].ip;
            }

            listItemUI.SetServerLabel(listServerEntries[i].name, listServerEntries[i].ip);

            Button joinButton = listItemUI.GetJoinButton();
            joinButton.onClick.RemoveAllListeners();
            joinButton.onClick.AddListener(() => 
            {
                NLSNetworkManager.singleton.networkAddress = modifiedAddress;
                NLSNetworkManager.singleton.StartClient();
            });
        }
    }
}
