using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Wox.Infrastructure.Storage;

namespace Wox.Plugin.CopyFiles
{
    public class FileStorage : JsonStrorage<FileStorage>
    {
        [JsonProperty]
        public Dictionary<string, int> FileHistory = new Dictionary<string, int>();

        protected override string ConfigName
        {
            get { return "FileHistory"; }
        }

        public void AddFileHistory(string path)
        {
            path = Path.GetFullPath(path);
            if (FileHistory.ContainsKey(path))
            {
                FileHistory[path] += 1;
            }
            else
            {
                FileHistory.Add(path,1);
            }
            Save();
        }

    }
}
