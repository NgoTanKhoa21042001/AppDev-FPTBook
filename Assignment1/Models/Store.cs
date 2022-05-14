﻿using Assignment1.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment1.Models
{
    public class Store
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Slogan { get; set; }
        public string UId { get; set; }
        public Assignment1User? User { get; set; }

        public virtual ICollection<Book>? Books { get; set; }  // bật tính năng overload khi nào cần thì nó load 
    }
}