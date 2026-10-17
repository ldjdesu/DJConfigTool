using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ConfigTool
{
    class CSharpConfigSet:ConfigSet
    {
        string newLine = System.Environment.NewLine;
        /// <summary>生成跨列定长数组的逐行分配与解析，不再支持分号数组。</summary>
        public string GetArray(string configName, string typeName, string fieldName, int index, ref int num, int size, bool isDefine = false)
        {
            index++;
            string elementType = (isDefine ? "ConfigDefine." : "") + typeName;
            string parse = GetTypeToString(typeName, "data[i][temp" + (num + 1) + "]") + ";";
            string temp = "\t\tvar temp" + num + " = new " + elementType + "[" + size + "];" + newLine +
                "\t\tfor (int temp" + (num + 1) + " = " + index + "; temp" + (num + 1) + " < " + index + " + " + size + "; temp" + (num + 1) + "++)" + newLine +
                "\t\t{" + newLine +
                "\t\t\ttemp" + num + "[temp" + (num + 1) + " - " + index + "] = " + parse + newLine +
                "\t\t}" + newLine +
                "\t\tcache." + fieldName + " = temp" + num + ";" + newLine;
            num += 2;
            return temp;
        }
        public string GetType(string fieldName,string typeName, int index, bool isDefine = false)
        {
            string temp = "\t\tcache." + fieldName + " = ";
            temp += GetTypeToString(typeName, "data[i][" + index + "]") + ";" + newLine;
            return temp;
        }
        /// <summary>生成索引写入，重复唯一键报告表、字段、行及键值。</summary>
        public string GetDicAdd(string fildName, bool isList, int index, ref int num)
        {
            string temp = "";
            if (isList)
            {
                temp += "\t\t\tList<int> temp" + num+";" + newLine +
                        "\t\t\tif (" + fildName + "ListDic.TryGetValue(data[i][" + index + "], out temp" + num + "))" + newLine +
                        "\t\t\t{" + newLine +
                        "\t\t\t\ttemp" + num + ".Add(i);" + newLine +
                        "\t\t\t}" + newLine +
                        "\t\t\telse" + newLine +
                        "\t\t\t{" + newLine +
                        "\t\t\t\ttemp" + num + " = new List<int>(5);" + newLine +
                        "\t\t\t\t" + fildName + "ListDic.Add(data[i][" + index + "], temp" + num + ");" + newLine +
                        "\t\t\t\ttemp" + num + ".Add(i);" + newLine +
                        "\t\t\t}" + newLine;
                num++;
            }
            else
            {
                temp += "\t\t\tif (" + fildName + "Dic.ContainsKey(data[i][" + index + "])) throw new ArgumentException(GetType().Name + \" 行\" + (i + 1) + \" 字段 " + fildName + " 重复键：\" + data[i][" + index + "]);" + newLine;
                temp += "\t\t\t" + fildName + "Dic.Add(data[i][" + index + "], i);" + newLine;
            }


            return temp;
        }
        /// <summary>生成表容器和跨 ConfigSet 值类型副本共享的逐行解析缓存。</summary>
        public string GetStart(string className, bool hasUnique, bool hasNoUnique)
        {
            string temp = "//该脚本为打表工具自动生成，切勿修改！" + newLine +
                "using UnityEngine;" + newLine + "using System;" + newLine +
                "using System.Collections.Generic;" + newLine +
                "public struct " + className + "_ConfigSet:IDataConfig" + newLine + "{" + newLine +
                "\tList<string[]> data;" + newLine +
                "\t" + className + "DataConfig[] parsedRows;" + newLine +
                "\tbool[] parsedFlags;" + newLine;
            if (hasUnique)
                temp += "\tDictionary<string, Dictionary<string, int>> configDic;" + newLine;
            if (hasNoUnique)
                temp += "\tDictionary<string, Dictionary<string, List<int>>> configListDic;" + newLine;
            return temp;
        }
        public string GetStruct(string configName, string typeName,string fieldName)
        {
            return "\t\tcache." + fieldName + " = new ConfigDefine." + typeName + "" + newLine +
                   "\t\t{" + newLine;
        }
        public string GetStructTypeLoop(string typeName, string fieldName)
        {
            return "\t\t\t" + fieldName + "= new ConfigDefine." + typeName + "{" + newLine;
        }
        public string GetStructType(string typeName, string fieldName,int index)
        {
            return "\t\t\t" + fieldName + "=" + GetTypeToString(typeName, "data[i][" + index + "]") + "," + newLine;
        }
        public string GetStructEnd(bool isLoop=false)
        {
            string temp = ";";
            if (isLoop)
            {
                temp = ",";
            }
            return "\t\t}"+ temp + newLine;
        }
        /// <summary>生成结构体数组的首次查询分配与解析。</summary>
        public string GetStructArray(string configName, string typeName,int index,ref int num, int arraySize, int structSize)
        {
            index += 1;
            string temp = "\t\tvar temp" + num + " = new ConfigDefine." + typeName + "[" + arraySize + "];" + newLine +
                           "\t\tfor (int temp"+ (num+1)+" = "+ index+"; temp" + (num+1)+" < "+ index+" + "+ arraySize+" * "+ structSize+"; temp" + (num+1)+" += "+ structSize+")" + newLine +
                           "\t\t{" + newLine +
                           "\t\t\ttemp"+ num+"[(temp"+ (num+1)+" - "+ index+") / "+ structSize+ "] = new ConfigDefine." + typeName + "{" + newLine;
            num += 2;
            return temp;
        }
        public string GetStructArrayType(string typeName, string fieldName, int num,int offset)
        {
            return "\t\t\t\t" + fieldName + "=" + GetTypeToString(typeName, "data[i][temp" + (num-1) + "+"+ offset+"]") + "," + newLine;
        }
        public string GetStructArrayEnd(string configName,string fieldName, int num)
        {
            return "\t\t\t};" + newLine +
                   "\t\t}" + newLine +
                   "\t\tcache." + fieldName + " = temp"+ (num-2)+";" + newLine;
        }
        /// <summary>按表的索引种类生成查询；两种索引共存时支持跨种类查询。</summary>
        public string GetKeyStrFunction(string configName, bool isUnique, bool hasUnique, bool hasNoUnique)
        {
            if ((isUnique && !hasUnique) || (!isUnique && !hasNoUnique)) return "";
            string dataType = configName + "DataConfig";
            var output = new StringBuilder();
            output.AppendLine("\t/// <summary>" + (isUnique
                ? "按索引取首条配置；无命中返回默认值。数组为表缓存，请勿修改。"
                : "按表格顺序取全部命中配置；无命中返回空列表。数组为表缓存，请勿修改。") + "</summary>");
            output.AppendLine("\tpublic " + (isUnique ? dataType : "List<" + dataType + ">") +
                " " + (isUnique ? "GetConfigByKey" : "GetConfigsByKey") + "(string keyName, string value)");
            output.AppendLine("\t{");
            if (isUnique)
            {
                output.AppendLine("\t\tif (keyName == null || value == null) return default(" + dataType + ");");
            }
            else
            {
                output.AppendLine("\t\tvar result = new List<" + dataType + ">();");
                output.AppendLine("\t\tif (keyName == null || value == null) return result;");
            }
            if (hasUnique)
            {
                output.AppendLine("\t\tif (configDic.TryGetValue(keyName, out var indices) && indices.TryGetValue(value, out int index))");
                output.AppendLine("\t\t{");
                output.AppendLine(isUnique ? "\t\t\treturn DeserializeByIndex(index);" : "\t\t\tresult.Add(DeserializeByIndex(index));");
                if (!isUnique) output.AppendLine("\t\t\treturn result;");
                output.AppendLine("\t\t}");
            }
            if (hasNoUnique)
            {
                output.AppendLine("\t\tif (configListDic.TryGetValue(keyName, out var groups) && groups.TryGetValue(value, out var rows))");
                output.AppendLine("\t\t{");
                if (isUnique)
                    output.AppendLine("\t\t\treturn DeserializeByIndex(rows[0]);");
                else
                {
                    output.AppendLine("\t\t\tresult.Capacity = rows.Count;");
                    output.AppendLine("\t\t\tfor (int i = 0; i < rows.Count; i++) result.Add(DeserializeByIndex(rows[i]));");
                }
                output.AppendLine("\t\t}");
            }
            output.AppendLine("\t\treturn " + (isUnique ? "default(" + dataType + ")" : "result") + ";");
            output.AppendLine("\t}");
            return output.ToString();
        }
        public string GetDic(string fildName, bool isList)
        {
            if (isList)
            {
                return "\t\tDictionary<string,List<int>> " + fildName + "ListDic = new Dictionary<string,List<int>>();" + newLine;
            }
            else
            {
                return "\t\tDictionary<string,int> " + fildName + "Dic = new Dictionary<string,int>();" + newLine;
            }
        }
        public string GetDicEnd(string fildName, bool isList)
        {
            string temp = isList ? "List" : "";
            return "\t\tconfig"+temp+"Dic.Add(\""+fildName+"\","+ fildName+ temp+"Dic);" +newLine;
        }
        /// <summary>重建索引与解析缓存；再次 Init 不复用旧行结果。</summary>
        public string GetInit(string configName, bool hasUnique, bool hasNoUnique, string strDic)
        {
            string temp = "\t/// <summary>初始化只读配置和索引；重新初始化会替换本容器的行缓存。</summary>" + newLine +
                "\tpublic void Init(List<string[]> data)" + newLine + "\t{" + newLine +
                "\t\tif (data == null) throw new ArgumentNullException(nameof(data));" + newLine +
                "\t\tthis.data = data;" + newLine +
                "\t\tparsedRows = new " + configName + "DataConfig[data.Count];" + newLine +
                "\t\tparsedFlags = new bool[data.Count];" + newLine;
            if (hasUnique)
                temp += "\t\tconfigDic = new Dictionary<string, Dictionary<string, int>>();" + newLine;
            if (hasNoUnique)
                temp += "\t\tconfigListDic = new Dictionary<string, Dictionary<string, List<int>>>();" + newLine;
            temp += strDic;
            temp += "\t\tfor (int i = 0; i < data.Count; i++)" + newLine + "\t\t{" + newLine;
            return temp;
        }
        /// <summary>每行只解析一次，缓存数组由 ConfigSet 的值类型副本共享。</summary>
        public string GetDeserialize(string configName)
        {
            return "\t/// <summary>首次访问解析该行，后续访问复用解析结果。</summary>" + newLine +
                "\tprivate " + configName + "DataConfig DeserializeByIndex(int i)" + newLine + "\t{" + newLine +
                "\t\tif (parsedFlags[i]) return parsedRows[i];" + newLine +
                "\t\tvar cache = new " + configName + "DataConfig();" + newLine;
        }
        /// <summary>完整解析成功后才标记缓存有效。</summary>
        public string GetDeserializeEnd()
        {
            return "\t\tparsedRows[i] = cache;" + newLine + "\t\tparsedFlags[i] = true;" + newLine +
                "\t\treturn cache;" + newLine + "\t}" + newLine;
        }
        public string GetEnd()
        {
            return "}"+newLine;
        }

        private string GetTypeToString(string typeName,string value)
        {
            string temp;
            if (typeName=="int")
            {
                typeName = "Int32";
            }
            if (typeName == "string"|| typeName == "String")
            {
                temp = value ;
            }
            else if (typeName == "UInt32" || typeName == "Int32" || typeName == "UInt64" || typeName == "Int64"
                || typeName == "Boolean" || typeName == "float" || typeName == "double" )
            {

                temp = value+".Parse" + typeName+"()";
            }
            else
            {
                
                temp = "(ConfigDefine." + typeName + ")"+ value + ".ParseUInt32()";
            }
            return temp;
        }
        /// <summary>生成返回独立列表的全表查询，列表内数组仍引用只读行缓存。</summary>
        public string GetStrAllConfig(string configName)
        {
            string temp="";
            temp += "\t/// <summary>返回全表的独立列表；配置中的数组为只读共享缓存。</summary>" + newLine;
            temp += "\tpublic List<" + configName + "DataConfig> GetAllConfig()" + newLine +
                "\t{" + newLine +
                "\t\tList<" + configName + "DataConfig> temp = new List<" + configName + "DataConfig>(data.Count);" + newLine +
                "\t\tfor (int i = 0; i < data.Count; i++)" + newLine +
                "\t\t{" + newLine +
                "\t\t\ttemp.Add(DeserializeByIndex(i));" + newLine +
                "\t\t}" + newLine +
                "\t\treturn temp;" + newLine +
                "\t}" + newLine;
            return temp;
        }
    }
}
