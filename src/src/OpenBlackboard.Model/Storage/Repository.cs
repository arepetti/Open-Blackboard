using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBlackboard.Model.Storage
{
    public sealed class Repository
    {
        public Repository(BackboardContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            _context = context;
        }

        public void Push(DataSet dataset)
        {

        }

        private readonly BackboardContext _context;
    }
}
