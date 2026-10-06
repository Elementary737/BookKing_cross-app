using Core.Abstractions;

namespace Core.Storage;

public static class StoreFactory
{
    public static IBookStore Create(string[] args, string dataPath)
    {
        if (args.Contains("--file"))
        {
            var fileStore = new FileBookStore(dataPath);
            if (fileStore.List().Count == 0)
            {
                foreach (var book in SampleData.Books())
                    fileStore.Add(book);
            }
            return fileStore;
        }

        return new InMemoryBookStore(SampleData.Books());
    }
}