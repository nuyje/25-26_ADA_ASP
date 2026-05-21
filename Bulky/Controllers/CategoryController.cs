using Bulky.Data;
using Bulky.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Bulky.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CategoryController(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            List<Category> objCategoryList = _db.Categories.ToList();
            return View(objCategoryList);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category obj)
        {
            //Zelf custom validatie toevoegen aan website 
            if (obj.Name == obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError("Name", "Display Order cannot exactly match the name.");
            }

            if (obj.Name != null && obj.Name.ToLower() == "test")
            {
                ModelState.AddModelError("", "Name test is not allowed.");
            }

            if (ModelState.IsValid)
            {
                _db.Categories.Add(obj);
                _db.SaveChanges();

                TempData["gelukt"] = "Category added successfully"; 

                return RedirectToAction("Index", "Category");
            }
            return View();

        }
        public IActionResult Edit(int? id)
        {
            //Controle op ID
            if (id == null || id == 0)
            {
                return NotFound();
            }

            //op basis van ID het juiste Category object uit _db ophalen
            Category? categoryFromDb = _db.Categories.Find(id); //.Find werkt enkel op primary key! 
            //andere methoden om dit te doen: 
            Category? categoryFromDb1 = _db.Categories.FirstOrDefault(u => u.Id == id);
            //met FirstOrDefault ook mogelijk om op andere velden dan de primary key te zoeken
            Category? categoryFromDb2 = _db.Categories.Where(u => u.Id == id).FirstOrDefault();
            if (categoryFromDb == null)
            {
                return NotFound();
            }
            return View(categoryFromDb);
        }

        [HttpPost]
        public IActionResult Edit(Category obj)
        {
            if (ModelState.IsValid)
            {

                _db.Categories.Update(obj);
                _db.SaveChanges();
                TempData["success"] = "Category edited!";
                //Je wilt een actie van de controller met de naam Index terug uitvoeren
                //hiermee worden alle categories terug geladen en getoond en ga je terug naar deze pagina
                return RedirectToAction("Index", "Category");
            }
            return View();


        }

        public IActionResult Delete(int? id)
        {
            //Controle op ID
            if (id == null || id == 0)
            {
                return NotFound();
            }

            //op basis van ID het juiste Category object uit _db ophalen
            Category? categoryFromDb = _db.Categories.Find(id); //.Find werkt enkel op primary key! 

            if (categoryFromDb == null)
            {
                return NotFound();
            }
            return View(categoryFromDb);
        }
        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int? id)
        {

            Category? categoryFromDb = _db.Categories.Find(id);
            if (categoryFromDb == null)
            {
                return NotFound();
            }

            _db.Categories.Remove(categoryFromDb);
            _db.SaveChanges();

            return RedirectToAction("Index");

        }
    }
}
