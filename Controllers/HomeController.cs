﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using NewsWebApp.Models;

namespace NewsWebApp.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public ActionResult Index()

        {
            var UserId = User.Identity.GetUserId();

            if (UserId == null)
            {
                var list1 = db.PostTypes.ToList();

                return View(list1);
            }
            
            
            //var role = user.UserType;
            
            else
            {
                var user = db.Users.Where(a => a.Id == UserId).SingleOrDefault();
                switch (user.UserType)
                {
                    case ("Admin"):
                        return RedirectToAction("Index", "Admin");

                    case ("Editor"):
                        return RedirectToAction("IndexEditor", "Home");

                }
            }
            var list = db.PostTypes.ToList();

            return View(list);
        }
        public ActionResult Like(int id)
        {

            Post updatePost = db.Posts.ToList().Find(u => u.PostId == id);
            updatePost.Like += 1;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult DisLike(int id)
        {
            Post updatePost = db.Posts.ToList().Find(u => u.PostId == id);
            updatePost.DisLike += 1;
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult IndexEditor()
        {
            var list = db.PostTypes.ToList();

            return View(list);
        }

        [Authorize]
        public ActionResult Details(int postId)
        {
            var post = db.Posts.Find(postId);
            if (post == null)
            {
                return HttpNotFound();
            }
            Session["postId"] = postId;

            return View(post);
        }
        [Authorize]
        public ActionResult Ask()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Ask(string message)
        {
            var UserId = User.Identity.GetUserId();
            var PostId = (int)Session["postId"];
            var check = db.AskForPosts.Where(a => a.PostId == PostId && a.UserId == UserId).ToList();
            if ( check.Count < 1 )
            {
                var post = new AskForPost();
                post.UserId = UserId;
                post.PostId = PostId;
                post.Message = message;
                db.AskForPosts.Add(post);
                db.SaveChanges();
                ViewBag.Result = "Successfully ..";
            }
            else
            {
                ViewBag.Result = "Your Question already sent..";
            }

            return View();
        }
        public ActionResult Reply()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Reply(string Reply,int id)
        {
           
            AskForPost ReplyMessage = db.AskForPosts.Find(id);
            if ( ReplyMessage == null)
            {
                return HttpNotFound();
            }

            ReplyMessage.Reply = Reply;
            db.Entry(ReplyMessage).State = EntityState.Modified;
            db.SaveChanges();

            ViewBag.Result = " Replied Successfully ..";


            return View();
        }

        [Authorize]
        public ActionResult GetAskByUser()
        {
            var UserId = User.Identity.GetUserId();
            var post = db.AskForPosts.Where(a => a.UserId == UserId);



            return View(post.ToList());
        }
        
        [Authorize]
        public ActionResult DetailsOfPost(int postId)
        {
            var post = db.AskForPosts.Where(a => a.PostId == postId).SingleOrDefault();
            var Id = post.Id;
            var asks = db.AskForPosts.Find(Id);
            if (post == null)
            {
                return HttpNotFound();
            }

            return View(asks);
        }
        public ActionResult DeleteQuestion(int? postid)
        {
            if (postid == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var Que = db.AskForPosts.Where(a => a.PostId == postid).SingleOrDefault();
            var Id = Que.Id;
            AskForPost post = db.AskForPosts.Find(Id);
            if (post == null)
            {
                return HttpNotFound();
            }
            return View(post);
        }

        // POST: Saved/Delete/5
        [HttpPost, ActionName("DeleteQuestion")]
        public ActionResult DeleteQuestion(int postid)
        {
            var Que = db.AskForPosts.Where(a => a.PostId == postid).SingleOrDefault();
            var Id = Que.Id;
            AskForPost post1 = db.AskForPosts.Find(Id);
            db.AskForPosts.Remove(post1);
            db.SaveChanges();
            return RedirectToAction("GetAskByUser");
        }

        [Authorize]
        public ActionResult GetAskByEditor()
        {
            var UserId = User.Identity.GetUserId();
            var Posts = from app in db.AskForPosts
                        join post in db.Posts
                        on app.PostId equals post.PostId
                        where post.UserId == UserId
                        select app;

            var grouped = from j in Posts
                          group j by j.post.ArticleTitle
                          into gr
                          select new PostViewMoedl
                          {
                              ArticleTitle = gr.Key,
                              Items = gr

                          };
            return View(grouped.ToList());
        }
        [Authorize]
        public ActionResult Search()
        {
            return View();
        }
        [Authorize]
        [HttpPost]
        public ActionResult Search(string SearchName)
        {
            var result = db.Posts.Where(a => a.ArticleTitle.Contains(SearchName)||
            a.PostType.ArticleType.Contains(SearchName)||a.User.UserName.Contains(SearchName)).ToList();
            return View(result);
        }
        [Authorize]
        public ActionResult SaveAction()
        {
            var UserId = User.Identity.GetUserId();
            var PostId =(int)Session["postId"];

            var check = db.SavePosts.Where(a => a.PostId == PostId && a.UserId == UserId).ToList();
            if(check.Count < 1) {
                var post = new SavePost();
                post.UserId = UserId;
                post.PostId = PostId;

                db.SavePosts.Add(post);
                db.SaveChanges();
                ViewBag.Result = " Saved Successfully ..";
                

            }
           else
            {
                ViewBag.Result = " Already Saved Check Your Saved Page..";
            }


            return View();

        }
        [Authorize]
        public ActionResult Saved()
        {
            var UserId = User.Identity.GetUserId();
            var Posts = from app in db.Posts
                        join post in db.SavePosts
                        on app.PostId equals post.PostId
                        where post.UserId == UserId
                        select app;

            return View(Posts.ToList());
        }
        // GET: Saved/Delete/5
        public ActionResult DeleteSaved(int? postid)
        {
            if (postid == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
             Post post = db.Posts.Find(postid);
            if (post == null)
            {
                return HttpNotFound();
            }
            return View(post);
        }

        // POST: Saved/Delete/5
        [HttpPost, ActionName("DeleteSaved")]
        public ActionResult DeleteSaved(int postid)
        {
            var post = db.SavePosts.Where(a => a.PostId == postid).SingleOrDefault();
            var Id = post.Id;
            SavePost post1 = db.SavePosts.Find(Id);
            db.SavePosts.Remove(post1);
            db.SaveChanges();
            return RedirectToAction("Saved");
        }



    }
}