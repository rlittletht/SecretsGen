
using System;
using TCore.CmdLine;

namespace SecretsGen
{
    // to hold parsed parameters for all features
    class Config : ICmdLineDispatch
    {
        public static string s_AppID = "bfbaffd7-2217-4deb-a85a-4f697e6bdf94";
        public static string s_AppTenant = "b90f9ef3-5e11-43e0-a75c-1f45e6b223fb";
        public static string s_KeyVault = "https://thetasoft.vault.azure.net/";

        public string ShowSecret { get; private set; }
        public string ManifestFile { get; private set; }
        public bool ContinueOnError { get; private set; }

        public static CmdLineConfig s_CmdLineConfig = new CmdLineConfig(new CmdLineSwitch[]
        {
            new CmdLineSwitch("Secret", false, false, "SecretID to fetch from Azure keyvault", "SecretID", null),
            new CmdLineSwitch("Manifest", false, false, "Manifest file of secrets files to create", "Manifest File", null),
            new CmdLineSwitch("ContinueOnError", true, false, "Continue processing file if a secret can't be fetched", "Continue on error", null), 
        });

        public bool FDispatchCmdLineSwitch(TCore.CmdLine.CmdLineSwitch cls, string sParam, object oClient, out string sError)
        {
            sError = "";

            if (cls.Switch == "Secret")
            {
                ShowSecret = sParam;
            } else if (cls.Switch == "Manifest")
            {
                ManifestFile = sParam;
            } 
            else if (cls.Switch == "ContinueOnError")
            {
                ContinueOnError = true;
            }
            else
            {
                sError = $"Invalid switch {cls.Switch}";
                return false;
            }

            return true;
        }
    }

}
