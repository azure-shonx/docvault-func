using Newtonsoft.Json;

public class URLReply
{
    public string FileName { get; }
    public string URL { get; }

    public URLReply(string FileName, string URL)
    {
        this.FileName = FileName;
        this.URL = URL;
    }

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}