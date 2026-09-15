using System;
using System.Collections.Generic;
using System.Text;

namespace ConfigTool
{
    interface ConfigSet
    {
        /// <summary>生成跨列定长数组解析。</summary>
        public string GetArray(string configName, string typeName, string fieldName, int index, ref int num, int size,bool isDefine=false);
        public string GetType(string fieldName, string typeName, int index, bool isDefine = false);
        public string GetDicAdd(string fildName, bool isList, int index, ref int num);
        public string GetStruct(string configName, string typeName,string fieldName);
        public string GetStructTypeLoop( string typeName, string fieldName);
        
        public string GetStructType(string typeName, string fieldName, int index);

        public string GetStructEnd(bool isLoop=false);
        public string GetStructArray(string configName, string typeName, int index, ref int num, int arraySize, int structSize);
        public string GetStructArrayType(string typeName, string fieldName, int num,int offset);
        public string GetStructArrayEnd(string configName, string fieldName, int num);
        /// <summary>仅为表中存在的索引种类生成查询方法。</summary>
        public string GetKeyStrFunction(string configName, bool isUnique, bool hasUnique, bool hasNoUnique);
        public string GetDic(string fildName, bool isList);
        /// <summary>生成索引与缓存初始化。</summary>
        public string GetInit(string configName, bool hasUnique, bool hasNoUnique,string strDic);
        /// <summary>生成容器字段。</summary>
        public string GetStart(string className, bool hasUnique, bool hasNoUnique);
        public string GetDicEnd(string fildName, bool isList);

        public string GetDeserialize(string configName);
        public string GetDeserializeEnd();
        public string GetEnd();
        public string GetStrAllConfig(string configName);
    }
}
