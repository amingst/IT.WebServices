using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IT.WebServices.Helpers
{
    public static class DirectoryInfoExtensions
    {
        public static DirectoryInfo CreateGuidDirectory(this DirectoryInfo parentDir, Guid id, int depthOfSubFolders = 2)
        {
            var dir = parentDir.CreateGuidSubDirectories(id, depthOfSubFolders);

            var name = id.ToString();
            dir = dir.CreateSubdirectory(name);

            return dir;
        }

        public static DirectoryInfo CreateGuidSubDirectories(this DirectoryInfo parentDir, Guid id, int depthOfSubFolders = 2)
        {
            var dir = parentDir;
            var name = id.ToString();

            for (int i = 0; i < depthOfSubFolders; i++)
                dir = dir.CreateSubdirectory(name.Substring(i * 2, 2));

            return dir;
        }

        public static FileInfo CreateGuidFileInfo(this DirectoryInfo parentDir, Guid id, int depthOfSubFolders = 2)
        {
            var dir = parentDir.CreateGuidSubDirectories(id, depthOfSubFolders);

            var fi = new FileInfo(dir.FullName + "/" + id.ToString());

            return fi;
        }
    }
}
