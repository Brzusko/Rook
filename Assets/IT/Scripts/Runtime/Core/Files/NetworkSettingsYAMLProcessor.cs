using System.Collections;
using System.Collections.Generic;
using IT;
using IT.Data;
using UnityEngine;
using YamlDotNet.RepresentationModel;

public class NetworkSettingsYAMLProcessor : YAMLFileProcessor<NetworkSettings, NetworkSettingsWriteData>
{
    public NetworkSettingsYAMLProcessor(string filePath) : base(filePath)
    {
    }

    public override void WriteContent<TW>(TW newContent)
    {
        if (newContent is not NetworkSettingsWriteData data)
        {
            Debug.LogError("Could not cast to given data type");
            return;
        }
        
        _fileContent.Port = data.Port;
        _fileContent.Address = data.Address;
        _fileContent.MaxClients = data.MaxClients;
        _fileContent.AsDedicatedServer = data.AsDedicatedServer;

    }
}
