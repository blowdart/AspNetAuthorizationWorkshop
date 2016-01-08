using AuthorizationLab.Models;
using System;
using System.Collections.Generic;

namespace AuthorizationLab.Repositories
{
    public interface IAlbumRepository
    {
        IEnumerable<Album> Get();
        Album Get(Guid id);
        Album Update(Album album);
    }
}
