using RA2MapNameEncrypt.Utils;
using Shimakaze.Struct.Ini;
using Shimakaze.Struct.Ini.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA2MapNameEncrypt.Data
{
    class RA2Map
    {
        public FileInfo file;
        public string[] AIElementRegs;

        public RA2Map(string path)
        {
            file = new FileInfo(path);
            Task.WaitAny(AIElementParse());
        }

        private async Task AIElementParse()
        {
            var lSection = new List<string>();
            var doc = await IniDocumentUtils.ParseAsync(file.OpenRead());
            foreach (var reg in doc["TaskForces"])
            {
                lSection.Add(reg.Value);
            }
            foreach (var reg in doc["ScriptTypes"])
            {
                lSection.Add(reg.Value);
            }
            foreach (var reg in doc["TeamTypes"])
            {
                lSection.Add(reg.Value);
            }
            AIElementRegs = lSection.ToArray();
        }

        
    }
}
