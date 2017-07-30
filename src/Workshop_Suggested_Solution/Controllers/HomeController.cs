using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using AuthorizationWorkshop.Models;
using AuthorizationWorkshop.Repositories;
using System.Threading.Tasks;

namespace AuthorizationWorkshop.Controllers
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
                return new NotFoundResult();
            }

            return View(album);
        }

        [Authorize(Policy = PolicyNames.AdministratorsOnly)]
        public async Task<IActionResult> Edit(Guid id)
        {
            var album = _albumRepository.Get(id);
            if (album == null)
            {
                return new NotFoundResult();
            }

            var authorizationResult = await _authorizationService.AuthorizeAsync(
                User,
                album,
                PolicyNames.CanEditAlbum);

            if (authorizationResult.Succeeded)
            {
                return View(album);
            }

            return new ForbidResult();
        }

        [Authorize(Policy = PolicyNames.AdministratorsOnly)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Album album)
        {
            var existingAlbum = _albumRepository.Get(album.Id);
            if (existingAlbum == null)
            {
                return new NotFoundResult();
            }

            var authorizationResult = await _authorizationService.AuthorizeAsync(
                User,
                existingAlbum,
                PolicyNames.CanEditAlbum);

            if (authorizationResult.Succeeded)
            {
                _albumRepository.Update(album);
                return View(album);
            }

            return RedirectToAction("Details", new { id = album.Id });
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
