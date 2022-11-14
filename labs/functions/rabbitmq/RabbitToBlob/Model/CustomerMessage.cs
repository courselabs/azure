using System;

namespace RabbitToBlob
{
    public class CustomerMessage
    {
        public string EventType {get; set;}
        public int CustomerId {get; set;}
    }
}