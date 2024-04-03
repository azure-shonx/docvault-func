using Newtonsoft.Json;

public class CosmosURL
{
    public string id; // It's actually FileName but don't worry about it.
    public string URL;

    public DateTimeOffset expires;

    [JsonConstructor]
    public CosmosURL(String id, String URL, DateTimeOffset expires)
    {
        this.id = id;
        this.URL = URL;
        this.expires = expires;
    }

    public CosmosURL(URLReply cosmos) : this(cosmos.FileName, cosmos.URL, cosmos.expires) { }

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}