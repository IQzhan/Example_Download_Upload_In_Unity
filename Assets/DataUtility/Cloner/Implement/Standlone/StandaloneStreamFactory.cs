using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E.Data
{
    public class StandaloneStreamFactory : StreamFactory
    {
        protected StandaloneStreamFactory() { }

        public override IStream GetStream(Uri uri)
        {
            throw new NotImplementedException();
        }
    }
}
