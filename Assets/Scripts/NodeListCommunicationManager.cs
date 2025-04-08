using Unity.VisualScripting;
using UnityEngine;

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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
