using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protean.Providers
{
    namespace Cache
    {
        public interface ICacheProvider
        {
            void Initiate(ref Cms myWeb);
            void PurgeAll();

            void PurgeFile(string fileName);
            void PurgeContentByTag(string tagHostOrPrefix);
        }

        public class DefaultProvider : ICacheProvider
        {
            public void Initiate(ref Cms myWeb)
            {
                throw new NotImplementedException();
            }

            public void PurgeAll()
            {
                throw new NotImplementedException();
            }

            public void PurgeContentByTag(string tagHostOrPrefix)
            {
                throw new NotImplementedException();
            }

            public void PurgeFile(string fileName)
            {
                throw new NotImplementedException();
            }
        }
    }
}
