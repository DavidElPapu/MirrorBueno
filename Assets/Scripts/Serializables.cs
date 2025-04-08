using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class NodeListServerEntry
{
    public string ip;
    public string name;
    public int port;
    public int players;
    public int capacity;
    public string extras;
}

[Serializable]
public class NodeListServerResponse
{
    public int count;
    public List<NodeListServerEntry> servers;
}

[Serializable]
public struct ServerInfo
{
    public string Name;
    public int Port;
    public int PlayerCount;
    public int PlayerCapacity;
    public string ExtraInfo;
}