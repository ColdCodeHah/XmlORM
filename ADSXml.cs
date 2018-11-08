using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Reflection;
using System.Linq.Expressions;

namespace ADS.Utilities.Xml
{
    /*2018-10-29 tangdj
     * 调用示例
     *              ADSXml xml = new ADSXml(@"E:\Users\G50-80\Desktop\Config.xml");
                    Device_Standard d= xml.Get<Device_Standard>();
                    d.FlowterName = "罗斯蒙特";
                    xml.Save<Device_Standard>(d);
                    xml.SaveSingle<Device_Standard>(it=>new Device_Standard {FlowterName="罗斯蒙特" });


                    [ADSXmlNode("Device_Standard")]
                    public class Device_Standard
                    {
                        public string FlowterName { get; set; }
                        public string FlowterDes { get; set; }
                        public string FlowterSN { get; set; }
                        public string FlowComNo { get; set; }
                        public string TCPomNo { get; set; }
                        public string DataBit { get; set; }
                        public string BaudRate { get; set; }
                        public string ParityBit { get; set; }
                        public string StopBit { get; set; }
                        public string AddressNo { get; set; }
                        public string ByteOrder { get; set; }
                        public string Price { get; set; }
                        public string IsCheck { get; set; }
                        public string CheckDays { get; set; }
                        public string RegisterCode { get; set; }
                    }
     * */


    /// <summary>
    /// ADSXml 的摘要说明。
    /// xml操作类
    /// </summary>
    public class ADSXml
    {
        protected string strXmlFile;
        protected XmlDocument objXmlDoc = new XmlDocument();
        /// <summary>
        /// xml文本内容
        /// </summary>
        public Dictionary<string, object> XmlContent = new Dictionary<string, object>();
        protected XmlNode NodeCurrent;
        protected Dictionary<string, object> DictionaryCurrent;
        public ADSXml(string XmlFile)
        {
            try
            {
                objXmlDoc.Load(XmlFile);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            strXmlFile = XmlFile;
        }

        /// <summary>
        /// 获取Xml结果
        /// </summary>
        /// <typeparam name="TResult">Model</typeparam>
        /// <param name="nodeName">可指定节点名称</param>
        /// <returns></returns>
        public TResult Get<TResult>(string nodeName=null)
        {
            getData();
            Type type = typeof(TResult);
            TResult obj = (TResult)Activator.CreateInstance(type);
            PropertyInfo[] propers = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            if (nodeName==null)
            {
                foreach (var m in type.GetCustomAttributes(true))
                {
                    if (m is ADSXmlNode)
                    {
                        ADSXmlNode node = (ADSXmlNode)m;
                        nodeName = node.NodeName;
                    }
                }
            }
            if (nodeName==null)
                throw new Exception("未指定类的节点特性");                
            //Dictionary<string, object> dic = (Dictionary<string, object>)XmlContent[nodeName];
            findDictionary(XmlContent, nodeName);
            if (!DictionaryCurrent.ContainsKey(nodeName))
                throw new Exception("Xml文件不存在指定节点");
            Dictionary<string, object> dic = (Dictionary<string, object>)DictionaryCurrent[nodeName];
            foreach (PropertyInfo p in propers)
            {
                if (dic.ContainsKey(p.Name))
                {
                    p.SetValue(obj, dic[p.Name], null);
                }
            }
            return obj;            
        }
        
        /// <summary>
        /// 保存单个叶子节点
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="exp"></param>
        /// <param name="nodeName"></param>
        /// <returns></returns>
        public bool SaveSingle<T>(Expression<Func<T, T>> exp,string nodeName=null)
        {
            Type type = typeof(T);
            if (nodeName == null)
            {
                foreach (var m in type.GetCustomAttributes(true))
                {
                    if (m is ADSXmlNode)
                    {
                        ADSXmlNode node = (ADSXmlNode)m;
                        nodeName = node.NodeName;
                    }
                }
            }
            if (nodeName == null)
                throw new Exception("未指定类的节点特性");
            //XmlNode xmlNode = objXmlDoc.SelectSingleNode("Root/Basic/" + nodeName);
            findNode(objXmlDoc, nodeName);
            if (!NodeCurrent.Name.Equals(nodeName))
                return false;

            MemberInitExpression body = (MemberInitExpression)exp.Body;
            foreach(MemberAssignment item in body.Bindings)
            {
                var name = item.Member.Name;
                ConstantExpression single = (ConstantExpression)item.Expression;
                var value = single.Value;
                NodeCurrent.SelectSingleNode(name).InnerText = value.ToString();
            }
            objXmlDoc.Save(strXmlFile);
            return true;
        }

        /// <summary>
        /// 以实体类为单位进行保存
        /// 对象属性为Null的，不保存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="nodeName"></param>
        /// <returns></returns>
        public int Save<T>(T obj, string nodeName = null)
        {
            int result = 0;
            Type type = typeof(T);            
            PropertyInfo[] propers = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            if (nodeName == null)
            {
                foreach (var m in type.GetCustomAttributes(true))
                {
                    if (m is ADSXmlNode)
                    {
                        ADSXmlNode node = (ADSXmlNode)m;
                        nodeName = node.NodeName;
                    }
                }
            }
            if (nodeName == null)
                throw new Exception("未指定类的节点特性");
            //XmlNode xmlNode= objXmlDoc.SelectSingleNode("Root/Basic/"+nodeName);
            findNode(objXmlDoc, nodeName);
            if (!NodeCurrent.Name.Equals(nodeName))
                return 0;
            foreach(PropertyInfo p in propers)
            {
                var value=p.GetValue(obj, null);
                if (value != null)
                {
                    NodeCurrent.SelectSingleNode(p.Name).InnerText = value.ToString();
                    result++;
                }
            }
            objXmlDoc.Save(strXmlFile);
            return result;
        }       


        protected void getData()
        {
            Dictionary<string, object> dic = XmlContent;
            readData(objXmlDoc, dic);
        }

        protected void findNode(XmlNode xmldoc,string node)
        {
            XmlNode xml= xmldoc.SelectSingleNode(node);
            if (xml == null)
            {
                foreach (XmlNode m in xmldoc.ChildNodes)
                {
                    findNode(m, node);
                }
            }
            else
            {
                NodeCurrent = xml;
                return;
            }
        }

        protected void findDictionary(Dictionary<string ,object> dic,string key)
        {
            if (!dic.ContainsKey(key))
            {
                foreach(var m in dic)
                {
                    try
                    {
                        findDictionary((Dictionary<string, object>)m.Value, key);
                    }
                    catch { }
                }
            }
            else
            {
                DictionaryCurrent = dic;
                return;
            }
        }


        protected void readData(XmlNode node, Dictionary<string, object> dicValue)
        {
            XmlNodeList childNode = node.ChildNodes;
            try
            {
                if ((childNode.Count == 1 && !childNode[0].HasChildNodes)||childNode.Count==0)//叶子节点
                {
                    dicValue[node.Name] = node.FirstChild.Value;
                    return;
                }
            }
            catch
            {
                return;
            }
            Dictionary<string, object> dic = new Dictionary<string, object>();
            foreach (XmlNode m in childNode)
            {
                readData(m, dic);
            }
            dicValue[node.Name] = dic;
        }

    }
}
