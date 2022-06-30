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
        if (_db.PageRevisions.Where(h => h.PageId == page.Id).Count() == 0
            && (page.Content == null || page.Content.Length < 200))
            _db.Pages.Remove(page);
        else
            page.IsDeleted = true;
        _db.SaveChanges();
    }

    public PageRevision GetPageRevision(int pageId, int version) => _db.PageRevisions
        .Where(r => r.PageId == pageId && r.Version == version).SingleOrDefault();

    public List<PageRevisionDto> GetPageRevisions(int pageId) => _db.PageRevisions.Where(r => r.PageId == pageId)
        .Select(r => new PageRevisionDto()
        {
            PageId = r.PageId,
            Version = r.Version,
            Subject = r.Subject,
            TimeCreated = r.TimeCreated
        })
        .AsNoTracking().OrderByDescending(o => o.TimeCreated).ToList();

    public void AddPageRevision(PageRevision revision)
    {
        _db.PageRevisions.Add(revision);
        _db.SaveChanges();
    }

    public void SaveChanges() => _db.SaveChanges();
}
