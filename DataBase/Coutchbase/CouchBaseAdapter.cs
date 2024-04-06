using Couchbase.Extensions.DependencyInjection;
using Couchbase;
using Couchbase.KeyValue;

namespace DataBase.Coutchbase
{
    public interface ICouchBaseAdapter
    {
        Task<IGetResult> GetAndLockAsync(string key, TimeSpan expiry, GetAndLockOptions? options = null);
        Task<bool> ExistsAsync(string key, ExistsOptions? options = null);

        Task TouchAsync(string key, TimeSpan expiry, TouchOptions? options = null);

        Task<bool> InsertAsync(string key, object value, InsertOptions? options = null);
        Task<bool> UpsertAsync(string key, object value, UpsertOptions? options = null);
        Task<IGetResult?> GetAsync(string key);
        Task<IGetResult?> TryGetAsync(string key);

        Task<IGetResult> GetAndTouchAsync(string key, TimeSpan expiry, GetAndTouchOptions? options = null);

        Task<bool> RemoveAsync(string key);

        Task UnlockAsync(string key, ulong cas, UnlockOptions? options = null);

    }

    public class CouchBaseAdapter : ICouchBaseAdapter
    {
        private readonly IBucket _bucket;
        private readonly ICouchbaseCollection _collection;
        public CouchBaseAdapter(IBucketProvider bucketProvider)
        {
            try
            {
                _bucket = bucketProvider.GetBucketAsync("AeroflotData").Result;
                _collection = _bucket.DefaultCollection();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public async Task<bool> ExistsAsync(string key, ExistsOptions? options = null)
        {
            if (_collection is null) 
                return false;
            
            try
            {
                var result = await _collection.ExistsAsync(key, options);

                return result.Exists;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public async Task<bool> InsertAsync(string key, object value, InsertOptions? options = null)
        {   
            if (_collection is null) 
                return false;
            
            try
            {
                await _collection.InsertAsync(key, value, options);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public async Task<bool> UpsertAsync(string key, object value, UpsertOptions? options = null)
        {
            if (_collection is null) 
                return false;
            
            try
            {
                await _collection.UpsertAsync(key, value, options);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public async Task<IGetResult> GetAndLockAsync(string key, TimeSpan expiry, GetAndLockOptions? options = null)
        {
            if (_collection is null) 
                return null;
            
            try
            {
                var result = await _collection.GetAndLockAsync(key,expiry,options);
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }
        }

        public async Task<IGetResult?> GetAsync(string key)
        {
            if (_collection is null) 
                return null;
            
            try
            {
                var result = await _collection.GetAsync(key);
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }
        }

        public async Task<IGetResult?> TryGetAsync(string key)
        {
            if (_collection is null) 
                return null;
            
            try
            {
                var result = await _collection.TryGetAsync(key);
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }
        }

        public async Task<bool> RemoveAsync(string key)
        {
            if (_collection is null) 
                return false;
            
            try
            {
                await _collection.RemoveAsync(key);
                return true;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }

        public async Task TouchAsync(string key, TimeSpan expiry, TouchOptions? options = null)
        {
            if (_collection is null) 
                return;
            
            try
            {
                await _collection.TouchAsync(key, expiry,options);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public async Task<IGetResult> GetAndTouchAsync(string key, TimeSpan expiry, GetAndTouchOptions? options = null)
        {
            if (_collection is null) 
                return null;
            
            try
            {
                var result = await _collection.GetAndTouchAsync(key, expiry, options);
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }
        }

        public async Task UnlockAsync(string key, ulong cas, UnlockOptions? options = null)
        {
            if (_collection is null) 
                return;
            
            try
            {
                await _collection.UnlockAsync(key, cas, options);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }
    }
}
