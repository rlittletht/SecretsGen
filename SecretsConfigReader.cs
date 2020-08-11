
using System;
using System.IO;
using System.Xml;

namespace SecretsGen
{
    public partial class SecretsConfigReader
    {
        public SecretsConfigReader() { }

        static SecretsFilesConfig CreateSecretsFilesConfigFromDom(XmlDocument dom)
        {
            return null;
        }

        public static SecretsFilesConfig CreateSecretsFilesConfig(string xml)
        {
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(xml);
            }
            catch (Exception e)
            {
                Exception eGeneric = new Exception(e.Message);

                throw eGeneric;
            }

            return CreateSecretsFilesConfigFromDom(dom);
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

            return CreateSecretsFilesConfigFromDom(dom);
        }
    }
}