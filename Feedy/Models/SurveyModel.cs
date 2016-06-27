using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Feedy.Models
    {
       public class Event
    {
        // Primary Key
        public int EventID { get; set; }
        
        [Required]
        public string Place { get; set; }

        
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? Date { get; set; }


        /** TODO: validate File extension **/
        [Required]
        [NotMapped]
        public HttpPostedFileBase SourceFile { get; set; }

        //Foreign Key
        [Required]
        public int QuestionnaireID { get; set; }

        //Navigational Properties
        public virtual Questionnaire Questionnaire { get; set; }
        public ICollection<CountData> NumericDatas { get; set; }
        public ICollection<TextData> TextDatas { get; set; }


    }

    public class Questionnaire
        {
            //Primary key
            public int QuestionnaireID { get; set; }

            [Required]
            public string Name { get; set; }

            public string Comments { get; set; }

            //Navigation Properties
            public ICollection<Question> Questions { get; set; }
            public ICollection<Event> Events { get; set; }

        }

        public class Question
        {
            //Constructor
            public Question(string QuestionText) { this.Text = QuestionText; }
            //Primary Key
            public int QuestionID { get; set; }

            public string Text { get; set; }

            //Foreign Key
            public int QuestionaireID { get; set; }
           

            //Navigation Property
            public virtual Questionnaire Questionnaire { get; set; }
            public ICollection<Answer> Answers { get; set; }
        }

        public class Answer
        {
            public Answer(string AnswerText) { this.Text = AnswerText; }
            //Primary Key
            public int AnswerID { get; set; }

            public string Text { get; set; }
            

            //Foreign Key
            public int QuestionId { get; set; }

            //Navigation Property
            public virtual Question Question { get; set; }
            public ICollection<TextData> TextDataSet { get; set; }
            public ICollection<CountData> CountDataSet { get; set; }

        }

    public class CountData
    {
        public CountData(int Count) { this.Count = Count; }
        //Primary Key
        public int CountDataID { get; set; }

        public int Count { get; set; }

        //Foreign Key
        public int AnswerID { get; set; }
        public int EventID { get; set; }

        //Navigation Property
        public virtual Answer Answer { get; set; }
        public virtual Event Event { get; set; }
    }

        public class TextData
    {
        public TextData(string value) { Text = value; }

        //Primary Key
        public int TextDataID { get; set; }

        public string Text { get; set; }

        //Foreign Key
        public int AnswerID { get; set; }
        public int EventID { get; set; }

        //Navigation Property
        public virtual Answer Answer { get; set; }
        public virtual Event Event { get; set; }

    }

        

        public class FeedyDbContext : DbContext
        {
            public DbSet<Event> Events { get; set; }
            public DbSet<Questionnaire> Questionnaires { get; set; }
            public DbSet<Question> Questions { get; set;}
            public DbSet<Answer> Answers { get; set; }
            public DbSet<TextData> TextDataSet { get; set; }
            public DbSet<CountData> CountDataSet { get; set; }
        }
    
}