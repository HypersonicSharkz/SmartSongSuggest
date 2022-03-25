using System;

namespace Data
{

    public class FilesMeta
    {
        public string top10kVersion { get; set; }
        public DateTime top10kUpdated { get; set; }

        public String GetLargeVersion()
        {
            return top10kVersion.Split('.')[0]; ;
        }
    }
}