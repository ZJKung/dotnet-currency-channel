using System.Collections.Concurrent;
using System.Threading.Channels;
Channel<string> htmlCollection = Channel.CreateBounded<string>(4);

ConcurrentQueue<string> urls = new ConcurrentQueue<string>(new[]{
    "https://www.google.com",
    "https://www.bing.com",
    "https://www.yahoo.com",
    "https://github.com",
});

using HttpClient client = new();
await Task.Run(async () =>
{
    IEnumerable<Task> tasks = urls.Select(async url =>
    {
        var html = await (await client.GetAsync(url)).Content.ReadAsStringAsync();
        await htmlCollection.Writer.WriteAsync(html);
    });
    await Task.WhenAll(tasks);
    htmlCollection.Writer.Complete();
});

await foreach (var html in htmlCollection.Reader.ReadAllAsync())
{
    Console.WriteLine(html);
}
