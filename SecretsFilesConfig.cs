
using System.Collections.Generic;

namespace SecretsGen
{
    public class SecretsFilesConfig
    {
        private List<SecretsFileConfig> m_files = new List<SecretsFileConfig>();

        public List<SecretsFileConfig> Files => m_files;

        public SecretsFilesConfig() { }
    }
}
