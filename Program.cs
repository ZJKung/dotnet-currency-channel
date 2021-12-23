using System.Collections.Concurrent;
using System.Threading.Channels;
var htmlCollection = Channel.CreateBounded<string>(3);

var urls = new ConcurrentQueue<string>(new[]{
    "https://www.google.com",
    "https://www.bing.com",
    "https://www.yahoo.com",
});

using HttpClient client = new();
await Task.Factory.StartNew(async () =>
{
    var tasks = urls.Select(async url =>
    {
        var html = await (await client.GetAsync(url)).Content.ReadAsStringAsync();
        await htmlCollection.Writer.WriteAsync(html);
    });
    await Task.WhenAll(tasks);
    htmlCollection.Writer.Complete();
});


while (await htmlCollection.Reader.WaitToReadAsync())
{
    if (htmlCollection.Reader.TryRead(out var message))
    {
        Console.WriteLine(message);
    }
}
