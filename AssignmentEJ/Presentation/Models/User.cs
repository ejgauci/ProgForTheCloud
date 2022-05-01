using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Firestore;

namespace Presentation.Models
{
    [FirestoreData]
    public class User
    {
        [FirestoreProperty]
        public string Email { get; set; }

        [FirestoreProperty]
        public string FirstName { get; set; }

        [FirestoreProperty]
        public int Credits { get; set; }

        /*
        [FirestoreProperty]
        public string LastName { get; set; }

        [FirestoreProperty]
        public string MobileNo { get; set; }

        public string FullName
        {
            get { return FirstName + " " + LastName; }
        }*/


    }
}
