
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace SecretsGen
{
    public class GitIgnore
    {
        private List<string> m_plGitIgnore = new List<string>();
        private List<string> m_plTargetFiles = new List<string>();

        private int m_iInsertIgnoreHere = -1;
        private static string s_sGitIgnoreComment = "# ignore generated secrets files";
        bool m_fInsertingAtExistingSecrets = false;

        public GitIgnore()
        {
        }

        /*----------------------------------------------------------------------------
        	%%Function: CreateGitIgnoreFromReader
        	%%Qualified: SecretsGen.GitIgnore.CreateGitIgnoreFromReader
        	
        ----------------------------------------------------------------------------*/
        public static GitIgnore CreateGitIgnoreFromReader(TextReader tr, IEnumerable<string> rgsTargetFiles)
        {
            GitIgnore gitIgnore = new GitIgnore();
            string sLine;

            foreach (string s in rgsTargetFiles)
            {
                string sFilename = Path.GetFileName(s);

                if (!gitIgnore.m_plTargetFiles.Contains(sFilename))
                    gitIgnore.m_plTargetFiles.Add(sFilename);
            }

            bool fFoundFirstNonComment = false;
            bool fFoundFirstNonBlank = false;

            while ((sLine = tr.ReadLine()) != null)
            {
                gitIgnore.m_plGitIgnore.Add(sLine);
                if (gitIgnore.m_plTargetFiles.Contains(sLine))
                {
                    gitIgnore.m_plTargetFiles.Remove(sLine);
                    gitIgnore.m_iInsertIgnoreHere = gitIgnore.m_plGitIgnore.Count - 1;
                    gitIgnore.m_fInsertingAtExistingSecrets = true;
                }

                if (!fFoundFirstNonComment && !sLine.StartsWith("#"))
                {
                    fFoundFirstNonComment = true;
                }

                if (fFoundFirstNonComment && !fFoundFirstNonBlank && sLine.Length > 1)
                {
                    {
                        fFoundFirstNonBlank = true;
                        if (!sLine.StartsWith("#") || sLine.IndexOf("secrets") == -1)
                            gitIgnore.m_iInsertIgnoreHere = gitIgnore.m_plGitIgnore.Count - 1;
                        else // else this is a comment about secrets that we want to skip past and insert right after
                            gitIgnore.m_iInsertIgnoreHere = gitIgnore.m_plGitIgnore.Count;
                    }
                }
            }

            return gitIgnore;
        }

        /*----------------------------------------------------------------------------
        	%%Function: CreateGitIgnore
        	%%Qualified: SecretsGen.GitIgnore.CreateGitIgnore
        	
        ----------------------------------------------------------------------------*/
        public static GitIgnore CreateGitIgnore(string sGitIgnorePath, IEnumerable<string> rgsTargetFiles)
        {
            using (TextReader tr = File.OpenText(sGitIgnorePath))
            {
                return CreateGitIgnoreFromReader(tr, rgsTargetFiles);
            }
        }

        /*----------------------------------------------------------------------------
        	%%Function: WriteGitIgnore
        	%%Qualified: SecretsGen.GitIgnore.WriteGitIgnore
        	
        ----------------------------------------------------------------------------*/
        public void WriteGitIgnore(TextWriter tw)
        {
            int i = 0;
            bool fReusedComment = true;

            while (i < m_plGitIgnore.Count)
            {
                if (i == m_iInsertIgnoreHere 
                    && (m_plTargetFiles.Count > 0 || m_fInsertingAtExistingSecrets))
                {
                    if (!m_fInsertingAtExistingSecrets 
                        && (i == 0 || !m_plGitIgnore[i - 1].StartsWith("#") || m_plGitIgnore[i - 1].IndexOf("secrets") == -1))
                    {
                        tw.WriteLine(s_sGitIgnoreComment);
                        fReusedComment = false;
                    }

                    foreach (string s in m_plTargetFiles)
                        tw.WriteLine(s);

                    if (!fReusedComment && !m_fInsertingAtExistingSecrets)
                        tw.WriteLine();
                }

                tw.WriteLine(m_plGitIgnore[i]);
                i++;
            }
        }

        #region Tests

        [Test]
        public static void TestCreateGitIgnoreFromReader_NoTargetFiles()
        {
            TextReader tr = new StringReader("#test\n\nNonBlank\n");
            string[] rgsTargetFiles = { };

            GitIgnore gitIgnore = CreateGitIgnoreFromReader(tr, rgsTargetFiles);
            Assert.AreEqual(2, gitIgnore.m_iInsertIgnoreHere);
        }

        public static void AssertListsAreEqual(IEnumerable<string> leftStrings, IEnumerable<string> rightStrings)
        {
            List<string> plsLeft = new List<string>();
            List<string> plsRight = new List<string>();

            foreach (string s in leftStrings)
                plsLeft.Add(s);

            foreach (string s in rightStrings)
            {
                plsRight.Add(s);
                Assert.IsTrue(plsLeft.Contains(s));
            }

            foreach (string s in plsLeft)
                Assert.IsTrue(plsRight.Contains(s));
        }

        [Test]
        public static void TestCreateGitIgnoreFromReader_TargetFilesNoMatch()
        {
            TextReader tr = new StringReader("#test\n\nNonBlank\n");
            string[] rgsTargetFiles = { "c:\\temp\\SecretFoo.config" , "c:\\temp\\foo\\SecretFoo.config", "c:\\temp\\SecretBar.xml" };

            GitIgnore gitIgnore = CreateGitIgnoreFromReader(tr, rgsTargetFiles);
            Assert.AreEqual(2, gitIgnore.m_iInsertIgnoreHere);
            Assert.AreEqual(2, gitIgnore.m_plTargetFiles.Count);
            AssertListsAreEqual(gitIgnore.m_plTargetFiles, new string[] {"SecretFoo.config", "SecretBar.xml"});
        }

        [Test]
        public static void TestCreateGitIgnoreFromReader_TargetFiles1Match()
        {
            TextReader tr = new StringReader("#test\n\nNonBlank\nSecretFoo.config\n");
            string[] rgsTargetFiles = { "c:\\temp\\SecretFoo.config", "c:\\temp\\foo\\SecretFoo.config", "c:\\temp\\SecretBar.xml" };

            GitIgnore gitIgnore = CreateGitIgnoreFromReader(tr, rgsTargetFiles);
            Assert.AreEqual(3, gitIgnore.m_iInsertIgnoreHere);
            Assert.AreEqual(1, gitIgnore.m_plTargetFiles.Count);
            AssertListsAreEqual(gitIgnore.m_plTargetFiles, new string[] { "SecretBar.xml" });
        }

        [Test]
        public static void TestWriteGitIgnore_NoMatchNoTargetFiles()
        {
            string sGitIgnore = "#test\r\n\r\nNonBlank\r\nSecretFoo.config\r\n";
            TextReader tr = new StringReader(sGitIgnore);
            string[] rgsTargetFiles = {  };

            GitIgnore gitIgnore = CreateGitIgnoreFromReader(tr, rgsTargetFiles);
            StringWriter sw = new StringWriter();

            gitIgnore.WriteGitIgnore(sw);
            sw.Flush();
            sw.Close();

            Assert.AreEqual(sGitIgnore, sw.ToString());
        }

        [Test]
        public static void TestWriteGitIgnore_NoMatchNoCommentTargetFilesAtTop()
        {
            string sGitIgnore = "#test\r\n\r\nNonBlank\r\nSecretFoo.config\r\n";
            TextReader tr = new StringReader(sGitIgnore);
            string[] rgsTargetFiles = { "c:\\temp\\Secrets.xml"};

            GitIgnore gitIgnore = CreateGitIgnoreFromReader(tr, rgsTargetFiles);
            StringWriter sw = new StringWriter();

            gitIgnore.WriteGitIgnore(sw);
            sw.Flush();
            sw.Close();

            string sGitIgnoreExpected  = "#test\r\n\r\n" + s_sGitIgnoreComment + "\r\nSecrets.xml\r\n\r\nNonBlank\r\nSecretFoo.config\r\n";
            Assert.AreEqual(sGitIgnoreExpected, sw.ToString());
        }

        [Test]
        public static void TestWriteGitIgnore_NoMatch_WithCommentEmptyFollowingLine_TargetFilesAtTop()
        {
            string sGitIgnore = "#test\r\n\r\n# ignore any secrets here\r\n\r\nNonBlank\r\nSecretFoo.config\r\n";
            TextReader tr = new StringReader(sGitIgnore);
            string[] rgsTargetFiles = { "c:\\temp\\Secrets.xml" };

            GitIgnore gitIgnore = CreateGitIgnoreFromReader(tr, rgsTargetFiles);
            StringWriter sw = new StringWriter();

            gitIgnore.WriteGitIgnore(sw);
            sw.Flush();
            sw.Close();

            string sGitIgnoreExpected = "#test\r\n\r\n# ignore any secrets here\r\nSecrets.xml\r\n\r\nNonBlank\r\nSecretFoo.config\r\n";
            Assert.AreEqual(sGitIgnoreExpected, sw.ToString());
        }

        [Test]
        public static void TestWriteGitIgnore_NoMatch_WithCommentWithContent_TargetFilesAtTop()
        {
            string sGitIgnore = "#test\r\n\r\n# ignore any secrets here\r\nNonBlank\r\nSecretFoo.config\r\n";
            TextReader tr = new StringReader(sGitIgnore);
            string[] rgsTargetFiles = { "c:\\temp\\Secrets.xml" };

            GitIgnore gitIgnore = CreateGitIgnoreFromReader(tr, rgsTargetFiles);
            StringWriter sw = new StringWriter();

            gitIgnore.WriteGitIgnore(sw);
            sw.Flush();
            sw.Close();

            string sGitIgnoreExpected = "#test\r\n\r\n# ignore any secrets here\r\nSecrets.xml\r\nNonBlank\r\nSecretFoo.config\r\n";
            Assert.AreEqual(sGitIgnoreExpected, sw.ToString());
        }

        [Test]
        public static void TestWriteGitIgnore_NoMatchWithNoCommentTargetFilesAtTop()
        {
            string sGitIgnore = "#test\r\n\r\nNonBlank\r\nSecretFoo.config\r\n";
            TextReader tr = new StringReader(sGitIgnore);
            string[] rgsTargetFiles = { "c:\\temp\\Secrets.xml" };

            GitIgnore gitIgnore = CreateGitIgnoreFromReader(tr, rgsTargetFiles);
            StringWriter sw = new StringWriter();

            gitIgnore.WriteGitIgnore(sw);
            sw.Flush();
            sw.Close();

            string sGitIgnoreExpected = "#test\r\n\r\n" + s_sGitIgnoreComment + "\r\nSecrets.xml\r\n\r\nNonBlank\r\nSecretFoo.config\r\n";
            Assert.AreEqual(sGitIgnoreExpected, sw.ToString());
        }

        [Test]
        public static void TestWriteGitIgnore_Match_WithNoComment_TargetFilesAtMatch()
        {
            string sGitIgnore = "#test\r\n\r\nNonBlank\r\nSecretFoo.config\r\n";
            TextReader tr = new StringReader(sGitIgnore);
            string[] rgsTargetFiles = { "c:\\temp\\Secrets.xml", "c:\\temp\\bar\\SecretFoo.config" };

            GitIgnore gitIgnore = CreateGitIgnoreFromReader(tr, rgsTargetFiles);
            StringWriter sw = new StringWriter();

            gitIgnore.WriteGitIgnore(sw);
            sw.Flush();
            sw.Close();

            string sGitIgnoreExpected = "#test\r\n\r\nNonBlank\r\nSecrets.xml\r\nSecretFoo.config\r\n";
            Assert.AreEqual(sGitIgnoreExpected, sw.ToString());
        }


        [Test]
        public static void TestWriteGitIgnore_MatchAtLine0_WithNoComment_TargetFilesAtMatch()
        {
            string sGitIgnore = "SecretFoo.config\r\n#test\r\n\r\nNonBlank\r\n";
            TextReader tr = new StringReader(sGitIgnore);
            string[] rgsTargetFiles = { "c:\\temp\\Secrets.xml", "c:\\temp\\bar\\SecretFoo.config" };

            GitIgnore gitIgnore = CreateGitIgnoreFromReader(tr, rgsTargetFiles);
            StringWriter sw = new StringWriter();

            gitIgnore.WriteGitIgnore(sw);
            sw.Flush();
            sw.Close();

            string sGitIgnoreExpected = "Secrets.xml\r\nSecretFoo.config\r\n#test\r\n\r\nNonBlank\r\n";
            Assert.AreEqual(sGitIgnoreExpected, sw.ToString());
        }

        [Test]
        public static void TestWriteGitIgnore_Match_WithComment_TargetFilesAtMatch()
        {
            string sGitIgnore = "#test\r\n\r\n# ignore secrets here\r\nSecretFoo.config\r\nNonBlank\r\n";
            TextReader tr = new StringReader(sGitIgnore);
            string[] rgsTargetFiles = { "c:\\temp\\Secrets.xml", "c:\\temp\\bar\\SecretFoo.config" };

            GitIgnore gitIgnore = CreateGitIgnoreFromReader(tr, rgsTargetFiles);
            StringWriter sw = new StringWriter();

            gitIgnore.WriteGitIgnore(sw);
            sw.Flush();
            sw.Close();

            string sGitIgnoreExpected = "#test\r\n\r\n# ignore secrets here\r\nSecrets.xml\r\nSecretFoo.config\r\nNonBlank\r\n";
            Assert.AreEqual(sGitIgnoreExpected, sw.ToString());
        }
        #endregion
    }
}