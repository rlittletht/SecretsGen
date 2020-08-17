
using System.Collections.Generic;

namespace SecretsGen
{
    public class SecretsFilesConfig
    {
        private List<SecretsFileConfig> m_files = new List<SecretsFileConfig>();
        public Dictionary<string, string> SecretIDToSecret { get; internal set; } = new Dictionary<string, string>();

        public void AddSecret(string sSecretID, string sSecret)
        {
            SecretIDToSecret.Add(sSecretID, sSecret);
        }

        public List<SecretsFileConfig> Files => m_files;

        public SecretsFilesConfig() { }
    }
}
