﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ConfigTool
{
    interface ConfigSet
    {
        public string GetArray(string configName, string typeName, string fieldName, int index, ref int num, int flag, int size=0,bool isDefine=false);
        public string GetArrayCache( string typeName, int num, bool isDefine = false);
        public string GetArrayCacheNew( string typeName, int num, int size, bool isDefine = false);
        public string GetType(string fieldName, string typeName, int index, bool isDefine = false);
        public string GetDicAdd(string fildName, bool isList, int index, ref int num);
        public string GetStruct(string configName, string typeName,string fieldName);
        public string GetStructTypeLoop( string typeName, string fieldName);
        
        public string GetStructType(string typeName, string fieldName, int index);

        public string GetStructEnd(bool isLoop=false);
        public string GetStructArray(string configName, string typeName, int index, ref int num, int arraySize, int structSize);
        public string GetStructArrayType(string typeName, string fieldName, int num,int offset);
        public string GetStructArrayEnd(string configName, string fieldName, int num);
        public string GetKeyStrFunction(string configName, bool isUnique, bool hasNoUnique);
        public string GetDic(string fildName, bool isList);
        public string GetInit(string configName, bool hasNoUnique,string strDic);
        public string GetStart(string className,bool hasNoUnique);
        public string GetDicEnd(string fildName, bool isList);

        public string GetDeserialize(string configName);
        public string GetDeserializeEnd();
        public string GetEnd();
        public string GetStrAllConfig(string configName);
    }
}
