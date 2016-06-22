using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Feedy.Models;
using System.IO;
using Microsoft.VisualBasic.FileIO;
using System.Text;

namespace Feedy.Controllers
{
    public class SurveysController : Controller
    {
        private FeedyDbContext db = new FeedyDbContext();

        // GET: Surveys
        public ActionResult Index()
        {
            return View(db.Surveys.ToList());
        }

        // GET: Surveys/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Survey survey = db.Surveys.Find(id);
            if (survey == null)
            {
                return HttpNotFound();
            }
            return View(survey);
        }

        // GET: Surveys/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Surveys/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Name,Place,Date,SourceFile")] Survey survey)
        {
            if (ModelState.IsValid)
            {
                // TODO FILL FILE CONTENT TO MODEL.
                List < Question > Questions= ParseFileContent(survey.SourceFile);
                survey.Questions = Questions;
                db.Surveys.Add(survey);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(survey);
           
        }

        // GET: Surveys/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Survey survey = db.Surveys.Find(id);
            if (survey == null)
            {
                return HttpNotFound();
            }
            return View(survey);
        }

        // POST: Surveys/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Name,Place,Date")] Survey survey)
        {
            if (ModelState.IsValid)
            {
                db.Entry(survey).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(survey);
        }

        // GET: Surveys/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Survey survey = db.Surveys.Find(id);
            if (survey == null)
            {
                return HttpNotFound();
            }
            return View(survey);
        }

        // POST: Surveys/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Survey survey = db.Surveys.Find(id);
            db.Surveys.Remove(survey);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private string FileToString(HttpPostedFileBase file)
        {
            if (file == null)
                throw new ArgumentNullException("File is null. Error.");
            else
            {
                BinaryReader b = new BinaryReader(file.InputStream);
                
                byte[] binData = b.ReadBytes(checked((int)file.InputStream.Length));

                string result = System.Text.Encoding.Unicode.GetString(binData);

                return result;
            }
        }

        private List<Question> ParseFileContent(HttpPostedFileBase file)
        {
            string FileContent = FileToString(file);

            List<Question> Questions = new List<Question>();
            List<Answer> Answers = new List<Answer>();

            MemoryStream Strm = new MemoryStream(Encoding.Unicode.GetBytes(FileContent));
            TextFieldParser Parser = new TextFieldParser(Strm);

            string[] Delimiters = { ";" };
            Parser.Delimiters = Delimiters;

            List<string[]> Data = new List<string[]>();

            string[] RowElements;

            while (!Parser.EndOfData)
            {
                RowElements = Parser.ReadFields();

                Data.Add(RowElements);
            }


            int DataCounter = 0;
            //go through columns and create corresponding objects
            //First row of Data contains the QuestionTexts. Second Row AnswerTexts. Remaining rows contain data, either text or int.
            for(int column=0; column<Data[0].Length; ++column)
            {
                DataCounter = 0;

                for(int row=0; row<Data.Count; ++row)
                {
                    string Element = Data[row][column];

                    if (row == 0)
                    {
                        if (!string.IsNullOrEmpty(Element))
                        {
                            Questions.Add(new Question(Element));
                            Questions.Last<Question>().Answers = new List<Answer>();

                        }

                    }

                    else if (row == 1)
                    {
                        Questions.Last<Question>().Answers.Add(new Answer(Element));
                        Questions.Last<Question>().Answers.Last<Answer>().TextDataSet = new List<TextData>();
                    }

                    else
                    {
                        
                        if (string.IsNullOrWhiteSpace(Element))
                        {
                            /*ignore. We don't want to save or count emtpy Elements. */
                        }

                        //chec if Element is a number, and if it is roughly small enough to fit into an int. Relevant numbers will be much smaller than 10^5. Count them. Don't save.
                        else if (Element.All(c => char.IsDigit(c)) && Element.Count<char>() <= 5)
                        {
                            ++DataCounter;
                        }

                        //else its a TextAnswer, save it.
                        else
                        {
                            ++DataCounter;
                            Questions.Last<Question>().Answers.Last<Answer>().TextDataSet.Add(new TextData(Element));
                        }
                    }
                }
                //pass DataCounter to corresponding Answer.
                Questions.Last<Question>().Answers.Last<Answer>().Count = DataCounter;
            }

            return Questions;

        }
    }
}

    
        

