using System;

namespace Data
{

    public class FileFormatVersions
    {
        //Defaults a new created version (new install/first install with this) to 0, and force an update of the files if stored format does not match version
        //Only large version is expected to be used for now (New file needed), but allowing the option for smaller version updates that might require less updates/onetime handling
        public string activePlayerVersion { get; set; } = "0.0";
        public string songLibraryVersion { get; set; } = "0.0";
        public string top10kVersion { get; set; } = "0.0";

        //Get the major version of a string.
        public String GetMajorVersion(FileFormatType type)
        {
            if (type == FileFormatType.Top10kVersion) return GetMajorVersion(GetMajorVersion(top10kVersion));
            if (type == FileFormatType.SongLibraryVersion) return GetMajorVersion(GetMajorVersion(songLibraryVersion));
            if (type == FileFormatType.ActivePlayerVersion) return GetMajorVersion(GetMajorVersion(activePlayerVersion));
            return null;
        }

        private String GetMajorVersion(String version)
        {
            return version.Split('.')[0]; ;
        }
    }

    public enum FileFormatType
    {
        Top10kVersion = 1,
        SongLibraryVersion = 2,
        ActivePlayerVersion = 3
    }
}