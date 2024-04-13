using System;
using System.Collections.Generic;
using System.Text;

namespace ConfigTool
{
    interface Model
    {
        public string GetArray(string type,string fieldName);
        public string GetType(string type, string fieldName);
        public string GetStruct(string fieldName);

        public string GetStructType(string type, string fieldName);

        public string GetStructEnd(string fieldName);
        public string GetStructField(string fieldName, bool isArray);
        
        public string GetStart();
        public string GetStructHead();
        public string GetClassHead(string className);
        public string GetEnd();
    }
}
