using System.Linq;
using System.Threading.Tasks;
using AzureMasterclass.Domain;
using AzureMasterclass.Domain.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AzureMasterclass.Function
{
    public class SearchBookFunction
    {
        private readonly IAzureMasterclassDatabaseContext _databaseContext;
        private readonly IIsbnSearchService _isbnSearchService;

        public SearchBookFunction(IAzureMasterclassDatabaseContext databaseContext, 
            IIsbnSearchService isbnSearchService)
        {
            _databaseContext = databaseContext;
            _isbnSearchService = isbnSearchService;
        }
        
        [FunctionName("SearchBook")]
        public async Task Run([TimerTrigger("%SearchBookSchedule%" 
#if DEBUG
            ,RunOnStartup = true
#endif
        )]TimerInfo timerInfo,  ILogger log)    
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            
            var booksToSearch = await _databaseContext.Books.Where(x => x.Isbn == null).ToArrayAsync();
            foreach (var book in booksToSearch)
            {
                var candidates = await _isbnSearchService.SearchIsbnByBookName(book.Name);
                if (candidates.Length > 0)
                {
                    book.UpdateIsbn(candidates[0]);
                }
            }
            
            await _databaseContext.SaveEntitiesAsync();
        }
    }
}
