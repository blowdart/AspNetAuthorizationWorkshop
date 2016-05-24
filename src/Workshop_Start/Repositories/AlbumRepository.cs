using System;
using System.Collections.Generic;
using System.Linq;
using AuthorizationWorkshop.Models;

namespace AuthorizationWorkshop.Repositories
{
    public class AlbumRepository : IAlbumRepository
    {
        private static List<Album> _albums = new List<Album>
        {
            new Album { Id = Guid.NewGuid(), Title = "Songs to make your ears bleed", Artist = "David and the Fowlers", Publisher = CompanyNames.ToneDeafRecords },
            new Album { Id = Guid.NewGuid(), Title = "Irish jigs to RiverDance to", Artist = "Barry O'Dorrans", Publisher = CompanyNames.PaddyProductions },
            new Album { Id = Guid.NewGuid(), Title = "Didgeridoo songs to delight and seduce", Artist = "Downunder Damian Edwards",  Publisher = CompanyNames.ToneDeafRecords }
        };

        public IEnumerable<Album> Get()
        {
            return _albums.OrderBy(a => a.Title);
        }

        public Album Get(Guid id)
        {
            return (_albums.FirstOrDefault(a => a.Id == id));
        }

        public Album Update(Album album)
        {
            lock (_albums)
            {
                foreach (var item in _albums.Where(a => a.Id == album.Id))
                {
                    item.Title = album.Title;
                    item.Artist = album.Artist;
                }
            }

            return album;
        }
    }
}