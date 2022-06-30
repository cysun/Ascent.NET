using Ascent.Models;
using Microsoft.EntityFrameworkCore;

namespace Ascent.Services;

public class PageService
{
    private readonly AppDbContext _db;

    public PageService(AppDbContext db)
    {
        _db = db;
    }

    public Page GetPage(int id) => _db.Pages.Find(id);

    public List<Page> GetLastViwedPages() => _db.Pages.AsNoTracking()
        .OrderByDescending(n => n.TimeViewed).Take(20).ToList();

    // maxResults=null for unlimited results
    public List<Page> SearchPages(string searchText, int? maxResults = 20)
    {
        if (string.IsNullOrWhiteSpace(searchText)) return new List<Page>();

        return _db.Pages.FromSqlRaw("SELECT * FROM \"SearchPages\"({0}, {1})", searchText, maxResults)
            .AsNoTracking().ToList();
    }

    public void AddPage(Page page)
    {
        _db.Pages.Add(page);
        _db.SaveChanges();
    }

    public void DeletePage(Page page)
    {
        if (_db.PageHistories.Where(h => h.PageId == page.Id).Count() == 0
            && (page.Content == null || page.Content.Length < 200))
            _db.Pages.Remove(page);
        else
            page.IsDeleted = true;
        _db.SaveChanges();
    }

    public void SaveChanges() => _db.SaveChanges();
}
