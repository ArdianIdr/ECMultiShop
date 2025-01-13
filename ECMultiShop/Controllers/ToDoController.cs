using ECMultiShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ECMultiShop.Controllers
{
    public class ToDoController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public ToDoController()
        {
            _dbContext = new ApplicationDbContext(); 
        }

        public ActionResult ToDoList()
        {
            // marrim todo listen nga data descending
            var tasks = _dbContext.ToDoTasks.OrderByDescending(t => t.CreatedAt).ToList();
            return View(tasks);
        }

[HttpPost]
public JsonResult AddTask(string task)
{
    if (!string.IsNullOrWhiteSpace(task))
    {
        var newTask = new ToDoTask
        {
            Task = task,
            IsCompleted = false,
            CreatedAt = DateTime.Now
        };
        _dbContext.ToDoTasks.Add(newTask);
        _dbContext.SaveChanges();

        return Json(newTask);
    }

    return Json(new { success = false, message = "Task cannot be empty." });
}


        [HttpPost]
        public JsonResult ToggleTaskCompletion(int id)
        {
            var task = _dbContext.ToDoTasks.FirstOrDefault(t => t.Id == id);
            if (task != null)
            {
                task.IsCompleted = !task.IsCompleted;
                _dbContext.SaveChanges();
                return Json(new { success = true, task = task.Task, isCompleted = task.IsCompleted });
            }
            return Json(new { success = false });
        }


        [HttpPost]
public JsonResult DeleteTask(int id)
{
    var task = _dbContext.ToDoTasks.Find(id);
    if (task != null)
    {
        _dbContext.ToDoTasks.Remove(task);
        _dbContext.SaveChanges();
        return Json(new { success = true });
    }
    return Json(new { success = false, message = "Task not found." });
}



    }
}