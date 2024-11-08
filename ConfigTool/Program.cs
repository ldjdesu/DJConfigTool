using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using static System.Net.Mime.MediaTypeNames;

namespace ConfigTool
{
    class Program
    {
        enum indexType
        {
            UnIndex,//不索引
            NoUnique,//多值
            Unique,//唯一值
        }
        class CodeTransform
        {
            public string name;
            public string code;
            public List<CodeTransformItem> items;

            public CodeTransform(string name, string code, List<CodeTransformItem> items)
            {
                this.name = name;
                this.code = code;
                this.items = items;
            }
        }
        class CodeTransformItem
        {
            public string name;
            public string code;
            public string value;

            public CodeTransformItem(string name, string code, string value)
            {
                this.name = name;
                this.code = code;
                this.value = value;
            }
        }

        static void Main(string[] args)
        {
            /*args = new string[1];
            args[0] = "";*/
            Model model=new CSharpModel();
            ConfigSet configSet = new CSharpConfigSet();
            Console.WriteLine("Hello World!");
            
            string csvPath = args[0]+ @".\Data\CsvOld";
            string csvNewPath = args[0] + @".\Data\CsvNew";
            string cSharpPath = args[0]+ @".\Data\CSharp";
            string md5Path = args[0]+ @".\Data\MD5";
            string ConfigDefinePath = args[0]+ @".\ConfigDefine.xml";
            try
            {
                XmlDocument definedoc = new XmlDocument();
                definedoc.Load(ConfigDefinePath);
                Dictionary<string, CodeTransform> enumDic = new Dictionary<string, CodeTransform>();
                XmlNodeList enumNodeList = definedoc.SelectNodes("/definitions/enumDefine/enum");
                for (int i = 0; i < enumNodeList.Count; i++)
                {
                    string enumStr = enumNodeList[i].Attributes["name"].Value;
                    string enumCode = enumNodeList[i].Attributes["code"].Value;
                    XmlNodeList fieldList = enumNodeList[i].ChildNodes;
                    List<CodeTransformItem> items = new List<CodeTransformItem>(fieldList.Count);
                    for (int j = 0; j < fieldList.Count; j++)
                    {
                        var field = fieldList[j];
                        items.Add(new CodeTransformItem(field.Attributes["name"].Value, field.Attributes["code"].Value, field.Attributes["index"].Value));
                    }
                    enumDic.Add(enumStr, new CodeTransform(enumStr, enumCode, items));
                }
                Dictionary<string, CodeTransform> structDic = new Dictionary<string, CodeTransform>();
                XmlNodeList structNodeList = definedoc.SelectNodes("/definitions/structDefine/struct");
                for (int i = 0; i < structNodeList.Count; i++)
                {
                    string structStr = structNodeList[i].Attributes["name"].Value;
                    string structCode= structNodeList[i].Attributes["code"].Value;
                    XmlNodeList fieldList = structNodeList[i].ChildNodes;
                    List<CodeTransformItem> items = new List<CodeTransformItem>(fieldList.Count);
                    for (int j = 0; j < fieldList.Count; j++)
                    {
                        var field = fieldList[j];
                        string name = field.Attributes["name"].Value;
                        string fieldName = field.Attributes["fieldName"].Value;
                        string notes = field.Attributes["notes"]?.Value;
                        items.Add(new CodeTransformItem(name, fieldName, notes));
                    }
                    structDic.Add(structStr, new CodeTransform(structStr, structCode, items));
                }



                var files = Directory.EnumerateFiles(csvPath, "*.csv");//得到所有csv文件
                List<string> csvMD5;
                Dictionary<string, string> configName_MD5 = new Dictionary<string, string>();
                try
                {
                    csvMD5 = new List<string>(File.ReadAllLines(md5Path + "\\CsvMD5.txt"));
                    foreach (var item in csvMD5)
                    {
                        string[] str = item.Split(",");
                        configName_MD5.Add(str[0], str[1]);
                    }
                }
                catch (Exception)
                {
                    csvMD5 = new List<string>();
                    Console.WriteLine("无MD5文件，将重新创建");
                }
                string defineStr=model.GetStart()+model.GetDefineHead();

                HashSet<string> defineKeys=new HashSet<string>();
                foreach (string filePath in files)
                {
                    Console.WriteLine(filePath);
                    string[] tempFilePath = filePath.Split('\\');
                    string configName = tempFilePath[tempFilePath.Length - 1].Replace(".csv","");
                    string str = File.ReadAllText(filePath);

                    string[] text = File.ReadAllLines(filePath);
                    //======================构建model类 Start
                    string outText = GenerateModel(model, configName, text, filePath, enumDic, structDic);

                    MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                    Byte[] newMD5Bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
                    var sBuilder = new StringBuilder();
                    for (int i = 0; i < newMD5Bytes.Length; i++)
                    {
                        sBuilder.Append(newMD5Bytes[i].ToString("x2"));
                    }
                    string newMD5Str = sBuilder.ToString();
                    //string newMD5Str = Encoding.UTF8.GetString(newMD5Bytes);
                    string oldMD5;
                    if (configName_MD5.TryGetValue(configName,out oldMD5))
                    {
                        if (newMD5Str== oldMD5)//对比md5
                        {
                            continue;
                        }
                        else
                        {
                            configName_MD5[configName] = newMD5Str;
                        }
                    }
                    else//添加md5
                    {
                        configName_MD5.Add(configName, newMD5Str);
                    }
                    string outPutPath = cSharpPath+"\\" + configName + "DataConfig.cs";
                    File.WriteAllText(outPutPath, outText, Encoding.UTF8);
                    //======================构建model类 End

                    //======================构建set类 Start
                    outText = GenerateSet(configSet, configName, text, filePath, enumDic, structDic);
                    outPutPath = cSharpPath + "\\" + configName + "_ConfigSet.cs";
                    File.WriteAllText(outPutPath, outText, Encoding.UTF8);
                    //======================构建set类 End

                    //======================替换csv文件 Start
                    outText = GenerateCsv(configName, text, filePath, enumDic, structDic);
                    outPutPath = csvNewPath + "\\" + configName + ".csv";
                    File.WriteAllText(outPutPath, outText, Encoding.UTF8);
                    //======================替换csv文件 End
                }
                //======================构建Define类 Start
                defineStr = model.GetStart() + model.GetDefineHead();
                for (int i = 0; i < enumNodeList.Count; i++)
                {
                    defineStr += model.GetEnum(enumNodeList[i].Attributes["code"].Value, enumNodeList[i].Attributes["name"].Value);
                    XmlNodeList fieldList = enumNodeList[i].ChildNodes;
                    List<CodeTransformItem> items = new List<CodeTransformItem>(fieldList.Count);
                    for (int j = 0; j < fieldList.Count; j++)
                    {
                        var field = fieldList[j];
                        int index = int.Parse(field.Attributes["index"].Value);
                        defineStr += model.GetEnumType(field.Attributes["code"].Value, index, field.Attributes["name"].Value);
                    }
                    defineStr += model.GetEnumEnd();
                }
                CodeTransform tempCodeTransform;
                for (int i = 0; i < structNodeList.Count; i++)
                {
                    defineStr += model.GetStruct(structNodeList[i].Attributes["code"].Value, structNodeList[i].Attributes["name"].Value);
                    XmlNodeList fieldList = structNodeList[i].ChildNodes;
                    List<CodeTransformItem> items = new List<CodeTransformItem>(fieldList.Count);
                    for (int j = 0; j < fieldList.Count; j++)
                    {
                        var field = fieldList[j];
                        string realTypeStr = field.Attributes["name"].Value;
                        string notesStr = "";
                        if (field.Attributes["notes"] != null)
                        {
                            notesStr = field.Attributes["notes"].Value;
                        }
                        if (enumDic.TryGetValue(field.Attributes["name"].Value, out tempCodeTransform))
                        {
                            realTypeStr = tempCodeTransform.code;
                            notesStr= tempCodeTransform.name;
                        }
                        defineStr += model.GetStructType(realTypeStr, field.Attributes["fieldName"].Value, false, notesStr);
                    }
                    defineStr += model.GetStructEnd();
                }
                defineStr += model.GetEnd();
                File.WriteAllText(cSharpPath + "\\ConfigDefine.cs", defineStr, Encoding.UTF8);
                //======================构建Define类 End
                string md5Str = "";
                foreach (var item in configName_MD5)
                {
                    md5Str+=item.Key+","+item.Value + Environment.NewLine;
                }
                File.WriteAllText(md5Path + "\\CsvMD5.txt", md5Str, Encoding.Default);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("有错误，点击任意键退出");
                Console.ReadKey();
            }

            Console.WriteLine("构建完成");
        }

        static private string GenerateModel(Model model, string configName, string[] text, string filePath, Dictionary<string, CodeTransform> enumDic, Dictionary<string, CodeTransform> structDic)
        {
            //构建开头
            string outPut = model.GetStart();
            outPut += model.GetClassHead(configName);
            string[] notesStr = text[0].Split(',');
            string[] typeStr = text[1].Split(',');
            string[] fildStr = text[2].Split(',');
            int flag = 0;//标志位(1为结构体数组模式)
            int offset = 0;//长度位
            CodeTransform tempCodeTransform;
            //构建字段
            for (int i = 0; i < typeStr.Length; i++)
            {
                if (typeStr[i].Contains("note"))
                {
                    continue;
                }
                else if (enumDic.TryGetValue(typeStr[i], out tempCodeTransform))
                {
                    string type = tempCodeTransform.code;
                    outPut += model.GetType(type, fildStr[i],false,true, notesStr[i]);
                }
                else if (structDic.TryGetValue(typeStr[i], out tempCodeTransform))
                {
                    int size = tempCodeTransform.items.Count;
                    string structName = tempCodeTransform.code;
                    string fildName = fildStr[i];
                    bool isArray = false;
                    if (flag == 1)
                    {
                        isArray = true;
                        outPut += model.GetStructField(structName, fildName, isArray, notesStr[i-1]);
                        i += offset * size;
                        flag = 0;
                    }
                    else
                    {
                        outPut += model.GetStructField(structName, fildName, isArray, notesStr[i]);
                        i += size;
                    }
                }
                else if (typeStr[i].Contains("_"))
                {
                    string[] typeDivision = typeStr[i].Split('_');
                    if (typeDivision[0] == "Array")
                    {
                        string suffix = typeDivision[1];
                        if (suffix == "Struct")//结构体数组模式
                        {
                            flag = 1;
                            offset = int.Parse(fildStr[i]);
                            //structName = fildStr[i];
                        }
                        else if (Regex.IsMatch(suffix, @"^\d+$"))//数组模式1
                        {
                            int size = int.Parse(suffix);
                            string realTypeStr= typeStr[i + 1];
                            bool isDefine = false;
                            if (enumDic.TryGetValue(realTypeStr, out tempCodeTransform))
                            {
                                realTypeStr= tempCodeTransform.code;
                                isDefine = true;
                            }
                            outPut += model.GetType(realTypeStr, fildStr[i + 1],true, isDefine, notesStr[i]);
                            i += size;
                        }
                        else//数组模式2
                        {
                            outPut += model.GetType(suffix, fildStr[i],true,false, notesStr[i]);
                        }
                    }
                    else
                    {
                        Console.WriteLine("未能识别的类型前缀,请检查!  路径为：" + filePath + "  第" + i + "列");
                        throw new Exception();
                    }
                }
                else
                {
                    outPut += model.GetType(typeStr[i], fildStr[i],false,false, notesStr[i]);
                }
            }
            outPut += model.GetEnd();
            return outPut;
        }

        static private string GenerateSet(ConfigSet configSet, string configName, string[] text, string filePath, Dictionary<string, CodeTransform> enumDic, Dictionary<string, CodeTransform> structDic)
        {
            string[] typeStrTemp = text[1].Split(',');//0行是注释，跳过
            string[] fildStrTemp = text[2].Split(',');
            string[] indexStrTemp = text[3].Split(',');
            List<string> typeStr = new List<string>();
            List<string> fildStr = new List<string>();
            List<string> indexStr = new List<string>();
            HashSet<int> noteSet = new HashSet<int>();//注释列;
            for (int i = 0; i < typeStrTemp.Length; i++)
            {
                if (typeStrTemp[i].Contains("note"))
                {
                    noteSet.Add(i);
                }
                else
                {
                    typeStr.Add(typeStrTemp[i]);
                    fildStr.Add(fildStrTemp[i]);
                    indexStr.Add(indexStrTemp[i]);
                }
            }
            bool hasNoUnique = false;
            for (int i = 0; i < indexStr.Count; i++)
            {
                if (indexStr[i].Contains("NoUnique"))
                {
                    hasNoUnique = true;
                    break;
                }
            }
            //构建开头
            string outPut = configSet.GetStart(configName, hasNoUnique);
            //构建字段
            string strDic = "";//索引的开始
            string strDicEnd = "";//索引的结尾
            indexType[] indexTypes = new indexType[typeStr.Count];//索引类型标记（0为不索引，1为Unique，2为NoUnique）
            for (int i = 0; i < indexStr.Count; i++)
            {
                if (indexStr[i].Length != 0)
                {
                    string[] indexDivision = indexStr[i].Split('_');
                    if (indexDivision[0] == "Index")
                    {
                        if (indexDivision[1] == "Unique")
                        {
                            strDic += configSet.GetDic(fildStr[i], false);
                            strDicEnd += configSet.GetDicEnd(fildStr[i], false);
                            indexTypes[i] = indexType.Unique;
                        }
                        else if (indexDivision[1] == "NoUnique")
                        {
                            strDic += configSet.GetDic(fildStr[i], true);
                            strDicEnd += configSet.GetDicEnd(fildStr[i], true);
                            indexTypes[i] = indexType.NoUnique;
                        }
                        else
                        {
                            Console.WriteLine("未能识别的索引后缀,请检查!  路径为：" + filePath + "  第" + i + "列");
                            throw new Exception();
                        }
                    }
                    else
                    {
                        Console.WriteLine("未能识别的索引前缀,请检查!  路径为：" + filePath + "  第" + i + "列");
                        throw new Exception();
                    }
                }
                else
                {
                    indexTypes[i] = indexType.UnIndex;
                    continue;
                }
            }
            outPut += configSet.GetKeyStrFunction(configName, true, hasNoUnique);
            outPut += configSet.GetKeyStrFunction(configName, false, hasNoUnique);

            int flag = 0;//标志位(1为结构体数组模式)
            int offset = 0;//长度位
            string structName = "";//缓存结构体字段名
            int num = 1;//临时变量名序号
            string arrayCache = "";
            string arrayCacheNew = "";
            outPut += configSet.GetDeserialize(configName);
            CodeTransform tempCodeTransform;
            CodeTransform tempCodeTransform2;
            for (int i = 0; i < typeStr.Count; i++)
            {
                if (enumDic.TryGetValue(typeStr[i], out tempCodeTransform))
                {
                    string type = tempCodeTransform.code;
                    outPut += configSet.GetType(fildStr[i], type, i, true);
                }
                else if (structDic.TryGetValue(typeStr[i], out tempCodeTransform))
                {
                    bool isArray = flag == 1;
                    int size = tempCodeTransform.items.Count;
                    structName = tempCodeTransform.code;
                    string fildName = fildStr[i];
                    if (isArray)
                    {
                        arrayCache += configSet.GetArrayCache("ConfigDefine." + structName, num);
                        arrayCacheNew += configSet.GetArrayCacheNew("ConfigDefine." + structName, num, offset);
                        outPut += configSet.GetStructArray(configName, structName, i, ref num, offset, size);
                        for (int j = 0; j < size; j++, i++)
                        {
                            string realTypeStr = tempCodeTransform.items[j].name;
                            bool isDefine = false;

                            if (enumDic.TryGetValue(realTypeStr, out tempCodeTransform2))
                            {
                                realTypeStr = tempCodeTransform2.code;
                                isDefine = true;
                            }
                            outPut += configSet.GetStructArrayType(realTypeStr, fildStr[i + 1], num, j);
                        }
                        i += offset * size - size;
                        outPut += configSet.GetStructArrayEnd(configName, fildName, num);
                    }
                    else
                    {
                        outPut += configSet.GetStruct(configName, structName, fildName);
                        for (int j = 0; j < size; j++, i++)
                        {
                            outPut += configSet.GetStructType(tempCodeTransform.items[j].name, fildStr[i + 1], i + 1);
                        }
                        outPut += configSet.GetStructEnd();
                    }
                }
                else if (typeStr[i].Contains("_"))
                {
                    string[] typeDivision = typeStr[i].Split('_');
                    if (typeDivision[0] == "Array")
                    {
                        string suffix = typeDivision[1];
                        if (suffix == "Struct")//结构体数组模式
                        {
                            flag = 1;
                            offset = int.Parse(fildStr[i]);
                            //structName = fildStr[i];
                        }
                        else if (Regex.IsMatch(suffix, @"^\d+$"))//数组模式1
                        {
                            int size = int.Parse(suffix);
                            string realTypeStr = typeStr[i + 1];
                            bool isDefine = false;
                            if (enumDic.TryGetValue(realTypeStr, out tempCodeTransform))
                            {
                                realTypeStr = tempCodeTransform.code;
                                isDefine = true;
                            }
                            arrayCache += configSet.GetArrayCache(realTypeStr, num, isDefine);
                            arrayCacheNew += configSet.GetArrayCacheNew(realTypeStr, num, size, isDefine);
                            outPut += configSet.GetArray(configName, realTypeStr, fildStr[i + 1], i, ref num, 1, size, isDefine);
                            i += size;
                        }
                        else//数组模式2
                        {
                            outPut += configSet.GetArray(configName, suffix, fildStr[i], i, ref num, 2);
                        }
                    }
                    else
                    {
                        Console.WriteLine("未能识别的类型前缀,请检查!  路径为：" + filePath + "  第" + i + "列");
                        throw new Exception();
                    }
                }
                else
                {
                    outPut += configSet.GetType( fildStr[i], typeStr[i], i);
                }
            }
            outPut += configSet.GetDeserializeEnd();
            outPut += arrayCache;

            outPut += configSet.GetInit(configName, hasNoUnique, strDic+ arrayCacheNew);

            for (int i = 0; i < typeStr.Count; i++)
            {
                if (!typeStr[i].Contains("_"))
                {
                    switch (indexTypes[i])
                    {
                        case indexType.UnIndex:
                            break;
                        case indexType.NoUnique:
                            outPut += configSet.GetDicAdd(fildStr[i], true, i, ref num);
                            break;
                        case indexType.Unique:
                            outPut += configSet.GetDicAdd(fildStr[i], false, i, ref num);
                            break;
                        default:
                            break;
                    }
                }
            }

            outPut += "\t\t" + configSet.GetEnd();
            outPut += strDicEnd;
            outPut += "\t" + configSet.GetEnd();

            outPut += configSet.GetStrAllConfig(configName);
            outPut += configSet.GetEnd();
            return outPut;
        }
        static private string GenerateCsv(string csvName, string[] text, string filePath, Dictionary<string, CodeTransform> enumDic, Dictionary<string, CodeTransform> structDic)
        {
            string str = "";
            string[] typeStr = text[1].Split(',');
            for (int i = 1; i < text.Length; i++)
            {
                if (i<4)
                {
                    continue;
                }
                string[] valueStr= text[i].Split(',');
                string strTemp = "";
                CodeTransform tempCodeTransform;
                for (int j = 0; j < valueStr.Length; j++)
                {
                    if (typeStr[j].Contains("note"))
                    {
                        continue;
                    }
                    string value = "";
                    if (i<3)
                    {
                        value = valueStr[j];
                    }
                    else
                    {
                        string type = typeStr[j];
                        if (enumDic.TryGetValue(type, out tempCodeTransform))
                        {
                            for (int k = 0; k < tempCodeTransform.items.Count; k++)
                            {
                                if (tempCodeTransform.items[k].name == valueStr[j])
                                {
                                    value = tempCodeTransform.items[k].value;
                                    break;
                                }
                                if (k == tempCodeTransform.items.Count)
                                {
                                    Console.WriteLine("未能识别的枚举,请检查!  路径为：" + filePath + "  第" + i + "行" + "  第" + j + "列");
                                    throw new Exception();
                                }
                            }
                        }
                        else
                        {
                            value = valueStr[j];
                        }
                    }
                    string dou = "";
                    if (j < valueStr.Length)
                    {
                        dou = ",";
                    }
                    strTemp += value + dou;
                }
                string temp = "";
                if (i < text.Length)
                {
                    temp = Environment.NewLine;
                }
                str += strTemp + temp;
            }
            return str;
        }
    }
}
