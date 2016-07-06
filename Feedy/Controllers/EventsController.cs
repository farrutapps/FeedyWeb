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
using System.Data.Entity.Infrastructure;

namespace Feedy.Controllers
{
    public class EventsController : Controller
    {
        private FeedyDbContext db = new FeedyDbContext();

        // GET: Surveys
        public ActionResult Index()
        {
            return View(db.Events.ToList());
        }

        // GET: Surveys/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Questionnaire survey = db.Questionnaires.Find(id);
            if (survey == null)
            {
                return HttpNotFound();
            }
            return View(survey);
        }

        // GET: Surveys/Create
        public ActionResult Create()
        {
            PopulateQuestionnairesDropDownList();
            return View();
        }

        // POST: Surveys/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "EventID,Place,Date,SourceFile,QuestionnaireID")] Event ThisEvent)
        {

            try
            {
                if (ModelState.IsValid)
                {

                    ThisEvent.Questionnaire = db.Questionnaires
                        .Include(t => t.Questions.Select(a => a.Answers.Select(d => d.CountDataSet)))
                        .Include(t => t.Questions.Select(a => a.Answers.Select(d => d.TextDataSet)))
                        .FirstOrDefault();




                    // If Questionnaire has no questions saved yet, take the ones provided.
                    if (ThisEvent.Questionnaire.Questions.Any())
                    {
                        // TRY BLAA
                            AppendDataToQuestionnaire(ThisEvent);
                        
                       
                    }
                    else
                    {
                        ThisEvent.Questionnaire.Questions = ConvertFileToModel(ThisEvent);
                        db.Entry(ThisEvent.Questionnaire).State = EntityState.Modified;
                    }

                    db.Events.Add(ThisEvent);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            catch (RetryLimitExceededException /* dex */)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.)
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            }
            PopulateQuestionnairesDropDownList(ThisEvent.QuestionnaireID);
        
            return View(ThisEvent);
           
        }

        private void CheckDataForCompatibility(List<string[]> data, Questionnaire questionnaire)
        {
            // TO BE IMPLEMENTED
        }

        // GET: Surveys/CreateQuestionnaire
        public ActionResult CreateQuestionnaire()
        {
            
            return View();
        }

        // POST: Surveys/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateQuestionnaire([Bind(Include = "QuestionnaireID,Name,Comments")] Questionnaire questionnaire)
        {
   
                if (ModelState.IsValid)
                {
                    
                    db.Questionnaires.Add(questionnaire);
                    db.SaveChanges();
                    return RedirectToAction("Create");
                }
          
            return View(questionnaire);

        }


        // GET: Surveys/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Event ThisEvent = db.Events.Find(id);
            if (ThisEvent == null)
            {
                return HttpNotFound();
            }

            PopulateQuestionnairesDropDownList();
            return View(ThisEvent);
        }

        // POST: Surveys/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Name,Place,Date")] Event ThisEvent)
        {
            if (ModelState.IsValid)
            {
                db.Entry(ThisEvent).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(ThisEvent);
        }

        // GET: Surveys/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Questionnaire survey = db.Questionnaires.Find(id);
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
            Event ThisEvent = db.Events.Find(id);
            db.Events.Remove(ThisEvent);
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

        private void PopulateQuestionnairesDropDownList(object selectedQuestionnaire = null)
        {
            var questionnairesQuery = from d in db.Questionnaires
                                   orderby d.Name
                                   select d;
            ViewBag.QuestionnaireID = new SelectList(questionnairesQuery, "QuestionnaireID", "Name", selectedQuestionnaire);
        }

        

        private List<Question> ConvertFileToModel(Event myEvent)
        {
            List<string[]> Data = ParseFileContent(myEvent.SourceFile);

            //first two rows are question and answer texts.
            myEvent.ParticipantsCount = Data.Count - 2;

            List<Question> Questions = new List<Question>();
            List<Answer> Answers = new List<Answer>();

            TextData TextDataElement;
            CountData CountDataElement;

            int DataCounter = 0;
            //go through columns and create corresponding objects
            //First row of Data contains the QuestionTexts. Second Row AnswerTexts. Remaining rows contain data, either text or int.
            for (int column = 0; column < Data[0].Length; ++column)
            {
                DataCounter = 0;

                for (int row = 0; row < Data.Count; ++row)
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
                        Questions.Last<Question>().Answers.Last<Answer>().CountDataSet = new List<CountData>();
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
                            TextDataElement = new TextData(Element);
                            TextDataElement.Event = myEvent;
                            Questions.Last().Answers.Last().TextDataSet.Add(TextDataElement);
                        }
                    }
                }

                //pass DataCounter to corresponding Answer.
                CountDataElement = new CountData(DataCounter);
                CountDataElement.Event = myEvent;
                Questions.Last().Answers.Last().CountDataSet.Add(CountDataElement);
            }

            return Questions;
            
        }

        private void AppendDataToQuestionnaire(Event myEvent)
        {

            List<string[]> Data = ParseFileContent(myEvent.SourceFile);

            //first two rows are question and answer texts.
            myEvent.ParticipantsCount = Data.Count-2;

            try
            {
                CheckDataForCompatibility(Data, myEvent.Questionnaire);
            }
            
            catch(Exception e)
            {
                throw new NotImplementedException("Uploading wrong files is not yet handled.");
            }


            IEnumerable<Answer> RefAnswer;
            TextData TextDataElement;
            CountData CountDataElement;
            
            int DataCounter = 0;
            //go through columns and create corresponding objects
            //First row of Data contains the QuestionTexts. Second Row AnswerTexts. Remaining rows contain data, either text or int.
            for (int column = 0; column < Data[0].Length; ++column)
            {
                DataCounter = 0;


                //find corresponding questions and answers in model.

                RefAnswer =
                    from question in myEvent.Questionnaire.Questions
                    where question.Text == Data[0][column]
                    let answers = question.Answers
                    from answer in answers
                    where answer.Text == Data[1][column]
                    select answer;


                if (RefAnswer.Any())
                {
                    for (int row = 2; row < Data.Count; ++row)
                    {
                        string Element = Data[row][column];

                        // ignore if empty
                        if (!string.IsNullOrWhiteSpace(Data[row][column]))
                        {
                            //store if textanswer and count not null elements
                            if (!Element.All(c => char.IsDigit(c)))
                            {
                                TextDataElement = new TextData(Data[row][column]);
                                TextDataElement.Event = myEvent;
                                RefAnswer.FirstOrDefault().TextDataSet.Add(TextDataElement);

                            }
                            ++DataCounter;
                        }
                    }
                    CountDataElement = new CountData(DataCounter);
                    CountDataElement.Event = myEvent;
                    RefAnswer.FirstOrDefault().CountDataSet.Add(CountDataElement);
                }

                else
                {
                    // This is executed when no corresponding answer is found. For example names in »Mit welchem Teamer*in...«
                    RefAnswer =
                        from question in myEvent.Questionnaire.Questions
                        where question.Text == Data[0][column]
                        let answers = question.Answers
                        from answer in answers
                        select answer;

                    foreach (var answer in RefAnswer)
                    {
                        CountDataElement = new CountData(0);
                        CountDataElement.Event = myEvent;
                        RefAnswer.FirstOrDefault().CountDataSet.Add(CountDataElement);
                    }
                }
            }
        }

        private List<string[]> ParseFileContent(HttpPostedFileBase file)
        {

            BinaryReader b = new BinaryReader(file.InputStream);

            byte[] binData = b.ReadBytes(checked((int)file.InputStream.Length));

            string result = System.Text.Encoding.Unicode.GetString(binData);

            string FileContent = result;
            

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

            return Data;


        }
    }
}








