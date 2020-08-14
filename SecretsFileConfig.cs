
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using NUnit.Framework;
using XMLIO;

namespace SecretsGen
{
    class SecretsFileConfig
    {
        private Dictionary<string, string> m_mpPlaceholderToSecretID = new Dictionary<string, string>();
        private string TargetFile { get; set; }
        
        public SecretsFileConfig() { }

        /*----------------------------------------------------------------------------
        	%%Function: CreateSecretFileConfigFromXml
        	%%Qualified: SecretsGen.SecretsFileConfig.CreateSecretFileConfigFromXml
        	
        ----------------------------------------------------------------------------*/
        public static SecretsFileConfig CreateSecretFileConfigFromXml(XmlReader xr)
        {
            if (xr.Name == "secretsFileConfig")
            {
                if (xr.IsEmptyElement)
                    return null;

                // prepare read the attributes
                if (!XmlIO.Read(xr))
                    throw new Exception("can't unclosed secretFileConfig element");

                XmlIO.SkipNonContent(xr);
            }
            else
            {
                throw new Exception("parsing secretsFileConfig without <secretsFileConfig>");
            }

            SecretsFileConfig fileConfig = new SecretsFileConfig();

            // the reader should already be on the <secretFile>
            while (true)
            {
                if (xr.NodeType == XmlNodeType.Element)
                {
                    throw new Exception($"unknown element {xr.Name}");
                }

                if (xr.NodeType == XmlNodeType.Attribute)
                {
                    if (xr.Name == "targetFile")
                    {
                        fileConfig.TargetFile = XmlIO.ReadGenericStringElement(xr, "targetFile");
                        XmlIO.Read(xr);
                        XmlIO.SkipNonContent(xr);
                        continue;
                    }
                    else
                    {
                        throw new Exception($"bad attribute {xr.Name}");
                    }
                }
            }
        }

        [Test]
        public static void TestCreateSecretFileConfigFromXml_EmptyString()
        {
            SecretsFileConfig fileConfig;

            Assert.Throws<Exception>(
                () =>
                {
                    using (StringReader sr = new StringReader(""))
                    {
                        fileConfig = CreateSecretFileConfigFromXml(XmlReader.Create(sr));
                    }
                });
        }

        [Test]
        public static void TestCreateSecretFileConfigFromXml_EmptyElement()
        {
            SecretsFileConfig fileConfig;

            using (StringReader sr = new StringReader("<secretsFileConfig/>"))
            {
                XmlReader xr = XmlReader.Create(sr);
                XmlIO.Read(xr);
                XmlIO.SkipNonContent(xr);

                fileConfig = CreateSecretFileConfigFromXml(xr);
            }

            Assert.IsNull(fileConfig);
        }

    }
}
