﻿namespace Domain.Book.Messages
{
    public class CreateBook
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public decimal Cost { get; set; }
        public int InventoryAmount { get; set; }
    }
}