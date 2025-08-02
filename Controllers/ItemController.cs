// using System.Diagnostics;
using Microsoft.AspNetCore.Mvc; // [HTTP]
using Microsoft.EntityFrameworkCore; //[DbContext]
using ProductsRazor.Models;
using Microsoft.AspNetCore.Mvc.Rendering; // SelectList => LoadCate
using System.Diagnostics;

namespace ProductsRazor.Controllers
{
    public class ItemController : Controller
    {
        private readonly ProductsContext _context;

        public ItemController(ProductsContext context)
        {
            _context = context;
        }

        public IActionResult Create()
        {

            Item item = new Item
            {
                Serials = new List<Serial>()
            };

            LoadCategories();
            return View(item);
        }

        [HttpPost] //POST Create
        [ValidateAntiForgeryToken]
        public IActionResult Create(Item item, string? Addsn, int? DeleteSI)
        {
            ModelState.Remove("CreateBy");
            ModelState.Remove("UpdateBy");
            ModelState.Remove("IdCategoryNavigation");   //ต้องลบก่อน เพราะ ตั้งค่าไว้ว่า NOT NULL ซึ่งถ้าไม่ลบมันจะไม่ผ่านเงื่อนไข

            if (!string.IsNullOrEmpty(Addsn))
            {
                item.Serials = item.Serials.Where(s => !s.Isdeleted).ToList();

                item.Serials.Add(new Serial());
                LoadCategories();
                return View(item);
            }

            if (DeleteSI != null)
            {
                var serial = item.Serials.FirstOrDefault(s => s.IdSerial == DeleteSI);
                if (serial != null)
                {
                    serial.Isdeleted = true;
                }
                item.Serials = item.Serials.Where(s => !s.Isdeleted).ToList();  //กรองตัวที่ isdeleted == false
                LoadCategories();
                return View(item);
            }



            for (int i = 0; i < item.Serials.Count; i++)
            {
                ModelState.Remove($"Serials[{i}].IdItemNavigation");
                ModelState.Remove($"Serials[{i}].Status");
            }

            if (ModelState.IsValid)
            {

                item.CreateBy = "admin";
                item.CreateDate = DateTime.Now;
                item.UpdateBy = "admin";
                item.UpdateDate = DateTime.Now;
                item.IsDeleted = false;

                if (item.Serials != null)
                { //เพราะ serialnumber โอกาสเป็น null ได้ แต่ serialnumber ที่เป็น array ไม่สามารถ null ได้
                    UpdateSerialNumber(item.Serials, item);
                }
                _context.Items.Add(item);
                _context.SaveChanges();
                return RedirectToAction("Index");


            }
            LoadCategories();
            return View(item);
        }







        public async Task<IActionResult> Index()
        {
            List<Item> items = await _context.Items //ดึงข้อมูลทั้งหมด
            .Where(item => !item.IsDeleted) // เฉพาะ ทีเป็น false(0)
            .Include(item => item.IdCategoryNavigation)//ดึง namecategory มาแสดงด้วย
            .Include(item => item.Serials)             // ดึง join table(ItemSerial)
            .ToListAsync();
            return View(items);
        }

        public async Task<IActionResult> Edit(int? id)  //GET
        {
            if (id == null || _context.Items == null)
            {
                return NotFound();
            }

            Item item = await _context.Items
                .Include(i => i.IdCategoryNavigation)
                .Include(i => i.Serials)
                .FirstAsync(m => m.IdItem == id); //ดึงข้อมูลทั้งหมดตาม id
            if (item == null)
            {  
                return NotFound();
            }


            LoadCategories();
            return View(item);
        }




        private bool ItemExists(int id)
        {
            return _context.Items.Any(e => e.IdItem == id);
        }

        [HttpPost]  //POST Update
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Item item, string? Addsn, List<int>? DeleteSI)//ถ้าไม่ระบุ IdItem จะเกิดปัญหาในการสร้างข้อมูลใหม่ เพราะ IdItem เป็น Primary Key
        {
            if (id != item.IdItem)
            {
                return NotFound();
            }

            ModelState.Remove("CreateBy");
            ModelState.Remove("UpdateBy");
            ModelState.Remove("IdCategoryNavigation");    //ต้องลบก่อน เพราะ ตั้งค่าไว้ว่า NOT NULL ซึ่งถ้าไม่ลบมันจะไม่ผ่านเงื่อนไข



            if (!string.IsNullOrEmpty(Addsn))
            {
                item.Serials.Add(new Serial()); 
                LoadCategories();
                return View(item);
            }

             if (DeleteSI != null && DeleteSI.Count > 0)
            {
                foreach (var idToDelete in DeleteSI)
                {
                    var serial = item.Serials.FirstOrDefault(s => s.IdSerial == idToDelete);
                    if (serial != null)
                    {
                        serial.Isdeleted = true;      // ทำเครื่องหมายว่าลบ (soft delete) 
                        serial.UpdateDate = DateTime.Now;  // อัพเดตวันที่แก้ไข
                    }
                }

                LoadCategories();
                return View(item);
            }




            for (int i = 0; i < item.Serials.Count; i++)
            {
                ModelState.Remove($"Serials[{i}].IdItemNavigation");
                ModelState.Remove($"Serials[{i}].Status");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    //อัพเดต
                    item.UpdateBy = "admin";
                    item.UpdateDate = DateTime.Now;

                    if (item.Serials != null)
                    { //เพราะ serialnumber โอกาสเป็น null ได้ แต่ serialnumber ที่เป็น array ไม่สามารถ null ได้
                        UpdateSerialNumber(item.Serials, item);

                    }
                    _context.Entry(item).State = EntityState.Modified;
                    await _context.SaveChangesAsync();

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ItemExists(item.IdItem))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            LoadCategories();
            return View(item);
        }

        private void UpdateSerialNumber(ICollection<Serial> Serials, Item item)
        {
            foreach (var sn in Serials)
            {
                if (sn.IdSerial == 0)
                {
                    sn.IdItem = item.IdItem;
                    sn.CretaeDate = DateTime.Now;
                    sn.UpdateDate = DateTime.Now;
                    sn.Status = "Active";
                    sn.Isdeleted = false;

                    _context.Serials.Add(sn);
                }
                else
                {

                    var existing = _context.Serials.FirstOrDefault(s => s.IdSerial == sn.IdSerial);
                    if (existing != null)
                    {

                        existing.SerialNumber = sn.SerialNumber;
                        existing.UpdateDate = DateTime.Now;
                        existing.Isdeleted = sn.Isdeleted;
                        // ห้ามแตะ Isdeleted ตรงนี้ เพราะลบไว้แล้วไม่ควร reset
                        _context.Serials.Update(existing);
                    }
                }
            }
        }


        private bool ValidateItem(Item item, string[] serialNumber)
        {
            bool hasError = false;

            if (string.IsNullOrWhiteSpace(item.Name))
            {
                ModelState.AddModelError("Name", "Name is required");
                hasError = true;
            }

            if (item.Price <= 0)
            {
                ModelState.AddModelError("Price", "Price is required.");
                hasError = true;
            }

            if (serialNumber == null || serialNumber.All(string.IsNullOrWhiteSpace))
            {
                ModelState.AddModelError("serialNumber", "Serial number is required.");
                hasError = true;
            }

            if (item.IdCategory <= 0)
            {
                ModelState.AddModelError("IdCategory", "Category is required.");
                hasError = true;
            }

            return hasError;
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Items == null)
            {
                return NotFound();
            }

            var item = await _context.Items
                .Include(i => i.IdCategoryNavigation)
                .Include(i => i.Serials.Where(s => !s.Isdeleted))
                .FirstOrDefaultAsync(i => i.IdItem == id);
            if (item == null)
            {
                return NotFound();
            }

            ViewBag.SerialNumber = item.Serials.Select(s => s.SerialNumber).ToList();
            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.Items
                .Include(i => i.Serials)
                .FirstOrDefaultAsync(i => i.IdItem == id);
            if (item == null)
            {
                return NotFound();
            }


            _context.Serials.RemoveRange(item.Serials);


            _context.Items.Remove(item);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return NotFound();
        }

        private void LoadCategories()        //โหลดข้อมูล Category
        {
            ViewBag.Categories = _context.Categories
            .Select(c => new SelectListItem
            {
                Value = c.IdCategory.ToString(),
                Text = c.NameCategory
            })
            .ToList();
        }

    }
}