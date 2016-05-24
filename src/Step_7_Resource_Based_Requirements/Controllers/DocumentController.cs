using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthorizationLab.Controllers
{
    public class DocumentController : Controller
    {
        IDocumentRepository _documentRepository;
        IAuthorizationService _authorizationService;

        public DocumentController(IDocumentRepository documentRepository,
                                  IAuthorizationService authorizationService)
        {
            _documentRepository = documentRepository;
            _authorizationService = authorizationService;
        }

        public IActionResult Index()
        {
            return View(_documentRepository.Get());
        }

        public async Task<IActionResult> Edit(int id)
        {
            var document = _documentRepository.Get(id);

            if (document == null)
            {
                return new NotFoundResult();
            }

            if (await _authorizationService.AuthorizeAsync(User, document, new EditRequirement()))
            {
                return View(document);
            }
            else
            {
                return new ChallengeResult();
            }
        }
    }
}