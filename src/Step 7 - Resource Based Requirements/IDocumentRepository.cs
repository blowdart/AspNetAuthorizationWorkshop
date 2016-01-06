using System.Collections.Generic;

namespace AuthorizationLab
{
    public interface IDocumentRepository
    {
        IEnumerable<Document> Get();

        Document Get(int id);
    }
}
