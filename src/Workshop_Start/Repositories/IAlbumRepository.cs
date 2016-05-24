using System;
using System.Collections.Generic;
using AuthorizationWorkshop.Models;

namespace AuthorizationWorkshop.Repositories
{
    public interface IAlbumRepository
    {
        IEnumerable<Album> Get();
        Album Get(Guid id);
        Album Update(Album album);
    }
}
