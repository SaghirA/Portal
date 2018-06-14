using Job.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Job.Controllers
{
    public class JobController : Controller
    {
        JobPortalEntities db = new JobPortalEntities();

        // GET: Job
        public ActionResult Index()
        {
            db = new JobPortalEntities();
            return View(db.JobDetails.ToList());
        }

        public ActionResult Details(long id=0)
        {
            JobDetail jobdetail = db.JobDetails.Find(id);
            if (jobdetail == null)
                return HttpNotFound();
            return View(jobdetail);
                    
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(JobDetail job)
        {
            if (ModelState.IsValid)
            {
                db.JobDetails.Add(job);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Edit(long id=0)
        {
            JobDetail jobdetail = db.JobDetails.Find(id);
            if (jobdetail == null)
                return HttpNotFound();
            return View(jobdetail);
        }

        [HttpPost]
        public ActionResult Edit(JobDetail jobdetail)
        {
            if(ModelState.IsValid)
            {
                db.JobDetails.Add(jobdetail);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(jobdetail);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}