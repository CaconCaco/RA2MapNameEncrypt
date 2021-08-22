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
    class RA2Map : FileSystemInfo
    {
        public string[] AIElementRegs;
        public FileInfo file;

        public RA2Map(string fs)
        {
            file = new FileInfo(fs);
            Task.WaitAny(AIElementParse(file));
        }
        public RA2Map(FileInfo f)
        {
            file = f;
            Task.WaitAny(AIElementParse(f));
        }

        public override string Name => file.Name;

        public override bool Exists => file.Exists;

        public string DirectoryName => file.DirectoryName;

        public override void Delete() => file.Delete();

        public void CopyTo(string fn, bool ow = false) => file.CopyTo(fn, ow);

        private async Task AIElementParse(FileInfo f)
        {
            var stream = f.OpenRead();
            var doc = await IniDocumentUtils.ParseAsync(stream);
            var regs = new List<string>(doc["TaskForces"].Select(i => (string)i.Value));
            regs.AddRange(doc["ScriptTypes"].Select(i => (string)i.Value));
            regs.AddRange(doc["TeamTypes"].Select(i => (string)i.Value));
            AIElementRegs = regs.ToArray();
            await stream.DisposeAsync();
        }
    }
}
