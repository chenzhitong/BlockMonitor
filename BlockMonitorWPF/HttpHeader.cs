namespace BlockMonitor
{
    public class HttpHeader
    {
        public HttpHeader(string name, string value)
        {
            Name = name;
            Value = value;
        }
        public string Name;
        public string Value;
    }
}
