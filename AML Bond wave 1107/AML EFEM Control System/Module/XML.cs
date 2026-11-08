using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using System.Windows.Forms;
using System.Collections;

namespace AML
{
    class XML
    {
        /// <summary>
        /// 建立以 _file 為檔名的XML檔案。  Ex.  CreateFile("test");
        /// </summary>
        /// <param name="_file"></param>  檔名開頭不能為數字
        public static void CreateFile(string _file)
        {
            XmlDocument Doc = new XmlDocument();
            if (!File.Exists(_file + ".xml"))
            {
                XmlElement Element = Doc.CreateElement(_file);
                Doc.AppendChild(Element);
                Doc.Save(_file + ".xml");
            }
        }
        /// <summary>
        /// 在節點 _node 下新增節點 _node1 ，所讀取之XML檔案名稱為 _node 第一部分。Ex.  CreateNode("test/abc","def");  test即為檔名
        /// </summary>
        /// <param name="_node"></param>  名字開頭不能為數字
        /// <param name="_subnode"></param>  名字開頭不能為數字
        /*
        public static void CreateNode(string _node, string _subnode)
        {
            XmlDocument Doc = new XmlDocument();
            string File = _node.Split('/')[0];
            Doc.Load(File + ".xml");
            XmlNode Node = Doc.SelectSingleNode(_node);
            if (Node == null)
                return;

            XmlElement Node1 = Doc.CreateElement(_subnode);
            Node.AppendChild(Node1);
            Doc.Save(File + ".xml");
        }
        */
        /// <summary>
        /// 子節點下新增描述。EX.  CreateStatement("test/dog/taiwandog","color","black"); 
        /// </summary>
        /// <param name="_node"></param>  開頭都不能為數字
        /// <param name="_subtitle"></param>  開頭都不能為數字
        /// <param name="_statement"></param>
        public static void CreateStatement(string _node, string _subtitle, string _statement)
        {
            XmlDocument Doc = new XmlDocument();
            string File = _node.Split('/')[0];
            string SubNode = _node.Split('/')[_node.Split('/').Length - 1];
            Doc.Load(File + ".xml");
            XmlNode Node = Doc.SelectSingleNode(_node);
            if (Node == null)
                return;

            XmlElement Statement = Doc.CreateElement(SubNode);
            Statement.SetAttribute(_subtitle, _statement);
            Node.AppendChild(Statement);
            Doc.Save(File + ".xml");
        }
        /// <summary>
        /// 清空節點內所有資料。EX.  ClearNode("test/abc");
        /// </summary>
        /// <param name="_file"></param>  開頭都不能為數字
        public static void ClearNode(string _node)
        {
            XmlDocument Doc = new XmlDocument();
            string File = _node.Split('/')[0];
            Doc.Load(File + ".xml");
            XmlNode Node = Doc.SelectSingleNode(_node);
            if (Node == null)
                return;

            XmlElement Delete = (XmlElement)Node;
            Delete.RemoveAll();
            Doc.Save(File + ".xml");
        }
        /// <summary>
        /// 複寫節點內所帶資料。EX.  ReWrite("test/dog/taiwandog","color","white");
        /// </summary>
        /// <param name="_node"></param>
        /// <param name="_target"></param>
        /// <param name="_statement"></param>
        public static void ReWrite(string _node, string _target, string _statement)
        {
            XmlDocument Doc = new XmlDocument();
            string File = _node.Split('/')[0];
            Doc.Load(File + ".xml");
            XmlNode Node = Doc.SelectSingleNode(_node);
            if (Node == null)
                return;

            XmlElement Element = (XmlElement)Node;
            string Data = Element.GetAttribute(_target);
            XmlAttribute attribute = Element.GetAttributeNode(_target);
            XmlAttributeCollection Attributes = Element.Attributes;

            string Space = "";

            foreach (XmlAttribute item in Attributes)
            {
                Space += item.Name + "," + item.Value + Environment.NewLine;
                if (item.Name == _target)
                    item.Value = _statement;
            }

            Doc.Save(File + ".xml");
        }
        /// <summary>
        /// 讀取節點內所帶資料，若節點內無此項資料，或無此節點則傳回預設值。EX.  Read("test/dog/taiwandog","color","pink");
        /// </summary>
        /// <param name="_node"></param>
        /// <param name="_subtitle"></param>
        /// <param name="_defaultValue"></param>
        /// <returns></returns>
        public static string Read(string _node, string _subtitle, string _defaultValue)
        {
            //_node內須附加XML檔名
            try
            {
                string File = _node.Split('/')[0];
                XmlDocument Doc = new XmlDocument();
                Doc.Load(File + ".xml");
                string NodeTemp = _node;
                XmlNode Node = Doc.SelectSingleNode(NodeTemp);
                if (Node == null)
                    throw new Exception();

                XmlElement Element = (XmlElement)Node;
                string Data = Element.GetAttribute(_subtitle);
                return Data;
            }
            catch
            {
                return _defaultValue;
            }
        }

        //===================================自訂=============================================
        public static string Read(string _node, string _defaultValue)
        {
            try
            {
                string File = _node.Split('/')[0];
                XmlDocument Doc = new XmlDocument();
                Doc.Load(File + ".xml");
                string NodeTemp = _node;
                XmlNode Node = Doc.SelectSingleNode(NodeTemp);
                if (Node == null)
                    throw new Exception();

                XmlElement Element = (XmlElement)Node;
                string Data = Element.GetAttribute("value");
                return Data;
            }
            catch
            {
                return _defaultValue;
            }
        }

        public static void CreateNode(string _node, string _subnode, string _target, string _default_value)
        {
            XmlDocument Doc = new XmlDocument();
            string File = _node.Split('/')[0];
            Doc.Load(File + ".xml");
            XmlNode Node = Doc.SelectSingleNode(_node);
            if (Node == null)
                return;
          
            XmlElement Node1 = Doc.CreateElement(_subnode);
            Node1.SetAttribute(_target, _default_value);
            Node.AppendChild(Node1);
            Doc.Save(File + ".xml");
        }
    }
}
