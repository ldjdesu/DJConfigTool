using System;
using System.Collections.Generic;
using System.Text;

namespace ConfigTool
{
    interface Model
    {
        //public string GetArray(string type,string fieldName);
        public string GetType(string type, string fieldName,bool isArray ,bool isDefine=false,string notes="");
        public string GetStruct(string type, string notes);

        public string GetStructType(string type, string fieldName, bool isDefine = false, string notes = "");

        public string GetStructEnd();
        public string GetStructField(string type,string fieldName, bool isArray, string notes = "");
        public string GetEnum(string type, string notes);

        public string GetEnumType(string fieldName, int index, string notes = "");

        public string GetEnumEnd();

        public string GetStart();
        public string GetDefineHead();
        public string GetClassHead(string className);
        public string GetIsNull();
        public string GetEnd();
    }
}
