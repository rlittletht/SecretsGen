using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCore.CmdLine;
using TCore.KeyVault;

namespace SecretsGen
{
    public class SecretManager
    {
        private string m_sVault;
        private string m_sAppTenant;
        private string m_sAppID;

        public SecretManager()
        {
            m_sVault = Config.s_KeyVault;
            m_sAppID = Config.s_AppID;
            m_sAppTenant = Config.s_AppTenant;

            CreateClient();
        }

        public SecretManager(string sVault, string sAppTenant, string sAppID)
        {
            m_sVault = sVault;
            m_sAppTenant = sAppTenant;
            m_sAppID = sAppID;

            CreateClient();
        }

        public SecretManager(string sAppTenant, string sAppID)
        {
            m_sVault = Config.s_KeyVault;
            m_sAppTenant = sAppTenant;
            m_sAppID = sAppID;

            CreateClient();
        }

        private Client m_clientKeyVault;

        void CreateClient()
        {
            m_clientKeyVault = new Client(m_sAppTenant, m_sAppID, m_sVault);
        }

        public bool FFetchSecret(string sSecretID, out string sSecret, out string sError)
        {
            Task<string> task = m_clientKeyVault.GetSecret(sSecretID);
            sError = null;

            try
            {
                task.Wait();
                sSecret = task.Result;
            }
            catch (Exception e)
            {
                sError = e.InnerException != null ? $"{e.Message}: {e.InnerException.Message}" : e.Message;
                sSecret = null;
            }

            return sSecret != null;
        }
    }
}