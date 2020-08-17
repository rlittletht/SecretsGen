
using NUnit.Framework;
using System.IO;
using Microsoft.Identity.Client;

namespace SecretsGen
{
    public class PathHelper
    {
        /*----------------------------------------------------------------------------
        	%%Function: FullPathFromPaths
        	%%Qualified: SecretsGen.PathHelper.FullPathFromPaths
        	
            Sadly we have to wait for .Net 5 or use .NetCore3 in order to get a 
            useful Path.GetFullPath() method.  For now, do this ourselves
        ----------------------------------------------------------------------------*/
        public static string FullPathFromPaths(string sPath, string sBase)
        {
            if (sPath.StartsWith("\\") || sPath[1] == ':')
                return sPath;

            return Path.GetFullPath(Path.Combine(sBase, sPath));
        }

        [TestCase("c:\\foo", "c:\\current\\dir", "c:\\foo")]
        [TestCase("\\foo", "c:\\current\\dir", "\\foo")]
        [TestCase(".\\foo", "c:\\current\\dir", "c:\\current\\dir\\foo")]
        [TestCase("foo", "c:\\current\\dir", "c:\\current\\dir\\foo")]
        [TestCase("foo\\bar", "c:\\current\\dir", "c:\\current\\dir\\foo\\bar")]
        [TestCase("\\\\server\\share\\foo", "c:\\current\\dir", "\\\\server\\share\\foo")]
        [Test]
        public static void TestFullPathFromPaths(string sPath, string sBase, string sExpected)
        {
            Assert.AreEqual(sExpected, FullPathFromPaths(sPath, sBase));
        }


        public static void EnsureDirectoriesExist(string sFullPath)
        {
            string sDirectory = sFullPath;

            while ((sDirectory = Path.GetDirectoryName(sDirectory)) != null)
            {
                if (Directory.Exists(sDirectory))
                    break;

                Directory.CreateDirectory(sDirectory);
            }
        }

        #if DANGEROUS // these affect the local filesystem, including deleting files. They are disabled by default
        [TestCase("c:\\__notthere\\foo.txt", "c:\\__notthere")]
        [TestCase("c:\\__nobackup\\temp\\__notthere\\__orthere\\__orthereeven\\foo.txt", "c:\\__nobackup\\temp\\__notthere")]
        [Test]
        public static void TestEnsureDirectoriesExist(string sFullPath, string sCleanupRoot)
        {
            EnsureDirectoriesExist(sFullPath);
            using (FileStream fs = File.Create(sFullPath))
            {
                fs.Close();
            }

            if (sCleanupRoot != null)
                Directory.Delete(sCleanupRoot, true);
        }
        #endif
    }
}
