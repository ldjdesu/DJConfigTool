namespace ConfigTool
{
    class CSharpModel:Model
    {
        string newLine = System.Environment.NewLine;
        public string GetArray(string type, string fieldName)
        {
            return "\tpublic " + type + "[] " + fieldName + ";" + newLine;
        }
        public string GetType(string type, string fieldName)
        {
            return "\tpublic " + type + " " + fieldName + ";" + newLine;
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
        public string GetStructHead()
        {
            return "namespace ConfigStruct" + newLine +
                    "{" + newLine;
        }
        public string GetStruct(string fieldName)
        {
            return "\tpublic struct S_" + fieldName + newLine + "\t{" + newLine;
        }
        public string GetStructType(string type, string fieldName)
        {
            return "\t\tpublic " + type + " " + fieldName + ";" + newLine;
        }
        public string GetStructEnd(string fieldName)
        {
            return "\t}" + newLine;
        }
        public string GetStructField(string fieldName, bool isArray)
        {
            string temp = isArray ? "[] " : "";
            return "\tpublic ConfigStruct.S_" + fieldName + temp + " " + fieldName + ";" + newLine;
        }
        public string GetEnd()
        {
            return "}";
        }


    }
}
