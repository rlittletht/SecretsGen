using System;
using System.Collections.Generic;
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
            args = new[] {"-Secret", "AzureSql-ConnectionString/09fbb405e7b2434ea50979b6b6e92cf1" };

            Config config = new Config();
            CmdLine cmdLine = new CmdLine(Config.s_CmdLineConfig);

            if (!cmdLine.FParse(args, config, null, out string sError))
            {
                cmdLine.Usage(Console.WriteLine);
                return;
            }

            if (config.ShowSecret != null)
                ShowSecret(config);
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
