using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Google.Apis.Storage.v1.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Presentation.Models;
using StackExchange.Redis;

namespace Presentation.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        string connectionString = "";
        public AdminController(IConfiguration configuration)
        {
            connectionString = configuration["redis"];
        }



        public IActionResult Create()
        {
            return View();
        }


        /*
        [HttpPost]
        public IActionResult Create(MenuItem m)
        {
            try
            {
                ConnectionMultiplexer cm = ConnectionMultiplexer.Connect(connectionString);
                var db = cm.GetDatabase();

                var myMenuItems = db.StringGet("menuitems");

                List<MenuItem> myList = new List<MenuItem>();

                if (myMenuItems.IsNullOrEmpty)
                {
                    myList = new List<MenuItem>();
                }
                else
                {
                    myList = JsonConvert.DeserializeObject<List<MenuItem>>(myMenuItems);
                }

                myList.Add(m);

                var myJsonString = JsonConvert.SerializeObject(myList);
                db.StringSet("menuitems", myJsonString);

                ViewBag.Message = "Saved in cache";
            }
            catch (Exception e)
            {
                ViewBag.Error = "not saved in cache";
            }
            

            return View();
        }*/

        [HttpPost]
        public IActionResult Create(CreditItem c)
        {
            try
            {
                ConnectionMultiplexer cm = ConnectionMultiplexer.Connect(connectionString);
                var db = cm.GetDatabase();

                var myCreditItems = db.StringGet("credititems");

                List<CreditItem> myList = new List<CreditItem>();

                if (myCreditItems.IsNullOrEmpty)
                {
                    myList = new List<CreditItem>();
                }
                else
                {
                    myList = JsonConvert.DeserializeObject<List<CreditItem>>(myCreditItems);
                }

                myList.Add(c);

                var myJsonString = JsonConvert.SerializeObject(myList);
                db.StringSet("credititems", myJsonString);

                ViewBag.Message = "Saved in cache";
            }
            catch (Exception e)
            {
                ViewBag.Error = "not saved in cache: "+e;
            }


            return View();
        }
        


    }

}
