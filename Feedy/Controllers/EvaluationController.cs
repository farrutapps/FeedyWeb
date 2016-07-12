using Feedy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace Feedy.Controllers
{
    public class EvaluationController : Controller
    {
        private FeedyDbContext db = new FeedyDbContext();


        // GET: Evaluation
        public ActionResult Index()
        {
            PopulateQuestionnairesDropDownList();

                return View();       
                
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(int QuestionnaireID)
        {
            
            return RedirectToAction("SelectQuestions", new { questionnaireID = QuestionnaireID});
        }

       public ActionResult SelectQuestions(int questionnaireID, int? ActionQuestionID, int? selectedQuestionsDataID) {

            SelectedQuestionsData Data;

            if (selectedQuestionsDataID == null)
            {
                Data = new SelectedQuestionsData();

                Data.SelectedQuestions = new List<SelectedQuestion>();

                Data.Questions = db.Questions
                 .Where(q => q.QuestionnaireID == questionnaireID).ToList();

                db.SelectQuestionsDatas.Add(Data);
                db.SaveChanges();

                return View(Data);
            }


            else
            {


                Data = db.SelectQuestionsDatas
                    .Include(d => d.SelectedQuestions)
                    .Single(d => d.SelectedQuestionsDataID == selectedQuestionsDataID);


                Data.Questions = db.Questions
                 .Where(q => q.QuestionnaireID == questionnaireID).ToList();



                if (Data.SelectedQuestions == null)
                    Data.SelectedQuestions = new List<SelectedQuestion>();

                if (ActionQuestionID != null)
                {

                    //if already seleted, remove from db.
                    if (Data.SelectedQuestions.Select(sq => sq.QuestionID).Contains(ActionQuestionID.Value))
                    {
                        SelectedQuestion SelQuestion = Data.SelectedQuestions.Single(sq => sq.QuestionID == ActionQuestionID.Value);

                        Data.SelectedQuestions.Remove(SelQuestion);
                        db.Entry(SelQuestion).State = EntityState.Deleted;
                    }

                    //else, add new one.
                    else
                    {
                        SelectedQuestion selQuest = new SelectedQuestion();
                        selQuest.QuestionID = ActionQuestionID.Value;
                        Data.SelectedQuestions.Add(selQuest);

                        db.Entry(Data).State = EntityState.Modified;
                    }

                    db.SaveChanges();
                }
                return View(Data);
            }


            
        }

        private void PopulateQuestionnairesDropDownList(object selectedQuestionnaire = null)
        {
            var questionnairesQuery = from d in db.Questionnaires
                                      orderby d.Name
                                      select d;
            ViewBag.QuestionnaireID = new SelectList(questionnairesQuery, "QuestionnaireID", "Name", selectedQuestionnaire);
        }


    }

   
}