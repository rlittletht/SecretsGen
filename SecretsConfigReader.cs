
using System;
using System.IO;
using System.Xml;
using XMLIO;

namespace SecretsGen
{
    public partial class SecretsConfigReader
    {
        public SecretsConfigReader() { }

        static bool FParseSecretsFilesConfigElements(XmlReader xr, string sElement, SecretsFilesConfig filesConfig)
        {
            if (sElement == "secretsFileConfig")
            {
                SecretsFileConfig fileConfig = fileConfig = SecretsFileConfig.CreateSecretFileConfigFromXml(xr);

                if (fileConfig != null)
                    filesConfig.Files.Add(fileConfig);

                return true;
            }

            throw new Exception($"unknown element {sElement}");
        }

        static SecretsFilesConfig CreateSecretsFilesConfigFromXml(XmlReader xr)
        {
            // if the config is empty, return null
            if (!XmlIO.Read(xr))
                return null;

            XmlIO.SkipNonContent(xr);
            SecretsFilesConfig filesConfig = new SecretsFilesConfig();

            XmlIO.FReadElement(xr, filesConfig, "secretsFilesConfig", null, FParseSecretsFilesConfigElements);

            return filesConfig;
        }

        public static SecretsFilesConfig CreateSecretsFilesConfig(string xml)
        {
            using (StringReader sr = new StringReader(xml))
            {
                return CreateSecretsFilesConfigFromXml(XmlReader.Create(sr));
            }
        }

        public static SecretsFilesConfig CreateSecretsFilesConfig(Stream stm)
        {
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(stm);
            }
            catch (Exception e)
            {
                Exception eGeneric = new Exception(e.Message);

                throw eGeneric;
            }

            return CreateSecretsFilesConfigFromXml(XmlReader.Create(stm));
        }
    }
}