using System.Text.Json;

namespace AzureMasterclass.Domain.Services;

public interface IIsbnSearchService
{
    Task<string[]> SearchIsbnByBookName(string bookName);
}


record BookSearchResult(string Kind, int TotalItems, Volume[] Items);

record Volume(string Kind, string Id, VolumeInfo VolumeInfo);

record VolumeInfo(string Title, string Subtitle, string Description, int PageCount, IndustryIdentifier[] IndustryIdentifiers);

record IndustryIdentifier(string Type, string Identifier);

public class IsbnSearchService: IIsbnSearchService
{
    private readonly IHttpClientFactory _httpClientFactory;
    
    public IsbnSearchService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<string[]> SearchIsbnByBookName(string bookName)
    {
        // api documentation: https://developers.google.com/books/docs/v1/reference/volumes/list
        var googleBookApiEndpoint = $"https://www.googleapis.com/books/v1/volumes?q={bookName}";
        
        var httpClient = _httpClientFactory.CreateClient();
        var res = await httpClient.GetAsync(googleBookApiEndpoint);
        
        if (!res.IsSuccessStatusCode)
        {
            throw new ApplicationException($"Google Book Search API responded with status code {res.StatusCode}.");
        }
        
        await using var responseStream = await res.Content.ReadAsStreamAsync();
        var reader = new StreamReader(responseStream);
        
        var json = await reader.ReadToEndAsync();
        var results = JsonSerializer.Deserialize<BookSearchResult>(
            json, 
            new JsonSerializerOptions{PropertyNameCaseInsensitive = true});
        if (results == null)
        {
            throw new ApplicationException($"Response ({json}) could not be deserialized");
        }

        return results.Items.Select(x => x.VolumeInfo.IndustryIdentifiers.FirstOrDefault(i => i.Type.Equals("ISBN_13")))
            .Where(x => x != null)
            .Select(x => x!.Identifier)
            .ToArray();
    }
}