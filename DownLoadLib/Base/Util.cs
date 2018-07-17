using System;
using System.IO;
using System.Threading;
using System.Xml;

namespace DownLoadLib
{
    public class Util
    {
        public static void DeleteFileIfExists(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException("filePath");
            }
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (IOException)
                {
                    Thread.Sleep(500);
                    File.Delete(filePath);
                }
                catch (UnauthorizedAccessException)
                {
                    FileAttributes attributes = File.GetAttributes(filePath);
                    if ((attributes & FileAttributes.ReadOnly).Equals(FileAttributes.ReadOnly))
                    {
                        File.SetAttributes(filePath, attributes & ~FileAttributes.ReadOnly);
                        File.Delete(filePath);
                    }
                }
            }
        }

        #region xml操作

        public static XmlNodeList GetXmlNodeList(string filePath, string xpath)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(filePath);
            return xmlDoc.DocumentElement.SelectNodes(xpath);
        }

        public static string QueryLastModified(string fileName)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load("DownloadFileInfo.xml");
            XmlNodeList xmlNodes = xmlDoc.DocumentElement.SelectNodes("item[@key='" + fileName + "']");
            if (xmlNodes.Count > 0)
                return xmlNodes[0].Attributes["date"].Value;
            return null;
        }

        public static void DeleteLastModified(string fileName)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load("DownloadFileInfo.xml");
            XmlElement root = xmlDoc.DocumentElement;
            XmlElement selectEle = (XmlElement)root.SelectSingleNode("item[@key='" + fileName + "']");
            root.RemoveChild(selectEle);
            xmlDoc.Save("DownloadFileInfo.xml");
        }

        public static void CreateLastModified(string fileName, string date)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load("DownloadFileInfo.xml");
            XmlElement root = xmlDoc.DocumentElement;
            XmlElement conXml = xmlDoc.CreateElement("item");
            conXml.SetAttribute("key", fileName);
            conXml.SetAttribute("date", date);
            root.AppendChild(conXml);
            xmlDoc.Save("DownloadFileInfo.xml");
        }
        #endregion

}
}
