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
using CsvHelper;
using CsvHelper.Configuration;
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
            public int size=1;//占表格列数
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

        /// <summary>生成配置；失败返回非零退出码，便于批处理停止同步。</summary>
        static int Main(string[] args)
        {
            Model model = new CSharpModel();
            ConfigSet configSet = new CSharpConfigSet();
            string root = args.Length == 0 || string.IsNullOrEmpty(args[0]) ? "." : args[0];
            string csvPath = Path.Combine(root, "Data", "CsvOld");
            string csvNewPath = Path.Combine(root, "Data", "CsvNew");
            string cSharpPath = Path.Combine(root, "Data", "CSharp");
            string md5Path = Path.Combine(root, "Data", "MD5");
            string ConfigDefinePath = Path.Combine(root, "ConfigDefine.xml");
            try
            {
                Directory.CreateDirectory(csvNewPath);
                Directory.CreateDirectory(cSharpPath);
                Directory.CreateDirectory(md5Path);
                // 模板程序集或 XML 定义变化时，未改动的表也必须重新生成。
                string generatorStamp = typeof(Program).Assembly.ManifestModule.ModuleVersionId.ToString() + File.ReadAllText(ConfigDefinePath);
                XmlDocument definedoc = new XmlDocument();
                definedoc.Load(ConfigDefinePath);
                Dictionary<string, CodeTransform> enumDic = new Dictionary<string, CodeTransform>();
                XmlNodeList enumNodeList = definedoc.SelectNodes("/definitions/enumDefine/enum");
                for (int i = 0; i < enumNodeList.Count; i++)
                {
                    string enumStr = enumNodeList[i].Attributes["name"].Value;
                    string enumCode = enumNodeList[i].Attributes["code"].Value;
                    XmlNodeList fieldList = enumNodeList[i].SelectNodes("item");
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
                    XmlNodeList fieldList = structNodeList[i].SelectNodes("field");
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
                //占表格列数计算
                foreach (var structItem in structDic)
                {
                    int size = 0;
                    foreach (var fieldItem in structItem.Value.items)
                    {
                        CodeTransform temp;
                        size++;
                        if (structDic.TryGetValue(fieldItem.name, out temp))
                        {
                            size += temp.items.Count;
                        }
                    }
                    structItem.Value.size = size;
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

                    List<string[]> csvRows = ReadCsvRows(filePath);
                    //======================构建model类 Start
                    string outText = GenerateModel(model, configName, csvRows, filePath, enumDic, structDic);

                    using MD5 md5 = MD5.Create();
                    Byte[] newMD5Bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(generatorStamp + str));
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
                        if (newMD5Str == oldMD5 &&
                            File.Exists(Path.Combine(cSharpPath, configName + "DataConfig.cs")) &&
                            File.Exists(Path.Combine(cSharpPath, configName + "_ConfigSet.cs")) &&
                            File.Exists(Path.Combine(csvNewPath, configName + ".csv")))//对比md5及输出完整性
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
                    outText = GenerateSet(configSet, configName, csvRows, filePath, enumDic, structDic);
                    outPutPath = cSharpPath + "\\" + configName + "_ConfigSet.cs";
                    File.WriteAllText(outPutPath, outText, Encoding.UTF8);
                    //======================构建set类 End

                    //======================替换csv文件 Start
                    List<string[]> outputRows = GenerateCsv(configName, csvRows, filePath, enumDic, structDic);
                    outPutPath = csvNewPath + "\\" + configName + ".csv";
                    WriteCsvRows(outPutPath, outputRows);
                    //======================替换csv文件 End
                }
                //======================构建Define类 Start
                defineStr = model.GetStart() + model.GetDefineHead();
                for (int i = 0; i < enumNodeList.Count; i++)
                {
                    defineStr += model.GetEnum(enumNodeList[i].Attributes["code"].Value, enumNodeList[i].Attributes["name"].Value);
                    XmlNodeList fieldList = enumNodeList[i].SelectNodes("item"); ;
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
                    XmlNodeList fieldList = structNodeList[i].SelectNodes("field"); ;
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
                        else if (structDic.TryGetValue(field.Attributes["name"].Value, out tempCodeTransform))
                        {
                            realTypeStr = tempCodeTransform.code;
                            notesStr = tempCodeTransform.name;
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
                Console.Error.WriteLine(e.ToString());
                return 1;
            }

            Console.WriteLine("构建完成");
            return 0;
        }

        static private List<string[]> ReadCsvRows(string filePath)
        {
            var rows = new List<string[]>();
            var config = new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
                MissingFieldFound = null,
                BadDataFound = null
            };
            using var reader = new StreamReader(filePath, Encoding.UTF8);
            using var parser = new CsvParser(reader, config);
            while (parser.Read())
            {
                string[] row= parser.Record;
                rows.Add(row);
            }
            return rows;
        }

        /// <summary>写入CSV；合法空表保留空行，供生产解析器读取零条记录。</summary>
        static private void WriteCsvRows(string filePath, List<string[]> rows)
        {
            var config = new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false
            };
            using var writer = new StreamWriter(filePath, false, new UTF8Encoding(true));
            using var csvWriter = new CsvWriter(writer, config);
            // 合法的四行表头空表写入一个空行，生产解析器可据此读取零条记录。
            if (rows.Count == 0) writer.WriteLine();
            foreach (var row in rows)
            {
                foreach (var cell in row)
                {
                    csvWriter.WriteField(cell);
                }
                csvWriter.NextRecord();
            }
        }

        static private string GetCell(string[] row, int index)
        {
            if (index >= 0 && index < row.Length)
            {
                return row[index];
            }
            return string.Empty;
        }

        /// <summary>生成配置行类型，仅支持跨列定长数组及结构体数组。</summary>
        static private string GenerateModel(Model model, string configName, List<string[]> csvRows, string filePath, Dictionary<string, CodeTransform> enumDic, Dictionary<string, CodeTransform> structDic)
        {
            //构建开头
            string outPut = model.GetStart();
            outPut += model.GetClassHead(configName);
            string[] notesStr = csvRows[0];
            string[] typeStr = csvRows[1];
            string[] fildStr = csvRows[2];
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
                    int size = tempCodeTransform.size;
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
                        else
                        {
                            throw new InvalidDataException("已移除分号数组 " + typeStr[i] + "，请使用 Array_N 跨列定长数组。路径：" + filePath);
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
            outPut += model.GetIsNull();
            outPut += model.GetEnd();
            return outPut;
        }

        /// <summary>根据字段和索引元数据生成查询 API 与逐行解析代码。</summary>
        static private string GenerateSet(ConfigSet configSet, string configName, List<string[]> csvRows, string filePath, Dictionary<string, CodeTransform> enumDic, Dictionary<string, CodeTransform> structDic)
        {
            string[] typeStrTemp = csvRows[1];//0行是注释，跳过
            string[] fildStrTemp = csvRows[2];
            string[] indexStrTemp = csvRows[3];
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
            bool hasUnique = indexStr.Contains("Index_Unique");
            bool hasNoUnique = indexStr.Contains("Index_NoUnique");
            //构建开头
            string outPut = configSet.GetStart(configName, hasUnique, hasNoUnique);
            //构建字段
            string strDic = "";//索引的开始
            string strDicEnd = "";//索引的结尾
            indexType[] indexTypes = new indexType[typeStr.Count];//索引类型标记（0为不索引，1为Unique，2为NoUnique）
            for (int i = 0; i < indexStr.Count; i++)
            {
                if (indexStr[i].Length != 0)
                {
                    string[] indexDivision = indexStr[i].Split('_');
                    if (indexDivision.Length == 2 && indexDivision[0] == "Index")
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
            outPut += configSet.GetKeyStrFunction(configName, true, hasUnique, hasNoUnique);
            outPut += configSet.GetKeyStrFunction(configName, false, hasUnique, hasNoUnique);

            int flag = 0;//标志位(1为结构体数组模式)
            int offset = 0;//长度位
            string structName = "";//缓存结构体字段名
            int num = 1;//临时变量名序号
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
                    int size = tempCodeTransform.size;
                    structName = tempCodeTransform.code;
                    string fildName = fildStr[i];
                    if (isArray)
                    {
                        outPut += configSet.GetStructArray(configName, structName, i, ref num, offset, size);
                        int nowIndex = i;
                        CodeTransform tempCodeTransformLoop;
                        for (int j = 0; j < tempCodeTransform.items.Count; j++, i++)
                        {
                            string realTypeStr = tempCodeTransform.items[j].name;

                            if (enumDic.TryGetValue(realTypeStr, out tempCodeTransform2))
                            {
                                realTypeStr = tempCodeTransform2.code;
                            }
                            if (structDic.TryGetValue(tempCodeTransform.items[j].name, out tempCodeTransformLoop))
                            {
                                outPut += "\t" + configSet.GetStructTypeLoop(tempCodeTransformLoop.code, fildStr[i + 1]);
                                i++;
                                int start = i;
                                for (; i < start + tempCodeTransformLoop.items.Count; i++)
                                {
                                    realTypeStr = tempCodeTransformLoop.items[i - start].name;
                                    if (enumDic.TryGetValue(realTypeStr, out tempCodeTransform2))
                                    {
                                        realTypeStr = tempCodeTransform2.code;
                                    }
                                    outPut += "\t" + configSet.GetStructArrayType(realTypeStr, fildStr[i + 1], num,i- nowIndex);
                                }
                                outPut += "\t\t" + configSet.GetStructEnd(true);
                                i--;
                            }
                            else
                            {
                                outPut += configSet.GetStructArrayType(realTypeStr, fildStr[i + 1], num, i - nowIndex);
                            } 
                        }
                        i += offset * size - size;
                        outPut += configSet.GetStructArrayEnd(configName, fildName, num);
                        flag = 0;
                    }
                    else
                    {
                        outPut += configSet.GetStruct(configName, structName, fildName);
                        CodeTransform tempCodeTransformLoop;
                        for (int j = 0; j < tempCodeTransform.items.Count; j++,i++)
                        {

                            if (structDic.TryGetValue(tempCodeTransform.items[j].name, out tempCodeTransformLoop))
                            {
                                outPut += configSet.GetStructTypeLoop(tempCodeTransformLoop.code, fildStr[i+1]);
                                i++;
                                int start = i;
                                for (; i < start + tempCodeTransformLoop.items.Count; i++)
                                {
                                    outPut += "\t" + configSet.GetStructType(tempCodeTransformLoop.items[i- start].name, fildStr[i+1], i+1);
                                }
                                outPut += "\t" + configSet.GetStructEnd(true);
                                i--;
                            }
                            else
                            {
                                outPut += configSet.GetStructType(tempCodeTransform.items[j].name, fildStr[i+1], i+1);
                            }
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
                            outPut += configSet.GetArray(configName, realTypeStr, fildStr[i + 1], i, ref num, size, isDefine);
                            i += size;
                        }
                        else
                        {
                            throw new InvalidDataException("已移除分号数组 " + typeStr[i] + "，请使用 Array_N 跨列定长数组。路径：" + filePath);
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

            outPut += configSet.GetInit(configName, hasUnique, hasNoUnique, strDic);

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
        static private List<string[]> GenerateCsv(string csvName, List<string[]> csvRows, string filePath, Dictionary<string, CodeTransform> enumDic, Dictionary<string, CodeTransform> structDic)
        {
            var outputRows = new List<string[]>();
            string[] typeStr = csvRows[1];
            for (int i = 4; i < csvRows.Count; i++)
            {
                string[] valueStr= csvRows[i];
                var row = new List<string>();
                CodeTransform tempCodeTransform;
                for (int j = 0; j < typeStr.Length; j++)
                {
                    if (typeStr[j].Contains("note"))
                    {
                        continue;
                    }
                    string value="";
                    string sourceValue = GetCell(valueStr, j);
                    string type = typeStr[j];
                    if (!string.IsNullOrEmpty(sourceValue)&&enumDic.TryGetValue(type, out tempCodeTransform))
                    {
                        int k = 0;
                        for (; k < tempCodeTransform.items.Count; k++)
                        {
                            if (tempCodeTransform.items[k].name == sourceValue)
                            {
                                value = tempCodeTransform.items[k].value;
                                break;
                            }
                        }
                        if (k == tempCodeTransform.items.Count)
                        {
                            Console.WriteLine("未能识别的枚举,请检查!  路径为：" + filePath + "  第" + i + "行" + "  第" + j + "列");
                            throw new Exception();
                        }
                    }
                    else
                    {
                        value = sourceValue;
                    }
                    row.Add(value);
                }
                outputRows.Add(row.ToArray());
            }
            return outputRows;
        }
    }
}
