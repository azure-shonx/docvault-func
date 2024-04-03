using Newtonsoft.Json;

public class URLReply
{
    public string FileName { get; }
    public string URL { get; }

    public DateTimeOffset expires { get; }

    [JsonConstructor]
    public URLReply(string FileName, string URL, DateTimeOffset expires)
    {
        this.FileName = FileName;
        this.URL = URL;
        this.expires = expires;
    }

    public URLReply(CosmosURL cosmos) : this(cosmos.id, cosmos.URL, cosmos.expires) { }

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}