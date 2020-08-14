
using System;
using System.IO;
using System.Xml;
using XMLIO;

namespace SecretsGen
{
    public partial class SecretsConfigReader
    {
        public SecretsConfigReader() { }

        static SecretsFilesConfig CreateSecretsFilesConfigFromXml(XmlReader xr)
        {
            // if the config is empty, return null
            if (!XmlIO.Read(xr))
                return null;

            XmlIO.SkipNonContent(xr);

            if (xr.NodeType != XmlNodeType.Element)
                throw new Exception("no root element");

            if (xr.Name != "secretsFilesConfig")
                throw new Exception("bad root element in secretsConfig");

            SecretsFilesConfig filesConfig = new SecretsFilesConfig();

            if (xr.IsEmptyElement)
                return filesConfig; // just return an empty config

            if (!XmlIO.Read(xr))
                throw new Exception("can't find end of start root element");

            XmlIO.SkipNonContent(xr);

            while (xr.NodeType != XmlNodeType.Element)
            {
                // we don't expect any attributes, but we will be tolerant
                if (xr.NodeType != XmlNodeType.Attribute)
                    throw new Exception("non attribute on root element");

                if (!XmlIO.Read(xr))
                    throw new Exception("can't find end of start root element");

                XmlIO.SkipNonContent(xr);
            }

            while (true)
            {
                if (xr.NodeType == XmlNodeType.Element)
                {
                    // now we are inside the root element
                    if (xr.Name == "secretsFile")
                    {
                        //
                    }
                    continue;
                }

                if (xr.NodeType == XmlNodeType.EndElement)
                {
                    if (xr.Name != "secretsFilesConfig")
                        throw new Exception($"end tag {xr.Name} does not match start tag <secretsFilesConfig>");

                    break;
                }
                XmlIO.Read(xr);
            }

            return null;
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