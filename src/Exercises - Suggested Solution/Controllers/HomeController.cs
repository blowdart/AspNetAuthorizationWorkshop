using System;
using System.Threading.Tasks;

using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;

using AuthorizationLab.Repositories;
using AuthorizationLab.Models;

namespace AuthorizationLab.Controllers
{
    public class HomeController : Controller
    {
        IAuthorizationService _authorizationService;
        IAlbumRepository _albumRepository;

        public HomeController(
            IAuthorizationService authorizationService,
            IAlbumRepository albumRepository)
        {
            _authorizationService = authorizationService;
            _albumRepository = albumRepository;
        }

        public IActionResult Index()
        {
            return View(_albumRepository.Get());
        }

        public IActionResult Details(Guid id)
        {
            var album = _albumRepository.Get(id);
            if (album == null)
            {
                return new HttpNotFoundResult();
            }

            return View(album);
        }

        [Authorize(Policy = PolicyNames.AdministratorsOnly)]
        public async Task<IActionResult> Edit(Guid id)
        {
            var album = _albumRepository.Get(id);
            if (album == null)
            {
                return new HttpNotFoundResult();
            }

            if (await _authorizationService.AuthorizeAsync(
                User,
                album,
                PolicyNames.CanEditAlbum))
            {
                return View(album);
            }

            return new ChallengeResult();
        }

        [Authorize(Policy = PolicyNames.AdministratorsOnly)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Album album)
        {
            var existingAlbum = _albumRepository.Get(album.Id);
            if (existingAlbum == null)
            {
                return new HttpNotFoundResult();
            }

            if (await _authorizationService.AuthorizeAsync(
                User,
                existingAlbum,
                PolicyNames.CanEditAlbum))
            {
                _albumRepository.Update(album);
                return View(album);
            }

            return RedirectToAction("Details", new { id = album.Id });
        }
    }
}
