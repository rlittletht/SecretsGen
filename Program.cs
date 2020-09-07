using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TCore.CmdLine;

namespace SecretsGen
{
    class Program
    {
        static void Main(string[] args)
        {
//            args = new[] {"-Secret", "AzureSql-ConnectionString/09fbb405e7b2434ea50979b6b6e92cf1" };
//            args = new[] { "-Manifest", "d:\\dev\\SecretsGen\\Secrets.xml" };

            Config config = new Config();
            CmdLine cmdLine = new CmdLine(Config.s_CmdLineConfig);

            if (!cmdLine.FParse(args, config, null, out string sError))
            {
                cmdLine.Usage(Console.WriteLine);
                return;
            }

            if (config.ShowSecret != null)
                ShowSecret(config);
            else if (config.ManifestFile != null)
                ProcessManifestFile(config);
        }

        static void ProcessManifestFile(Config config)
        {
            // for this, we will need a SecretManager
            SecretManager secrets = new SecretManager();

            string sFullPathToManifest = Path.GetFullPath(config.ManifestFile);
            string sManifestDirectory = Path.GetDirectoryName(sFullPathToManifest);

            Stream stm = File.Open(sFullPathToManifest, FileMode.Open);
            SecretsFilesConfig filesConfig = SecretsConfigReader.CreateSecretsFilesConfig(stm);
            List<string> plsTargetFiles = new List<string>();

            foreach (SecretsFileConfig fileConfig in filesConfig.Files)
            {
                plsTargetFiles.Add(PathHelper.FullPathFromPaths(fileConfig.TargetFile, sManifestDirectory));

                // gather the secrets and look them up
                foreach (string secretID in fileConfig.PlaceholderToSecretID.Values)
                {
                    if (!filesConfig.SecretIDToSecret.ContainsKey(secretID))
                    {
                        if (!secrets.FFetchSecret(secretID, out string sSecret, out string sError))
                        {
                            Console.WriteLine($"Could not fetch secret '{config.ShowSecret}': {sError}");
                            if (!config.ContinueOnError)
                                throw new Exception("ContinueOnError not specified. halting");
                        }
                        else
                        {
                            Console.WriteLine($"Successfully fetch secret {secretID}");
                            filesConfig.AddSecret(secretID, sSecret);
                        }
                    }
                }
            }

            // at this point we have fetched all secrets

            // before we create the secrets files, lets make sure any .gitignore is updated
            // to ignore the files we are about to create (we don't want them getting committed)
            string sGitIgnorePath = Path.Combine(sManifestDirectory, ".gitignore");
            if (!config.NoGitIgnore && File.Exists(sGitIgnorePath))
            {
                GitIgnore ignore = GitIgnore.CreateGitIgnore(sGitIgnorePath, plsTargetFiles);

                string sGitIgnoreBackupPath = $"{sGitIgnorePath}.old";

                File.Move(sGitIgnorePath, sGitIgnoreBackupPath);
                using (StreamWriter sw = new StreamWriter(File.Create(sGitIgnorePath)))
                {
                    ignore.WriteGitIgnore(sw);
                    sw.Flush();
                    sw.Close();
                }
                File.Delete(sGitIgnoreBackupPath);
            }

            // ready to create each target file
            foreach (SecretsFileConfig fileConfig in filesConfig.Files)
            {
                string sFullPathToTargetFile = PathHelper.FullPathFromPaths(fileConfig.TargetFile, sManifestDirectory);
                string sFullPathToTemplateSource = sFullPathToManifest; // assume the template was embedded in the manifest

                if (fileConfig.TemplateFile != null && fileConfig.TargetFileContentTemplate == null)
                {
                    string sFullPath = PathHelper.FullPathFromPaths(fileConfig.TemplateFile, sManifestDirectory);

                    fileConfig.TargetFileContentTemplate = SecretsConfigReader.ReadStreamIntoString(File.Open(sFullPath, FileMode.Open));
                    sFullPathToTemplateSource = sFullPath; // now we know the template came from a template file
                }

                PathHelper.EnsureDirectoriesExist(sFullPathToTargetFile);

                StreamWriter sw = File.CreateText(sFullPathToTargetFile);
                
                sw.Write(fileConfig.TransformContentTemplate(filesConfig.SecretIDToSecret, sFullPathToTemplateSource));
                sw.Flush();
                sw.Close();
                Console.WriteLine($"Created file {sFullPathToTargetFile}, transformed from template...");
            }
        }
        static void ShowSecret(Config config)
        {
            // for this, we will need a SecretManager
            SecretManager secrets = new SecretManager();

            if (!secrets.FFetchSecret(config.ShowSecret, out string sSecret, out string sError))
            {
                Console.WriteLine($"Could not fetch secret '{config.ShowSecret}': {sError}");
            }
            else
            {
                Console.WriteLine($"'{config.ShowSecret}': {sSecret}");
            }
        }
    }
}
