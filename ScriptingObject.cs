namespace MySQLScripter
{
    public class ScriptingObject
    {
        public string Type { get; set; }
        public string GetList { get; set; }
        public string GetObject { get; set; }
        public string OutputField { get; set; }
        public ScriptingObject(string type, string getList, string getObject, string outputField)
        {
            Type = type;
            GetList = getList;
            GetObject = getObject;
            OutputField = outputField;
        }
    }
}
