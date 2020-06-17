using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Models;

namespace Interfaces
{
    public interface ITfsService
    {
        Task<List<StatusRecord>> GetDataFromTFS(Uri collectionUri, string projectName, List<string> queryFolderHierarchy);
    }
}
