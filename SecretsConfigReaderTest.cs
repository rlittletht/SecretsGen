
using System;
using System.IO;
using System.Xml;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace SecretsGen
{
    public partial class SecretsConfigReader
    {
        [Test]
        public static void TestCreateSecretsFilesConfig_InvalidXml()
        {
            // this should throw an exception
            Assert.Throws<Exception>(() => CreateSecretsFilesConfig("<invalidXml>"));
        }

        [Test]
        public static void TestCreateSecretsFilesConfig_Empty()
        {
            SecretsFilesConfig config = CreateSecretsFilesConfig("<secretsFilesConfig/>");

            Assert.IsNotNull(config);
        }

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
            Assert.AreEqual(1, fileConfig.PlaceholderToSecretID.Keys.Count);
            Assert.IsTrue(fileConfig.PlaceholderToSecretID.ContainsKey("$$$connectionString$$$"));
            Assert.AreEqual("test-connection-string", fileConfig.PlaceholderToSecretID["$$$connectionString$$$"]);
        }

        [Test]
        public static void TestCreateSecretFileConfigFromXmlTemplate_SecretWithTemplate()
        {
            SecretsFileConfig fileConfig;

            XmlReader xr = UnitTestCore.SetupXmlReaderForTest("<?xml version=\"1.0\"?><secretsFileConfig><secrets><secret placeholder=\"$$$connectionString$$$\">test-connection-string</secret></secrets><template><![CDATA[<secretTargetFile><connectionString=\"$$$connectionString$$$\"/></secretTargetFile>]]></template></secretsFileConfig>");
            UnitTestCore.AdvanceReaderToTestContent(xr, "secretsFileConfig");

            fileConfig = CreateSecretFileConfigFromXml(xr);
            Assert.IsNotNull(fileConfig);
            Assert.AreEqual(1, fileConfig.PlaceholderToSecretID.Keys.Count);
            Assert.IsTrue(fileConfig.PlaceholderToSecretID.ContainsKey("$$$connectionString$$$"));
            Assert.AreEqual("test-connection-string", fileConfig.PlaceholderToSecretID["$$$connectionString$$$"]);
            Assert.AreEqual("<secretTargetFile><connectionString=\"$$$connectionString$$$\"/></secretTargetFile>", fileConfig.TargetFileContentTemplate);
        }
        #endregion

    }
}