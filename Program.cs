using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            if (config.ManifestFile != null)
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

            foreach (SecretsFileConfig fileConfig in filesConfig.Files)
            {
                // gather the secrets and look them up
                foreach (string secretID in fileConfig.PlaceholderToSecretID.Values)
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
                        fileConfig.AddSecret(secretID, sSecret);
                    }
                }
            }

            // at this point we have fetched all secrets
            // ready to create each target file
            foreach (SecretsFileConfig fileConfig in filesConfig.Files)
            {
                string sFullPathToTargetFile = PathHelper.FullPathFromPaths(fileConfig.TargetFile, sManifestDirectory);

                PathHelper.EnsureDirectoriesExist(sFullPathToTargetFile);

                StreamWriter sw = File.CreateText(sFullPathToTargetFile);
                
                sw.Write(fileConfig.TransformContentTemplate());
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
