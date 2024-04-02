using Newtonsoft.Json;

public class URLRequest
{
    public string FileName { get; }

    public URLRequest(string FileName)
    {
        this.FileName = FileName;
    }

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}