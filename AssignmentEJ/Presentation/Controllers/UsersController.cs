using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Diagnostics.AspNetCore3;
using Google.Cloud.Firestore;
using Google.Cloud.PubSub.V1;
using Google.Cloud.Storage.V1;
using Google.Protobuf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UserModel = Presentation.Models.User;
using Presentation.Models;

namespace Presentation.Controllers
{
    public class UsersController : Controller
    {
        /*
         * string project = "";
         * string bucketName = "";
            public UsersController(IConfiguration configuration)
            {
                project = configuration["project"];
                bucketName = configuration["bucketName"];
            }
         */

        string project = "";
        string bucketName = "";
        private readonly ILogger<HomeController> _logger;
        private readonly IExceptionLogger _exceptionLogger;

        public UsersController(IConfiguration configuration, ILogger<HomeController> logger,
            [FromServices] IExceptionLogger exceptionLogger)
        {
            project = configuration["project"];
            bucketName = configuration["bucketName"];
            _logger = logger;
            _exceptionLogger = exceptionLogger;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {



            FirestoreDb db = FirestoreDb.Create(project);

            DocumentReference docRef = db.Collection("users").Document(User.Claims.ElementAt(4).Value);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            UserModel myUser = new UserModel();
            myUser.Email = User.Claims.ElementAt(4).Value;

            if (snapshot.Exists)
            {
                

                myUser = snapshot.ConvertTo<UserModel>();

            }
            else
            {
                _logger.LogInformation("Snapshot does ot exist: User/Index");
            }



            return View(myUser);
        }


        [Authorize]
        public async Task<IActionResult> Register(UserModel user, int creditsDD)
        {
            int currentCredits = user.Credits;

            FirestoreDb db = FirestoreDb.Create(project);

            DocumentReference docRef = db.Collection("users").Document(User.Claims.ElementAt(4).Value);

            user.Email = User.Claims.ElementAt(4).Value;

            user.Credits =  currentCredits+ creditsDD;

            await docRef.SetAsync(user);

            _logger.LogInformation("Accessed the Register method");


            return RedirectToAction("Index");
        }




        [HttpGet]
        public IActionResult Send()
        {


            return View();
        }

        //handle submit
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Send(Upload upl, IFormFile attachment)
        {

            upl.Id = Guid.NewGuid().ToString();

            try
            {
                if (attachment != null)
                {

                    FirestoreDb db = FirestoreDb.Create(project);


                    //subtracting tokens
                    DocumentReference docRef2 = db.Collection("users").Document(User.Claims.ElementAt(4).Value);
                    DocumentSnapshot snapshot = await docRef2.GetSnapshotAsync();
                    User userModel = new User();

                    if (snapshot.Exists)
                    {
                        userModel = snapshot.ConvertTo<User>();
                    }

                    if (userModel.Credits <= 0)
                    {
                        _logger.LogInformation("Not Enough Credits to convert");
                        return Ok("Not Enough Credits to convert");
                        //throw new Exception("Not Enough Credits to convert");
                    }
                    else
                    {
                        userModel.Credits -= 1;
                        await docRef2.SetAsync(userModel);

                        var storage = StorageClient.Create();
                        using (Stream myUploadingFile = attachment.OpenReadStream())
                        {
                            storage.UploadObject(bucketName, upl.Id + System.IO.Path.GetExtension(attachment.FileName), null,
                                myUploadingFile);
                        }

                        upl.AttachmentUri =
                            $"https://storage.googleapis.com/{bucketName}/{upl.Id + System.IO.Path.GetExtension(attachment.FileName)}";

                        upl.ConvertedUri =
                            $"https://storage.googleapis.com/{bucketName}/{upl.Id + ".pdf"}";




                        DocumentReference docRef = db.Collection("users").Document(User.Claims.ElementAt(4).Value).Collection("uploads").Document(upl.Id);

                        await docRef.SetAsync(upl);


                        //fix to convert pdf properly
                        string fileDoc = "";
                        if (attachment.Length > 0)
                        {
                            using (var memoryStream = new MemoryStream())
                            {
                                attachment.CopyTo(memoryStream);
                                var fileToBytes = memoryStream.ToArray();
                                fileDoc = Convert.ToBase64String(fileToBytes);
                            }
                        }

                        upl.AttachmentUri = fileDoc;

                        //publish to queue (pub/sub)

                        TopicName topic = new TopicName(project, "MSD63aEJTopic");
                        PublisherClient client = PublisherClient.Create(topic);
                        string file_serialized = JsonConvert.SerializeObject(upl); //upload to string
                        PubsubMessage upload = new PubsubMessage
                        {
                            Data = ByteString.CopyFromUtf8(file_serialized) //encapsulating string to byte string
                        };


                        

                        await client.PublishAsync(upload);
                    }
                    
                    
                }
            }
            catch(Exception ex)
            {
                _exceptionLogger.Log(ex);
                throw new Exception("Failed to upload file");
            }

            

            return RedirectToAction("List");

        }

        [Authorize]
        public async Task<IActionResult> List()
        {
            FirestoreDb db = FirestoreDb.Create(project);

            Query allUploadsQuery = db.Collection("users").Document(User.Claims.ElementAt(4).Value).Collection("uploads").OrderByDescending("DateSent"); ;
            QuerySnapshot allUploadsQuerySnapshot = await allUploadsQuery.GetSnapshotAsync();

            List<Upload> uploads = new List<Upload>();

            foreach (DocumentSnapshot documentSnapshot in allUploadsQuerySnapshot.Documents)
            {

                uploads.Add(documentSnapshot.ConvertTo<Upload>());

            }



            return View(uploads);
        }



    }
}
