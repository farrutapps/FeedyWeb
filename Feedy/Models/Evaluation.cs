using Feedy.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Feedy.Models
{





    public class Query
    {
        private FeedyDbContext db = new FeedyDbContext();
        public Query(List<Event> events, List<Question> questionSelection)
        {
            this.Events = events;
            this.QuestionSelection = questionSelection;
        }
        public List<Event> Events { get; set; }
        public List<Question> QuestionSelection { get; set; }

        public List<QuestionEvaluation> Evaluations
        {
            get
            {
                // Get data of events, fill into EvalQuestions for selected Questions. For each, call a new Questionevaluation.

                Question EvaluationQuestion;

                int ParticipantsCount = Events.Select(e => e.ParticipantsCount).Sum();

                // Select from database each question with data from wished events.
                foreach (var question in QuestionSelection)
                {
                    EvaluationQuestion = db.Questions
                        .Include(q => q.Answers.Select(a => a.CountDataSet.Where(c => Events.Select(e => e.EventID).Contains(c.EventID))))
                        .Where(qu => qu.QuestionID == question.QuestionID)
                        .FirstOrDefault();

                    Evaluations.Add(new QuestionEvaluation(EvaluationQuestion, ParticipantsCount));
                }



                return Evaluations;
            }
        }
    }

    public class QuestionEvaluation
    {
        public QuestionEvaluation(Question question, int participantsCount)
        {
            // for each answer in question create corresponding evaluations.
            QuestionName = question.Text;
            EvalMode = question.EvalMode;

            foreach (var answer in question.Answers)
            {
                if (question.EvalMode == EvaluationMode.MEAN_VALUE)
                {
                    AnswerEvaluations.Add(new MeanValueEvaluation(question, participantsCount));
                }
                else
                {
                    switch (question.EvalMode)
                    {
                        case EvaluationMode.ABSOLUTE:
                            AnswerEvaluations.Add(new AbsoluteEvaluation(answer));
                            break;
                        case EvaluationMode.PERCENTAGE:
                            AnswerEvaluations.Add(new PercentageEvaluation(answer, participantsCount));
                            break;

                        case EvaluationMode.TEXT:
                            AnswerEvaluations.Add(new TextEvaluation(answer));
                            break;
                        default:
                            AnswerEvaluations.Add(new AbsoluteEvaluation(answer));
                            break;
                    }
                }

            }

        }
        public string QuestionName { get; set; }
        public EvaluationMode EvalMode { get; set; }
        public List<AnswerEvaluation> AnswerEvaluations { get; set; }
    }

    public class AnswerEvaluation
    {

    }
    public class TextEvaluation : AnswerEvaluation
    {
        public TextEvaluation(Answer answer)
        {
            TextAnswers = answer.TextDataSet.Select(t => t.Text).ToList();
        }

        public List<string> TextAnswers { get; set; }
    }

    public class AbsoluteEvaluation : AnswerEvaluation
    {
        public AbsoluteEvaluation(Answer answer)
        {
            Value = answer.CountDataSet.Select(c => c.Count).Sum();
            AnswerText = answer.Text;
        }

        public int Value { get; set; }
        public string AnswerText { get; set; }
    }

    public class PercentageEvaluation : AnswerEvaluation
    {
        private Answer answer;


        public PercentageEvaluation(Answer answer, int participantsCount)
        {
            Value = (double)answer.CountDataSet.Select(c => c.Count).Sum() / participantsCount;
            AnswerText = answer.Text;
        }

        public double Value { get; set; }
        public string AnswerText { get; set; }
    }

    public class MeanValueEvaluation : AnswerEvaluation
    {
        public MeanValueEvaluation(Question Question, int participantsCount)
        {
            EvaluationLabel = "Mittelwert:";
            int AnswerCount = Question.Answers.Count;
            List<Answer> Answers = Question.Answers.ToList();

            // calc Value
            Value = 0;
            for (int i = 0; i < AnswerCount; ++i)
            {
                Value += (i + 1) * Answers[i].CountDataSet.Select(c => c.Count).Sum();
            }

            Value = Value / participantsCount;

            FirstAnswer = Answers.First().Text;
            LastAnswer = Answers.Last().Text;

            FirstAnswerValue = 1;
            LastAnswerValue = AnswerCount;
        }

       public string EvaluationLabel { get; set; }
       public double Value { get; set; }
       public string FirstAnswer { get; set; }

       public int FirstAnswerValue;
       public string LastAnswer { get; set; }
       public int LastAnswerValue;
    }

    public enum EvaluationMode
    {
        MEAN_VALUE, ABSOLUTE, PERCENTAGE, TEXT
    }

}

    
   