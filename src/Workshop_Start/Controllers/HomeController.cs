using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using AuthorizationWorkshop.Models;
using AuthorizationWorkshop.Repositories;

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

        public IActionResult Edit(Guid id)
        {
            var album = _albumRepository.Get(id);
            if (album == null)
            {
                return new NotFoundResult();
            }

            return View(album);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Album album)
        {
            var existingAlbum = _albumRepository.Get(album.Id);
            if (existingAlbum == null)
            {
                return new NotFoundResult();
            }

            _albumRepository.Update(album);

            return RedirectToAction("Details", new { id = album.Id });
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
