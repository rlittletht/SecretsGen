
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml;
using Microsoft.Identity.Client;
using NUnit.Framework;
using XMLIO;

// [assembly: InternalsVisibleTo("SecretsConfigReader")]

namespace SecretsGen
{
    public class SecretsFileConfig
    {
        public Dictionary<string, string> PlaceholderToSecretID { get; internal set; } = new Dictionary<string, string>();
        
        public string TargetFile { get; internal set; }
        public string TargetFileContentTemplate { get; internal set; }
        public SecretsFileConfig() { }

        public void AddPlaceholder(string sPlaceholder, string sSecretID)
        {
            PlaceholderToSecretID.Add(sPlaceholder, sSecretID);
        }

        public string TransformContentTemplate(Dictionary<string, string> SecretIDToSecret)
        {
            string sTransformed = TargetFileContentTemplate;

            foreach (string sKey in PlaceholderToSecretID.Keys)
            {
                string sSecret = SecretIDToSecret[PlaceholderToSecretID[sKey]];

                sTransformed = sTransformed.Replace(sKey, sSecret);
            }

            return sTransformed.Replace("\n", Environment.NewLine);
        }

        #region Test transformations

        [Test]
        public static void TestTransformContentTemplate_MissingKey()
        {
            SecretsFileConfig fileConfig = new SecretsFileConfig();
            Dictionary<string, string> secrets = new Dictionary<string, string>();

            fileConfig.PlaceholderToSecretID.Add("$$placeholder1$$", "secret-id-1");
            fileConfig.TargetFileContentTemplate = "this is my $$placeholder1$$ test";

            Assert.Throws<KeyNotFoundException>(() => fileConfig.TransformContentTemplate(secrets));
        }

        [Test]
        public static void TestTransformContentTemplate_SingleSecret()
        {
            SecretsFileConfig fileConfig = new SecretsFileConfig();
            Dictionary<string, string> secrets = new Dictionary<string, string>();

            fileConfig.PlaceholderToSecretID.Add("$$placeholder1$$", "secret-id-1");
            secrets.Add("secret-id-1", "MYSECRET!");
            fileConfig.TargetFileContentTemplate = "this is my $$placeholder1$$ test";

            string sTransformed = fileConfig.TransformContentTemplate(secrets);
            Assert.AreEqual("this is my MYSECRET! test", sTransformed);
        }

        [Test]
        public static void TestTransformContentTemplate_TwoSecretsOneReplace()
        {
            SecretsFileConfig fileConfig = new SecretsFileConfig();
            Dictionary<string, string> secrets = new Dictionary<string, string>();

            fileConfig.PlaceholderToSecretID.Add("$$placeholder1$$", "secret-id-1");
            fileConfig.PlaceholderToSecretID.Add("$$placeholder2$$", "secret-id-2");
            secrets.Add("secret-id-1", "MYSECRET!");
            secrets.Add("secret-id-2", "MY2SECRET!");
            fileConfig.TargetFileContentTemplate = "this is my $$placeholder1$$ test";

            string sTransformed = fileConfig.TransformContentTemplate(secrets);
            Assert.AreEqual("this is my MYSECRET! test", sTransformed);
        }

        [Test]
        public static void TestTransformContentTemplate_TwoSecretsTwoReplaces()
        {
            SecretsFileConfig fileConfig = new SecretsFileConfig();
            Dictionary<string, string> secrets = new Dictionary<string, string>();

            fileConfig.PlaceholderToSecretID.Add("$$placeholder1$$", "secret-id-1");
            fileConfig.PlaceholderToSecretID.Add("$$placeholder2$$", "secret-id-2");
            secrets.Add("secret-id-1", "MYSECRET!");
            secrets.Add("secret-id-2", "MY2SECRET!");
            fileConfig.TargetFileContentTemplate = "this is my $$placeholder1$$ test $$placeholder2$$";

            string sTransformed = fileConfig.TransformContentTemplate(secrets);
            Assert.AreEqual("this is my MYSECRET! test MY2SECRET!", sTransformed);
        }

        #endregion

    }
}
