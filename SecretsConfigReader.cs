
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using NUnit.Framework;
using XMLIO;

namespace SecretsGen
{
    public partial class SecretsConfigReader
    {
        public SecretsConfigReader() { }

        /*----------------------------------------------------------------------------
        	%%Function: FParseSecretsFilesConfigElements
        	%%Qualified: SecretsGen.SecretsConfigReader.FParseSecretsFilesConfigElements
        	
            <secretsFilesConfig>
                <secretsFileConfig>...</secretsFileConfig>
            </secretsFilesConfig>
        ----------------------------------------------------------------------------*/
        static bool FParseSecretsFilesConfigElements(XmlReader xr, string sElement, SecretsFilesConfig filesConfig)
        {
            if (sElement == "secretsFileConfig")
            {
                SecretsFileConfig fileConfig = fileConfig = CreateSecretFileConfigFromXml(xr);

                if (fileConfig != null)
                    filesConfig.Files.Add(fileConfig);

                return true;
            }

            throw new Exception($"unknown element {sElement}");
        }

        /*----------------------------------------------------------------------------
        	%%Function: CreateSecretsFilesConfigFromXml
        	%%Qualified: SecretsGen.SecretsConfigReader.CreateSecretsFilesConfigFromXml
        ----------------------------------------------------------------------------*/
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

        /*----------------------------------------------------------------------------
        	%%Function: CreateSecretsFilesConfig
        	%%Qualified: SecretsGen.SecretsConfigReader.CreateSecretsFilesConfig
        	
        ----------------------------------------------------------------------------*/
        public static SecretsFilesConfig CreateSecretsFilesConfig(string xml)
        {
            using (StringReader sr = new StringReader(xml))
            {
                return CreateSecretsFilesConfigFromXml(XmlReader.Create(sr));
            }
        }

        /*----------------------------------------------------------------------------
        	%%Function: CreateSecretsFilesConfig
        	%%Qualified: SecretsGen.SecretsConfigReader.CreateSecretsFilesConfig
        	
        ----------------------------------------------------------------------------*/
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

        #region File Parsing

        /*----------------------------------------------------------------------------
        	%%Function: FProcessSecretsFileAttributes
        	%%Qualified: SecretsGen.SecretsConfigReader.FProcessSecretsFileConfigAttributes
        	
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
        	%%Qualified: SecretsGen.SecretsConfigReader.FProcessSecretAttributes
        	
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
        	%%Qualified: SecretsGen.SecretsConfigReader.FParseSecretsElement
        	
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
        	%%Qualified: SecretsGen.SecretsConfigReader.FParseSecretsFileConfigElements
        	
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
        	%%Qualified: SecretsGen.SecretsConfigReader.CreateSecretFileConfigFromXml
        	
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
        	%%Qualified: SecretsGen.SecretsConfigReader.CreateSecretFileConfigFromXml
        	
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

    }
}