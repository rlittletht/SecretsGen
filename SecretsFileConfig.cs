
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Microsoft.Identity.Client;
using NUnit.Framework;
using XMLIO;

namespace SecretsGen
{
    class SecretsFileConfig
    {
        private Dictionary<string, string> m_mpPlaceholderToSecretID = new Dictionary<string, string>();
        private Dictionary<string, string> m_mpSecretIDSecret = new Dictionary<string, string>();

        private string TargetFile { get; set; }
        private string TargetFileContentTemplate { get; set; }
        public SecretsFileConfig() { }

        public void AddPlaceholder(string sPlaceholder, string sSecretID)
        {
            m_mpPlaceholderToSecretID.Add(sPlaceholder, sSecretID);
        }


        #region File Parsing

        /*----------------------------------------------------------------------------
        	%%Function: FProcessSecretsFileAttributes
        	%%Qualified: SecretsGen.SecretsFileConfig.FProcessSecretsFileConfigAttributes
        	
            <secretsFile targetFile="...">
        ----------------------------------------------------------------------------*/
        public static bool FProcessSecretsFileAttributes(string sAttribute, string sValue, SecretsFileConfig fileConfig)
        {
            if (sAttribute == "targetFile")
            {
                bool fRet = XmlIO.FProcessGenericValue(sValue, out string sTargetFile, (string)null);

                if (fRet)
                    fileConfig.TargetFile = sTargetFile;

                return fRet;
            }

            return false;
        }

        /*----------------------------------------------------------------------------
        	%%Function: FProcessSecretAttributes
        	%%Qualified: SecretsGen.SecretsFileConfig.FProcessSecretAttributes
        	
            <secret placeholder="...">
        ----------------------------------------------------------------------------*/
        public static bool FProcessSecretAttributes(string sAttributes, string sValue, Dictionary<string, string> attrs)
        {
            if (sAttributes == "placeholder")
            {
                attrs.Add("placeholder", sValue);
                return true;
            }

            return false;
        }

        /*----------------------------------------------------------------------------
        	%%Function: FParseSecretsElement
        	%%Qualified: SecretsGen.SecretsFileConfig.FParseSecretsElement
        	
            <secrets>
                <secret>...</secret>
            </secrets>
        ----------------------------------------------------------------------------*/
        public static bool FParseSecretsElement(XmlReader xr, string sElement, SecretsFileConfig fileConfig)
        {
            if (sElement == "secret")
            {
                XmlIO.ContentCollector contentCollector = new XmlIO.ContentCollector();
                Dictionary<string, string> attrs = new Dictionary<string, string>();

                if (XmlIO.FReadElement(xr, attrs, sElement, FProcessSecretAttributes, null, contentCollector))
                {
                    fileConfig.AddPlaceholder(attrs["placeholder"], contentCollector.ToString());
                    return true;
                }

                return false;
            }

            throw new Exception($"unknown element: {sElement}");
        }

        /*----------------------------------------------------------------------------
        	%%Function: FParseSecretsFileConfigElements
        	%%Qualified: SecretsGen.SecretsFileConfig.FParseSecretsFileConfigElements
        	
            <secretsFileConfig>
                <secrets>...</secrets>
                <template>...</template>
            </secretsFileConfig>
        ----------------------------------------------------------------------------*/
        public static bool FParseSecretsFileConfigElements(XmlReader xr, string sElement, SecretsFileConfig fileConfig)
        {
            if (sElement == "secrets")
            {
                // parse the secrets element
                return XmlIO.FReadElement(xr, fileConfig, sElement, null, FParseSecretsElement);
            }

            if (sElement == "template")
            {
                // this is a content block (likely cdata) that represents the content for the file we want
                // to create. it contains no elements or attributes, only content

                XmlIO.ContentCollector contentCollector = new XmlIO.ContentCollector();
                XmlIO.FReadElement<object>(xr, null, sElement, null, null, contentCollector);

                fileConfig.TargetFileContentTemplate = contentCollector.ToString();
                return true;
            }

            throw new Exception($"unknown element: {sElement}");
        }

        /*----------------------------------------------------------------------------
        	%%Function: CreateSecretFileConfigFromXml
        	%%Qualified: SecretsGen.SecretsFileConfig.CreateSecretFileConfigFromXml
        	
            Create a SecretsFileConfig from the given XmlReader
        ----------------------------------------------------------------------------*/
        public static SecretsFileConfig CreateSecretFileConfigFromXml(XmlReader xr)
        {
            SecretsFileConfig fileConfig = new SecretsFileConfig();

            if (!XmlIO.FReadElement<SecretsFileConfig>(xr, fileConfig, "secretsFileConfig", FProcessSecretsFileAttributes, FParseSecretsFileConfigElements))
                return null;

            return fileConfig;
        }

        /*----------------------------------------------------------------------------
        	%%Function: CreateSecretFileConfigFromXml
        	%%Qualified: SecretsGen.SecretsFileConfig.CreateSecretFileConfigFromXml
        	
        ----------------------------------------------------------------------------*/
        public static SecretsFileConfig CreateSecretFileConfigFromXmlNonTemplate(XmlReader xr)
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
        #endregion

        #region File I/O Tests

        [Test]
        public static void TestCreateSecretFileConfigFromXml_EmptyString()
        {
            SecretsFileConfig fileConfig;
            XmlReader xr = UnitTestCore.SetupXmlReaderForTest("");

            Assert.Throws<Exception>(
                () => { fileConfig = CreateSecretFileConfigFromXmlNonTemplate(xr); });
        }

        [Test]
        public static void TestCreateSecretFileConfigFromXml_EmptyElement()
        {
            SecretsFileConfig fileConfig;

            XmlReader xr = UnitTestCore.SetupXmlReaderForTest("<secretsFileConfig/>");
            UnitTestCore.AdvanceReaderToTestContent(xr, "secretsFileConfig");

            fileConfig = CreateSecretFileConfigFromXmlNonTemplate(xr);
            Assert.IsNull(fileConfig);
        }

        [Test]
        public static void TestCreateSecretFileConfigFromXmlTemplate_EmptyString()
        {
            SecretsFileConfig fileConfig;
            XmlReader xr = UnitTestCore.SetupXmlReaderForTest("");

            Assert.Throws<Exception>(
                () => { fileConfig = CreateSecretFileConfigFromXml(xr); });
        }

        [Test]
        public static void TestCreateSecretFileConfigFromXmlTemplate_EmptyElement()
        {
            SecretsFileConfig fileConfig;

            XmlReader xr = UnitTestCore.SetupXmlReaderForTest("<secretsFileConfig/>");
            UnitTestCore.AdvanceReaderToTestContent(xr, "secretsFileConfig");

            fileConfig = CreateSecretFileConfigFromXml(xr);
            Assert.IsNull(fileConfig);
        }

        [Test]
        public static void TestCreateSecretFileConfigFromXmlTemplate_SecretNoTemplate()
        {
            SecretsFileConfig fileConfig;

            XmlReader xr = UnitTestCore.SetupXmlReaderForTest("<secretsFileConfig><secrets><secret placeholder=\"$$$connectionString$$$\">test-connection-string</secret></secrets></secretsFileConfig>");
            UnitTestCore.AdvanceReaderToTestContent(xr, "secretsFileConfig");

            fileConfig = CreateSecretFileConfigFromXml(xr);
            Assert.IsNotNull(fileConfig);
            Assert.AreEqual(1, fileConfig.m_mpPlaceholderToSecretID.Keys.Count);
            Assert.IsTrue(fileConfig.m_mpPlaceholderToSecretID.ContainsKey("$$$connectionString$$$"));
            Assert.AreEqual("test-connection-string", fileConfig.m_mpPlaceholderToSecretID["$$$connectionString$$$"]);
        }

        [Test]
        public static void TestCreateSecretFileConfigFromXmlTemplate_SecretWithTemplate()
        {
            SecretsFileConfig fileConfig;

            XmlReader xr = UnitTestCore.SetupXmlReaderForTest("<secretsFileConfig><secrets><secret placeholder=\"$$$connectionString$$$\">test-connection-string</secret></secrets><template><![CDATA[<secretTargetFile><connectionString=\"$$$connectionString$$$\"/></secretTargetFile>]]></template></secretsFileConfig>");
            UnitTestCore.AdvanceReaderToTestContent(xr, "secretsFileConfig");

            fileConfig = CreateSecretFileConfigFromXml(xr);
            Assert.IsNotNull(fileConfig);
            Assert.AreEqual(1, fileConfig.m_mpPlaceholderToSecretID.Keys.Count);
            Assert.IsTrue(fileConfig.m_mpPlaceholderToSecretID.ContainsKey("$$$connectionString$$$"));
            Assert.AreEqual("test-connection-string", fileConfig.m_mpPlaceholderToSecretID["$$$connectionString$$$"]);
            Assert.AreEqual("<secretTargetFile><connectionString=\"$$$connectionString$$$\"/></secretTargetFile>", fileConfig.TargetFileContentTemplate);
        }
        #endregion

        public string TransformContentTemplate()
        {
            string sTransformed = TargetFileContentTemplate;

            foreach (string sKey in m_mpPlaceholderToSecretID.Keys)
            {
                string sSecret = m_mpSecretIDSecret[m_mpPlaceholderToSecretID[sKey]];

                sTransformed = sTransformed.Replace(sKey, sSecret);
            }

            return sTransformed;
        }

        #region Test transformations

        [Test]
        public static void TestTransformContentTemplate_MissingKey()
        {
            SecretsFileConfig fileConfig = new SecretsFileConfig();
            fileConfig.m_mpPlaceholderToSecretID.Add("$$placeholder1$$", "secret-id-1");
            fileConfig.TargetFileContentTemplate = "this is my $$placeholder1$$ test";

            Assert.Throws<KeyNotFoundException>(() => fileConfig.TransformContentTemplate());
        }

        [Test]
        public static void TestTransformContentTemplate_SingleSecret()
        {
            SecretsFileConfig fileConfig = new SecretsFileConfig();
            fileConfig.m_mpPlaceholderToSecretID.Add("$$placeholder1$$", "secret-id-1");
            fileConfig.m_mpSecretIDSecret.Add("secret-id-1", "MYSECRET!");
            fileConfig.TargetFileContentTemplate = "this is my $$placeholder1$$ test";

            string sTransformed = fileConfig.TransformContentTemplate();
            Assert.AreEqual("this is my MYSECRET! test", sTransformed);
        }

        [Test]
        public static void TestTransformContentTemplate_TwoSecretsOneReplace()
        {
            SecretsFileConfig fileConfig = new SecretsFileConfig();
            fileConfig.m_mpPlaceholderToSecretID.Add("$$placeholder1$$", "secret-id-1");
            fileConfig.m_mpPlaceholderToSecretID.Add("$$placeholder2$$", "secret-id-2");
            fileConfig.m_mpSecretIDSecret.Add("secret-id-1", "MYSECRET!");
            fileConfig.m_mpSecretIDSecret.Add("secret-id-2", "MY2SECRET!");
            fileConfig.TargetFileContentTemplate = "this is my $$placeholder1$$ test";

            string sTransformed = fileConfig.TransformContentTemplate();
            Assert.AreEqual("this is my MYSECRET! test", sTransformed);
        }

        [Test]
        public static void TestTransformContentTemplate_TwoSecretsTwoReplaces()
        {
            SecretsFileConfig fileConfig = new SecretsFileConfig();
            fileConfig.m_mpPlaceholderToSecretID.Add("$$placeholder1$$", "secret-id-1");
            fileConfig.m_mpPlaceholderToSecretID.Add("$$placeholder2$$", "secret-id-2");
            fileConfig.m_mpSecretIDSecret.Add("secret-id-1", "MYSECRET!");
            fileConfig.m_mpSecretIDSecret.Add("secret-id-2", "MY2SECRET!");
            fileConfig.TargetFileContentTemplate = "this is my $$placeholder1$$ test $$placeholder2$$";

            string sTransformed = fileConfig.TransformContentTemplate();
            Assert.AreEqual("this is my MYSECRET! test MY2SECRET!", sTransformed);
        }

        #endregion

    }
}
