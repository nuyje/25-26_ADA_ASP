using Bulky.Data;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Hosting; 

namespace BulkyWeb.Controllers
{
    public class ProductController : Controller
    {
        //het _db object maakt het mogelijk om Entity Framework te gebruiken
        private readonly ApplicationDbContext _db;

        //Mogelijk maken om met afbeeldingen te werken via volgend object
        private readonly IWebHostEnvironment _webHostEnvironment; 

        //aan de constructor geef je een ApplicationDbContext object mee 
        //we hebben dit geregistreerd in de services (Program.cs)
        //hier zit heel de configuratie mee in van de EF Core
        //die we eerder maakten (incl connections string)

        //constructor 
        public ProductController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment; 
        }
        public IActionResult Index()
        {
            List<Product> objProductList = _db.Products.ToList();
            return View(objProductList);
        }
        public IActionResult Create()
        {
            IEnumerable<SelectListItem> categoryList =
                _db.Categories.Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });

            ProductVM productVM = new ProductVM();
            productVM.Product = new Product();
            productVM.CategoryList = categoryList;
            return View(productVM);
        }

        [HttpPost]
        public IActionResult Create(ProductVM obj, IFormFile? file)
        {


            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"images\product");

                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    obj.Product.ImageUrl = @"\images\product\" + fileName;
                }

                _db.Products.Add(obj.Product);
                //enkel na volgende lijn ga je effectief wegschrijven naar de database. 
                //zo kan je bijvoorbeeld meerdere wijzingen bijhouden / klaarzetten en dan maar
                //1 keer naar de database gaan wanneer je klaar bent met alles
                _db.SaveChanges();
                //bericht toevoegen aan TempData om op volgende pagina melding te tonen
                TempData["success"] = "Product created successfully";
                //Je wilt een actie van de controller met de naam Index terug uitvoeren
                //hiermee worden alle categories terug geladen en getoond en ga je terug naar deze pagina
                return RedirectToAction("Index", "Product");
            }
            return View(obj);


        }
        public IActionResult Edit(int? id)
        {
            //Lijst van categories samenstellen om dropdownlist te vullen
            IEnumerable<SelectListItem> categoryList =
                _db.Categories.Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });

            //Werd er vanuit de view een ID mee gegeven? 
            if (id == null || id == 0)
            {
                return NotFound();
            }

            //op basis van ID het juiste Category object uit _db ophalen
            Product? productFromDb = _db.Products.Find(id);

            //product gevonden?
            if (productFromDb == null)
            {
                return NotFound();
            }

            ProductVM productVM = new ProductVM();
            productVM.Product = productFromDb;

            productVM.CategoryList = categoryList;
            return View(productVM);
            
            
        }

        [HttpPost]
        public IActionResult Edit(ProductVM obj, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"images\product");

                    if (!string.IsNullOrEmpty(obj.Product.ImageUrl))
                    {
                        //oude afbeelding verwijderen, hiervoor hebben we het pad naar afbeelding nodig
                        //in database staat er in het begin een backslash, die moeten we verwijderen
                        var oldImagePath = Path.Combine(wwwRootPath, obj.Product.ImageUrl.TrimStart('\\'));

                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }

                    }

                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    obj.Product.ImageUrl = @"\images\product\" + fileName;

                    _db.Products.Update(obj.Product);
                    _db.SaveChanges();
                    TempData["success"] = "Product edited!";
                    //Je wilt een actie van de controller met de naam Index terug uitvoeren
                    //hiermee worden alle categories terug geladen en getoond en ga je terug naar deze pagina
                    return RedirectToAction("Index", "Product");
                }
            }
            return View(obj);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            Product? productFromDb = _db.Products.FirstOrDefault(u => u.Id == id);

            if (productFromDb == null)
            {
                return NotFound();
            }
            return View(productFromDb);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int? id)
        {
            Product? obj = _db.Products.Find(id);
            if (obj == null)
            {
                return NotFound();
            }
            _db.Products.Remove(obj);
            _db.SaveChanges();
            return RedirectToAction("Index", "Product");

        }
    }
}
