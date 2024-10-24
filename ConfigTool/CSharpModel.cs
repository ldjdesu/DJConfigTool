using System.Linq;

namespace ConfigTool
{
    class CSharpModel:Model
    {
        string newLine = System.Environment.NewLine;
        /*public string GetArray(string type, string fieldName)
        {
            return "\tpublic " + type + "[] " + fieldName + ";" + newLine;
        }*/
        public string GetType(string type, string fieldName,bool isArray,bool isDefine=false)
        {
            string temp1 = "";
            if (isDefine)
            {
                temp1 += "ConfigDefine.";
            }
            string temp2 = " ";
            if (isArray)
            {
                temp2 = "[] ";
            }
               
            return "\tpublic "+ temp1 + type + temp2 + fieldName + ";" + newLine; ;
        }
        public string GetStart()
        {
            return "//该脚本为打表工具自动生成，切勿修改！" + newLine +
                    "using UnityEngine;" + newLine +
                    "using System;" + newLine;
        }
        public string GetClassHead(string className)
        {
            return "public struct " + className + "DataConfig:IDataConfigLine" + newLine +
                    "{" + newLine;
        }
        public string GetDefineHead()
        {
            return "namespace ConfigDefine" + newLine +
                    "{" + newLine;
        }
        public string GetStruct(string type)
        {
            return "\tpublic struct " + type + newLine + "\t{" + newLine;
        }
        public string GetStructType(string type, string fieldName,bool isDefine=false)
        {
            string temp1 = "";
            if (isDefine)
            {
                temp1 += "ConfigDefine.";
            }

            return "\t\tpublic " + temp1 + type + " " + fieldName + ";" + newLine;
        }
        public string GetStructEnd()
        {
            return "\t}" + newLine;
        }
        public string GetStructField(string type, string fieldName, bool isArray)
        {
            string temp = isArray ? "[] " : "";
            return "\tpublic ConfigDefine." + type + temp + " " + fieldName + ";" + newLine;
        }
        public string GetEnum(string fieldName)
        {
            return "\tpublic enum " + fieldName + newLine + "\t{" + newLine;
        }
        public string GetEnumType(string fieldName,int index)
        {
            return "\t\t " + fieldName + " = "+index.ToString()+"," + newLine;
        }
        public string GetEnumEnd()
        {
            return "\t}" + newLine;
        }
        public string GetEnd()
        {
            return "}";
        }


    }
}
