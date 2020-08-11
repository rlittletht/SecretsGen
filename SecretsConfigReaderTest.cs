
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
    }
}