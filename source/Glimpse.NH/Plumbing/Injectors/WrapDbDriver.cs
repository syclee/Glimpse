using System.Web.Configuration;
using System.Xml;
using Glimpse.Core.Extensibility;

namespace Glimpse.NH.Plumbing.Injectors
{
    public class WrapDbDriver
    {
        private IGlimpseLogger Logger { get; set; }

        public WrapDbDriver(IGlimpseLogger logger)
        {
            Logger = logger;
        }

        public void Inject()
        {
            var configuration = WebConfigurationManager.OpenWebConfiguration("~");
            //var doc = new XmlDocument(); 
            //var change = false; 
            //var configFile = Path.Combine(siteFolder, "web.config"); 
            //doc.Load(configFile);
            
            //var root = doc.DocumentElement;
            //var systemWebNode = (XmlNode)root["system.web"]; 
            ////find <globalization> node under system.web node
            //var globalizationNode = SelectGlobalizationNode(systemWebNode,doc); 
            //if (globalizationNode != null)
            //{  
            //    // found Globalization node in web.config.  So update Culture and UICulture attribute  
            //    ChangeXmlAttributeValue(doc, globalizationNode, "uiCulture", config.LanguageUICulture, ref change);   
            //    ChangeXmlAttributeValue(doc, globalizationNode, "culture", config.LanguageCulture, ref change);
            //}
            //else
            //{  
            //    //no globalization xmlnode in web.config.  So create globalization node and set culture and uiculture attribute Note: Pass doc.DocumentElement.NamespaceURI to namespaceURI method parameter. 
            //    // If you pass "" empty string you will end up generating <globalization xmlns="" uiCulture="es-MX" culture="es-MX" /> and this will throw an error "Unrecognized attribute 'xmlns'. Note that attribute names are case-sensitive. ".   
            //    var newGlobalizationNode = doc.CreateNode(XmlNodeType.Element, "globalization", doc.DocumentElement.NamespaceURI);
            //    newGlobalizationNode.Attributes.Append(CreateXmlAttribute(doc, "resourceProviderFactoryType", "Emmis.Common.Globalization.EmmisResourceProviderFactory"));  
            //    newGlobalizationNode.Attributes.Append(CreateXmlAttribute(doc, "uiCulture", config.LanguageUICulture));   
            //    newGlobalizationNode.Attributes.Append(CreateXmlAttribute(doc, "culture", config.LanguageCulture));   
                
            //    //Add <globalization> node under <system.web> node  systemWebNode.AppendChild(newGlobalizationNode);  
            //    change = true;
            //}

            //if (change)
            //{
            //    //Update web.config only if changes are made to web.config file
            //    try
            //    {
            //        doc.Save(configFile);
            //        Logger.Debug("Updated web.config file");
            //    }  
            //    catch (IOException ex)
            //    {
            //        Logger.Debug("Error occured while saving web.config changes.");
            //        Logger.Error(string.Format("Web.config update failed!  ExMsg: {0} {1} StackTrace: {2}", ex.Message, Environment.NewLine, ex.StackTrace));      
            //    }
            //}
            //else
            //{
            //    Logger.Debug("No need to update web.config file bcoz appsetting and globalization node values are accurate in web.config file");
            //}
        }

        private static XmlNode SelectGlobalizationNode(XmlNode systemWebNode, XmlDocument doc)
        { 
            XmlNamespaceManager xmlNamespaces = null;
            var xmlNamespacePrefix = ""; 
            
            //Get default namespace
            // this is very important part to get default name space. 
            // I was searching globalization node like this doc.SelectNodes("globalization") and this was working when I was passing empty string to namespaceURI method parameter while creating <globalization> node. Line#: . But as I mentioned line# that won't work.  So below method is very importand otherwise you won't be able to find </globalization> section. //To know more in detail about below method you can refere http://www.west-wind.com/Weblog/posts/2423.aspx
            GetXmlNameSpaceInfo(doc, ref xmlNamespacePrefix, ref xmlNamespaces); 
            
            //Search <globalizatio> node in <system.web> 
            var globalizationNode = systemWebNode.SelectSingleNode(xmlNamespacePrefix + "globalization", xmlNamespaces);  
            return globalizationNode;
        }

        private static void GetXmlNameSpaceInfo(XmlDocument dom, ref string xmlNamespacePrefix,ref XmlNamespaceManager xmlNamespaces)
        {  
            // *** Load up the Namespaces object so we can  
            // *** reference the appropriate default namespace  
            if (dom.DocumentElement.NamespaceURI == null || dom.DocumentElement.NamespaceURI == "")
            {
                xmlNamespaces = null;     
                xmlNamespacePrefix = "";
            }  
            else
            {
                xmlNamespacePrefix = string.IsNullOrEmpty(dom.DocumentElement.Prefix) ? "ww" : dom.DocumentElement.Prefix;       
                xmlNamespaces = new XmlNamespaceManager(dom.NameTable);      
                xmlNamespaces.AddNamespace(xmlNamespacePrefix, dom.DocumentElement.NamespaceURI);
                xmlNamespacePrefix += ":";
            }
        }

        private static XmlAttribute CreateXmlAttribute(XmlDocument doc, string attributeName, string attributeValue)
        {   
            var attribute = doc.CreateAttribute(attributeName); 
            attribute.Value = attributeValue;  
            return attribute;
        }

        private static void ChangeXmlAttributeValue(XmlDocument doc, XmlNode globalizationNode, string attributeName, string newAttributeValue, ref bool change)
        {
            var globalizationAttribute = globalizationNode.Attributes[attributeName];
            if (globalizationAttribute == null)
            {
                globalizationNode.Attributes.Append(CreateXmlAttribute(doc, attributeName, newAttributeValue)); 
                change = true;
            } 
            else
            {
                if (globalizationAttribute.Value != newAttributeValue)
                { 
                    globalizationAttribute.Value = newAttributeValue;
                    change = true; 
                }
            }
        }
    }
}