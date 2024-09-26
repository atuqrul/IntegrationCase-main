using Integration.Common;
using Integration.Backend;

namespace Integration.Service;

public sealed class ItemIntegrationService
{
    private ItemOperationBackend ItemIntegrationBackend { get; set; } = new();

    // SemaphoreSlim: Aynı anda yalnızca bir içeriğin kontrol edilip kaydedilmesini sağlar
    private static readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

    // Multithreaded ve paralel çağrılarda aynı içeriğin bir kez kaydedilmesini sağlıyoruz.
    public async Task<Result> SaveItemAsync(string itemContent)
    {
        // Kilidi edinmek için bekle (aynı anda yalnızca bir içerik kontrol edilecek)
        await _lock.WaitAsync();
        try
        {
            // Backend'de içerik daha önce kaydedilmiş mi kontrol ediyoruz
            if (ItemIntegrationBackend.FindItemsWithContent(itemContent).Count != 0)
            {
                return new Result(false, $"Duplicate item received with content {itemContent}.");
            }

            // Yeni içerik kaydediliyor
            var item = ItemIntegrationBackend.SaveItem(itemContent);

            return new Result(true, $"Item with content {itemContent} saved with id {item.Id}");
        }
        finally
        {
            // Kilidi serbest bırakıyoruz
            _lock.Release();
        }
    }

    // Tüm öğeleri listeleme
    public List<Item> GetAllItems()
    {
        return ItemIntegrationBackend.GetAllItems();
    }
}



