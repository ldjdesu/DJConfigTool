using System;
using System.Collections.Generic;
using System.Text;

namespace ConfigTool
{
    interface Model
    {
        //public string GetArray(string type,string fieldName);
        public string GetType(string type, string fieldName,bool isArray ,bool isDefine=false);
        public string GetStruct(string type);

        public string GetStructType(string type, string fieldName);

        public string GetStructEnd();
        public string GetStructField(string type,string fieldName, bool isArray);
        public string GetEnum(string type);

        public string GetEnumType(string fieldName);

        public string GetEnumEnd();

        public string GetStart();
        public string GetDefineHead();
        public string GetClassHead(string className);
        public string GetEnd();
    }
}
