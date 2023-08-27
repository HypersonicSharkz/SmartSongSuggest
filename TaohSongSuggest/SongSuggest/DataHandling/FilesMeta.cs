using System;

namespace Data
{

    public class FilesMeta
    {
        public string top10kVersion { get; set; } = "0.0";
        public string songLibraryVersion { get; set; } = "0.0";
        public DateTime top10kUpdated { get; set; }

        //Deprecated, use the String version and later rework old checks to new when playerData is updated.
        public String GetLargeVersion()
        {
            return top10kVersion.Split('.')[0]; ;
        }

        //Get the major version of a string.
        public String Major(FilesMetaType type)
        {
            if (type == FilesMetaType.Top10kVersion) return GetMajorVersion(GetMajorVersion(top10kVersion));
            if (type == FilesMetaType.SongLibraryVersion) return GetMajorVersion(GetMajorVersion(songLibraryVersion));
            return null;
        }

        private String GetMajorVersion(String version)
        {
            return version.Split('.')[0]; ;
        }
    }

    public enum FilesMetaType
    {
        Top10kVersion = 1,
        SongLibraryVersion = 2,
    }
}