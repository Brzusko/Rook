using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using IT.Interfaces;
using UnityEngine;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace IT
{
    public abstract class YAMLFileProcessor<T, TW> : IFileProcessor<T> where T : class, new() where TW : class
    {
        private string _filePath;
        protected T _fileContent;
        
        protected YAMLFileProcessor(string filePath)
        {
            _filePath = filePath;

            if (!File.Exists(filePath))
            {
                GenerateDefaultFile();
                return;
            }
            
            LoadFile();
        }

        public virtual void WriteContent<TW>(TW newContent) where TW : class
        {
            
        }

        public void SaveCache()
        {
            if (_fileContent == null)
            {
                Debug.LogError("File content is missing");
                return;
            }
            
            var serializer = new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
            var yaml = serializer.Serialize(_fileContent);
            using var fileStream = new FileStream(_filePath, FileMode.Truncate);
            fileStream.Write(Encoding.UTF8.GetBytes(yaml));
        }

        public void LoadFile()
        {
            using var reader = new StreamReader(_filePath, Encoding.UTF8);
            var content = reader.ReadToEnd();
            var deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            _fileContent = deserializer.Deserialize<T>(content);
        }

        public void Prune()
        {
            _fileContent = null;
        }

        public T GrabFileContent()
        {
            return _fileContent;
        }

        private void GenerateDefaultFile()
        {
            var content = new T();
            var serializer = new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
            var yaml = serializer.Serialize(content);
            using var fileStream = new FileStream(_filePath, FileMode.Create);
            fileStream.Write(Encoding.UTF8.GetBytes(yaml));
            _fileContent = content;
        }
        
    }
}
