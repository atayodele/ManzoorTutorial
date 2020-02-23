using SSBOL;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SSDAL
{
    public interface IStoryDb
    {
        IQueryable<Story> GetAll();
        IQueryable<Story> GetAll(bool IsApproved);
        IQueryable<Story> GetById(int SSid);

        IQueryable<Story> GetByUserId(string id);
        Task<bool> Create(Story story);
        Task<bool> Update(Story story);
        Task<bool> Delete(int SSid);
        Task<bool> Approve(int SSid);
    }
    public class StoryDb : IStoryDb
    {
        SSDbContext dbContext;

        public StoryDb(SSDbContext _dbContext)
        {
            dbContext = _dbContext;
        }

        public async Task<bool> Approve(int SSid)
        {
            var s = dbContext.Stories.Find(SSid);
            s.IsApproved = true;
            dbContext.Update(s);
            var result = await dbContext.SaveChangesAsync();
            if (result != 0)
                return true;
            else
                return false;
        }

        public async Task<bool> Create(Story story)
        {
            dbContext.Add(story);
            var result = await dbContext.SaveChangesAsync();
            if (result != 0)
                return true;
            else
                return false;
        }

        public async Task<bool> Delete(int SSid)
        {
            var s = dbContext.Stories.Find(SSid);
            dbContext.Remove(s);
            var result = await dbContext.SaveChangesAsync();
            if (result != 0)
                return true;
            else
                return false;
        }
        public async Task<bool> Update(Story story)
        {
            dbContext.Update(story);
            var result = await dbContext.SaveChangesAsync();
            if (result != 0)
                return true;
            else
                return false;
        }

        public IQueryable<Story> GetAll()
        {
            return dbContext.Stories;
        }

        public IQueryable<Story> GetAll(bool IsApproved)
        {
            return dbContext.Stories.Where(x => x.IsApproved == IsApproved);
        }

        public IQueryable<Story> GetById(int SSid)
        {
            return dbContext.Stories.Where(x => x.SSId == SSid);
        }

        public IQueryable<Story> GetByUserId(string id)
        {
            return dbContext.Stories.Where(x => x.Id == id);
        }
    }
}
