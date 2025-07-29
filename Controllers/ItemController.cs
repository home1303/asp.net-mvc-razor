using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductsRazor.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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

            LoadCategories();
            return View();
        }

        [HttpPost] //POST Create
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Price,IdCategory")] Item item, string[] serialNumber)
        {
            ModelState.Remove("CreateBy");
            ModelState.Remove("UpdateBy");
            ModelState.Remove("IdCategoryNavigation");   //ต้องลบก่อน เพราะ ตั้งค่าไว้ว่า NOT NULL ซึ่งถ้าไม่ลบมันจะไม่ผ่านเงื่อนไข

            ValidateItem(item, serialNumber);

            if (ModelState.IsValid)
            {

                item.CreateBy = "admin";
                item.CreateDate = DateTime.Now;
                item.UpdateBy = "admin";
                item.UpdateDate = DateTime.Now;
                item.IsDeleted = false;

                var joinTable = new List<JoinTable>();

                if (serialNumber != null)
                {
                    foreach (var sn in serialNumber)
                    {
                        if (!string.IsNullOrWhiteSpace(sn))
                        {
                            var serial = new Serial
                            {
                                SerialNumber = sn.Trim(),
                                Status = "Active",
                                CretaeDate = DateTime.Now,
                                UpdateDate = DateTime.Now
                            };

                            var join = new JoinTable
                            {
                                Item = item,
                                Serial = serial,
                                Remark = null

                            };
                            joinTable.Add(join);
                        }
                    }
                }

                item.JoinTables = joinTable;
                _context.Items.Add(item);  //cascade serial add พร้อมกับ item
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            LoadCategories();
            return View(item);
        }





        public async Task<IActionResult> Index()
        {
            List<Item> items = await _context.Items //ดึงข้อมูลทั้งหมด
            .Where(item => !item.IsDeleted) // เฉพาะ ทีเป็น false(0)
            .Include(item => item.IdCategoryNavigation)//ดึง namecategory มาแสดงด้วย
            .Include(item => item.JoinTables)             // ดึง join table
                .ThenInclude(JoinTable => JoinTable.Serial)//ดึง serials มาแสดงด้วย
            .ToListAsync();
            return View(items);
        }

        public async Task<IActionResult> Edit(int? id)  //GET
        {
            if (id == null || _context.Items == null)
            {
                return NotFound();
            }

            var item = await _context.Items
                .Include(i => i.IdCategoryNavigation)
                .Include(i => i.JoinTables)
                    .ThenInclude(JoinTable => JoinTable.Serial)
                .FirstOrDefaultAsync(m => m.IdItem == id); //ดึงข้อมูลทั้งหมดตาม id
            if (item == null)
            {
                return NotFound();
            }
            ViewBag.SerialNumber = item.JoinTables.Select(jt => jt.Serial.SerialNumber).ToList();   //ดึง serialNumber โดย item->JoinTables->Serial->SerialNumber

            LoadCategories();
            return View(item);
        }

        private bool ItemExists(int id)
        {
            return _context.Items.Any(e => e.IdItem == id);
        }

        [HttpPost]  //POST Update
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdItem,Name,Price,IdCategory")] Item item, string[] serialNumber)//ถ้าไม่ระบุ IdItem จะเกิดปัญหาในการสร้างข้อมูลใหม่ เพราะ IdItem เป็น Primary Key
        {
            if (id != item.IdItem)
            {
                return NotFound();
            }

            ModelState.Remove("CreateBy");
            ModelState.Remove("UpdateBy");
            ModelState.Remove("IdCategoryNavigation");    //ต้องลบก่อน เพราะ ตั้งค่าไว้ว่า NOT NULL ซึ่งถ้าไม่ลบมันจะไม่ผ่านเงื่อนไข


            ValidateItem(item, serialNumber);

            if (ModelState.IsValid)
            {
                try
                {
                    //ดึงค่า serial ตาม item ปัจจุบัน โดย Serials-> Jointable->ดึงจาก ทุกๆ(item.Iditem) เทียบกันว่าเท่ากับ item.Iditem ปัจจุบัน 
                    var existingJTables = await _context.JoinTables.Include(jt => jt.Serial).Where(jt => jt.Item.IdItem == item.IdItem).ToListAsync();


                    item.CreateBy = "admin";                  //อัพเดตให้ครบทุกฟิลด์
                    item.CreateDate = DateTime.Now;
                    item.UpdateBy = "admin";
                    item.UpdateDate = DateTime.Now;
                    item.IsDeleted = false;
                    _context.Update(item);

                    await _context.SaveChangesAsync();


                    var serialNumbersList = serialNumber
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .Select(s => s.Trim())
                        .ToList();

                    // ลบ JoinTable และ Serial ที่ไม่ได้ใช้
                    foreach (JoinTable jt in existingJTables)
                    {
                        if (!serialNumbersList.Contains(jt.Serial.SerialNumber))
                        {
                            // ลบ record ใน JoinTable ก่อน
                            _context.JoinTables.Remove(jt);

                            // เช็คว่า Serial นี้ยังมี JoinTable อื่นอยู่ไหม
                            bool isSerialUsed = await _context.JoinTables
                                .AnyAsync(x => x.SerialId == jt.SerialId && x.ItemId != item.IdItem);

                            if (!isSerialUsed)
                            {
                                // ถ้า Serial ไม่มีใช้แล้วใน JoinTable อื่น ให้ลบจาก Serial
                                _context.Serials.Remove(jt.Serial);
                            }
                        }
                    }

                    foreach (var sn in serialNumbersList)
                    {

                        var joinexsits = existingJTables.Any(jt => jt.Serial.SerialNumber == sn);
                        if (!joinexsits)
                        {
                            var serialEntity = await _context.Serials.FirstOrDefaultAsync(s => s.SerialNumber == sn);
                            if (serialEntity == null)
                            {
                                // เพิ่มใหม่
                                serialEntity = new Serial
                                {
                                    SerialNumber = sn,
                                    Status = "Active",
                                    CretaeDate = DateTime.Now,
                                    UpdateDate = DateTime.Now
                                };
                                _context.Serials.Add(serialEntity);
                                await _context.SaveChangesAsync();
                            }
                            var newjoin = new JoinTable
                            {
                                ItemId = item.IdItem,
                                SerialId = serialEntity.IdSerial,
                                Remark = null
                            };
                            _context.JoinTables.Add(newjoin);
                        }
                    }

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

            if (serialNumber == null || serialNumber.Length == 0 || serialNumber.All(sn => string.IsNullOrWhiteSpace(sn)))
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
                .Include(i => i.JoinTables)
                    .ThenInclude(JoinTable => JoinTable.Serial)
                .FirstOrDefaultAsync(i => i.IdItem == id);
            if (item == null)
            {
                return NotFound();
            }

            ViewBag.SerialNumber = item.JoinTables.Select(jt => jt.Serial.SerialNumber).ToList();
            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.Items
                .Include(i => i.JoinTables)
                    .ThenInclude(jt => jt.Serial)
                .FirstOrDefaultAsync(i => i.IdItem == id);
            if (item == null)
            {
                return NotFound();
            }

            //เก็บ serial ที่จะลบ
            var serialsToDelete = item.JoinTables.Select(jt => jt.Serial).ToList();

            // ลบ JoinTable records ที่เชื่อมกับ Item นี้
            if (item.JoinTables.Any())
            {
                _context.JoinTables.RemoveRange(item.JoinTables);
                await _context.SaveChangesAsync();
            }

            if (serialsToDelete.Any())
            {
                _context.Serials.RemoveRange(serialsToDelete);
                await _context.SaveChangesAsync();
            }
            _context.Items.Remove(item);
            // item.IsDeleted = true;
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
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
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