using Ascent.Models;
using Microsoft.EntityFrameworkCore;

namespace Ascent.Services;

public class PageService
{
    private readonly AppDbContext _db;

    public PageService(AppDbContext db) { _db = db; }

    public Page GetPage(int id) => _db.Pages.Find(id);

    public List<Page> GetPinnedPages() => _db.Pages.AsNoTracking()
        .Where(p => p.IsPinned).OrderBy(p => p.Subject).ToList();

    public List<Page> GetLastViwedPages() => _db.Pages.AsNoTracking()
        .Where(p => p.IsRegular).OrderByDescending(n => n.TimeViewed).Take(20).ToList();

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
        if (!page.IsRegular) return; // Can only delete regular pages

        if (_db.PageRevisions.Where(h => h.PageId == page.Id).Count() == 0
            && (page.Content == null || page.Content.Length < 200))
            _db.Pages.Remove(page);
        else
            page.IsDeleted = true;
        _db.SaveChanges();
    }

    public PageRevision GetPageRevision(int pageId, int version) => _db.PageRevisions
        .Where(r => r.PageId == pageId && r.Version == version).SingleOrDefault();

    public List<PageRevisionDto> GetPageRevisions(int pageId) => _db.PageRevisions.AsNoTracking()
        .Where(r => r.PageId == pageId)
        .Select(r => new PageRevisionDto()
        {
            PageId = r.PageId,
            Version = r.Version,
            Subject = r.Subject,
            TimeCreated = r.TimeCreated
        })
        .OrderByDescending(o => o.TimeCreated).ToList();

    public void AddPageRevision(PageRevision revision)
    {
        var pageId = revision.Page?.Id ?? revision.PageId;
        var lastTimestamp = _db.PageRevisions.Where(r => r.PageId == pageId)
            .Select(r => r.TimeCreated).OrderByDescending(t => t).Take(1).SingleOrDefault();

        // Limit 1 new revision every 5 minutes
        if (lastTimestamp == default || DateTime.UtcNow.Subtract(lastTimestamp).TotalMinutes > 5)
        {
            revision.TimeCreated = DateTime.UtcNow;
            _db.PageRevisions.Add(revision);
            _db.SaveChanges();
        }
    }

    public void SaveChanges() => _db.SaveChanges();
}
