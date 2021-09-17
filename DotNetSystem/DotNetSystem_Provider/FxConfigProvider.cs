using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Extensions.Configuration;

namespace DotNetSystem_JsonProvider {
    class FxConfigProvider : FileConfigurationProvider {
        //进行一个转换,将src类型转换成基类FileConfigurationSource，这样才能够满足FileConfigurationProvider的构造函数
        public FxConfigProvider(FxConfigSource src):base(src) {

        }

        //此处stream是要读取的web.config的文件流
        public override void Load(Stream stream) {
            //此处是字典忽略大小写
            var data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            XmlDocument xml = new XmlDocument();
            xml.Load(stream);
            var cdNodes = xml.SelectNodes("/configuration/connectionStrings/add");
            //Cast類型强轉
            foreach (XmlNode item in cdNodes.Cast<XmlNode>()) {
                string name = item.Attributes["name"].Value;
                string connectString = item.Attributes["connectString"].Value;
                //扁平化的根節點為connectStrings
                //[name1:{connectString:"xxx",providerName:"xxx"},name2:{connectString:"xxx",providerName:"xxx"}]
                //以{name}:connectString和{name}:providerName为键
                data[$"{name}:connectString"] = connectString;
                var providerName = item.Attributes["providerName"];
                if (providerName != null) {
                    data[$"{name}:providerName"] = providerName.Value;
                }
            }

            var appSettingsNodes = xml.SelectNodes("/configuration/appSettings/add");
            foreach (XmlNode item in appSettingsNodes.Cast<XmlNode>()) {
                string key = item.Attributes["key"].Value;
                key = key.Replace(".", ":");
                string value = item.Attributes["value"].Value;
                data[key] = value;
            }

            this.Data = data;
        }
    }
}
