using Google.Cloud.Firestore;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using UserModel = Presentation.Models.User;
using UploadModel = Presentation.Models.Upload;
using System;

namespace Presentation.Controllers
{
    public class CronController : Controller
    {
        string project = "";
        string bucketName = "";

        public CronController(IConfiguration configuration)
        {
            project = configuration["project"];
            bucketName = configuration["bucketName"];
        }

        public async Task<IActionResult> fileDelete()
        {
            var myStorage = StorageClient.Create();
            FirestoreDb db = FirestoreDb.Create(project);

            Query users = db.Collection("users");
            QuerySnapshot usersSnapshot = await users.GetSnapshotAsync();

            if (usersSnapshot != null)
            {
                foreach (DocumentSnapshot docSnapshot in usersSnapshot.Documents)
                {
                    UserModel user = docSnapshot.ConvertTo<UserModel>();
                    Query uploads = db.Collection("users").Document(user.Email).Collection("uploads");
                    QuerySnapshot filesSnapshot = await uploads.GetSnapshotAsync();

                    foreach (DocumentSnapshot fileSnapshot in filesSnapshot.Documents)
                    {
                        var fileDetails = fileSnapshot.ConvertTo<UploadModel>();
                        DateTime dateSent = fileDetails.DateSent.ToDateTime();
                        int daysGoneBy = DateTime.UtcNow.Subtract(dateSent).Days;

                        if (daysGoneBy > 1)
                        {
                            string extend = System.IO.Path.GetExtension(fileDetails.AttachmentUri);
                            myStorage.DeleteObject(bucketName, fileDetails.Id + extend);

                            DocumentReference docRef = db.Collection("users").Document(user.Email).Collection("uploads").Document(fileDetails.Id);
                            await docRef.DeleteAsync();
                        }
                    }
                }
            }
            return Ok("Original Files in Bucket Cleared");
        }
    }
}
