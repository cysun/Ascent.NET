using Ascent.Models;

namespace Ascent.Services
{
    public class MessageService
    {
        private readonly AppDbContext _db;

        public MessageService(AppDbContext db) { _db = db; }

        public void AddMessage(Message message)
        {
            _db.Messages.Add(message);
            _db.SaveChanges();
        }
    }
}
