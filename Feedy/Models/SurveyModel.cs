using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Feedy.Models
    {
       
        public class Survey
        {
            //Primary key
            public int ID { get; set; }

            
            public string Name { get; set; }
            public string Place { get; set; }
            

            [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
            public DateTime Date { get; set; }

            
            /** TODO: validate File extension **/
            [Required]
            [NotMapped]
            public HttpPostedFileBase SourceFile { get; set; }

            //Navigation Properties
            public ICollection<Question> Questions { get; set; }

        }

        public class Question
        {
            //Constructor
            public Question(string QuestionText) { this.Text = QuestionText; }
            //Primary Key
            public int ID { get; set; }

            public string Text { get; set; }

            //Foreign Key
            public int SurveyID { get; set; }

            //Navigation Property
            public virtual Survey Survey { get; set; }
            public ICollection<Answer> Answers { get; set; }
        }

        public class Answer
        {
            public Answer(string AnswerText) { this.Text = AnswerText; }
            //Primary Key
            public int ID { get; set; }

            public string Text { get; set; }
            public int Count { get; set; }

            //Foreign Key
            public int QuestionId { get; set; }

            //Navigation Property
            public virtual Question Question { get; set; }
            public ICollection<TextData> TextDataSet { get; set; }
        }

        public class TextData
    {
        public TextData(string value) { Text = value; }
        //Primary Key
        public int ID { get; set; }
        public string Text { get; set; }

        //Foreign Key
        public int AnswerID { get; set; }

        //Navigation Property
        public virtual Answer Answer { get; set; }

    }

        

        public class FeedyDbContext : DbContext
        {
            public DbSet<Survey> Surveys { get; set; }
            public DbSet<Question> Questions { get; set;}
            public DbSet<Answer> Answers { get; set; }
            public DbSet<TextData> TextDataSet { get; set; }
        }
    
}