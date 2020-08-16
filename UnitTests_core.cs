

using System;
using System.IO;
using System.Xml;

namespace SecretsGen
{
    public class UnitTestCore
    {
        /*----------------------------------------------------------------------------
            %%Function: SetupXmlReaderForTest
        	%%Qualified: SecretsGen.UnitTestCore.SetupXmlReaderForTest
    
            take a static string representing an XML snippet, and wrap an XML reader
            around the string
    
            NOTE: This is not very efficient -- it decodes the string into bytes, then
            creates a memory stream (which ought to be disposed of eventually since
            it is based on IDisposable), and then we finally return. But, these are 
            tests and will run fast enough. Don't steal this code for production
            though.
        ----------------------------------------------------------------------------*/
        public static XmlReader SetupXmlReaderForTest(string sTestString)
        {
            return XmlReader.Create(new StringReader(sTestString));
        }

        /*----------------------------------------------------------------------------
        	%%Function: AdvanceReaderToTestContent
        	%%Qualified: SecretsGen.UnitTestCore.AdvanceReaderToTestContent
        	
        ----------------------------------------------------------------------------*/
        public static void AdvanceReaderToTestContent(XmlReader xr, string sElementTest)
        {
            XmlNodeType nt;

            while (xr.Read())
            {
                nt = xr.NodeType;
                if (nt == XmlNodeType.Element && xr.Name == sElementTest)
                    return;
            }

            throw new Exception($"could not advance to requested element '{sElementTest}'");
        }
    }
}