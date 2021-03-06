using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Firestore;

namespace Presentation.Models
{
    [FirestoreData]
    public class Message
    {
        [FirestoreProperty]
        public string Id { get; set; }
        [FirestoreProperty]
        public string Text { get; set; }
        [FirestoreProperty]
        public string Recipient { get; set; }

        [FirestoreProperty, ServerTimestamp]
        public Timestamp DateSent { get; set; }

        [FirestoreProperty]
        public string AttachmentUri { get;set;}

    }
}
