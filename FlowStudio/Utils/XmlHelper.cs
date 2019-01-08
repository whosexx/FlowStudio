using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace FlowStudio
{
    public class XmlHelper
    {
        public static void Save(object obj, Type type, string path)
        {
            XmlSerializer xml = new XmlSerializer(type);
            XmlSerializerNamespaces name = new XmlSerializerNamespaces();
            name.Add("", "");
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = Encoding.UTF8;
            settings.Indent = true;

            using (XmlWriter writer = XmlWriter.Create(path, settings))
            {
                xml.Serialize(writer, obj, name);
                writer.Flush();
                return;
            }
        }

        public static object Read(Type type, string filepath)
        {
            XmlSerializer xml = new XmlSerializer(type);
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Prohibit;

            try
            {
                using (XmlReader reader = XmlReader.Create(filepath, settings))
                {
                    object obj = null;
                    obj = xml.Deserialize(reader);
                    return obj;
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }
        }

        public static void Serialize(object obj, Type type, StringBuilder sb)
        {
            XmlSerializer xml = new XmlSerializer(type);
            XmlSerializerNamespaces name = new XmlSerializerNamespaces();
            name.Add("", "");
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = Encoding.UTF8;
            settings.Indent = true;

            using (XmlWriter writer = XmlWriter.Create(sb, settings))
            {
                xml.Serialize(writer, obj, name);
                writer.Flush();
            }
        }

        public static object Deserialize(Type type, string xmlcontent)
        {
            XmlSerializer xml = new XmlSerializer(type);

            try
            {
                MemoryStream ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(xmlcontent));
                using (StreamReader reader = new StreamReader(ms, Encoding.UTF8))
                {
                    object obj = null;
                    obj = xml.Deserialize(reader);
                    return obj;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }
        }

        public static void BinarySerialize(object obj, string path)
        {
            FileInfo info = new FileInfo(path + ".tmp");
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter formater = new BinaryFormatter();
                //执行序列化
                formater.Serialize(ms, obj);
                File.WriteAllBytes(info.FullName, ms.ToArray());
            }

            if (File.Exists(path))
                File.Delete(path);
            info.MoveTo(path);
        }

        //封装二进制反序列化方法
        public static object BinaryDeserialize(string filepath)
        {
            try
            {
                var bytes = File.ReadAllBytes(filepath);
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    BinaryFormatter formater = new BinaryFormatter();
                    object user = formater.Deserialize(ms);
                    return user;
                }
            }
            catch
            {
                return null;
            }
        }

        public static void BinarySerialize(object obj, MemoryStream ms)
        {
            BinaryFormatter formater = new BinaryFormatter();
            //执行序列化
            formater.Serialize(ms, obj);
        }

        //封装二进制反序列化方法
        public static object BinaryDeserialize(Stream ms)
        {
            try
            {
                BinaryFormatter formater = new BinaryFormatter();
                object user = formater.Deserialize(ms);
                return user;
            }
            catch
            {
                return null;
            }
        }
    }
}
