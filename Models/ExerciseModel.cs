using MongoDB.Bson.Serialization.Attributes;

namespace TiengAnh.Models
{
    public class ExerciseModel : BaseModel
    {
        [BsonElement("ID_BT")]
        public int ID_BT { get; set; }
        
        [BsonElement("Question")]
        public string Question { get; set; } = null!;
        
        [BsonElement("Question_BT")]
        public string Question_BT { 
            get { return Question; }
            set { Question = value; }
        }
        
        [BsonElement("CorrectAnswer")]
        public string CorrectAnswer { get; set; } = null!;
        
        [BsonElement("Answer_BT")]
        public string Answer_BT { 
            get { return CorrectAnswer; }
            set { CorrectAnswer = value; }
        }
        
        // Helper method for char comparisons in views
        public bool IsAnswer(char option)
        {
            return CorrectAnswer != null && CorrectAnswer.Length > 0 && 
                   CorrectAnswer[0] == option;
        }
        
        [BsonElement("Options")]
        public List<string>? Options { get; set; }
        
        [BsonElement("Option_A")]
        public string Option_A { 
            get { return Options != null && Options.Count > 0 ? Options[0] : ""; }
            set { 
                if (Options == null) Options = new List<string>();
                if (Options.Count > 0) Options[0] = value;
                else Options.Add(value);
            }
        }
        
        [BsonElement("Option_B")]
        public string Option_B { 
            get { return Options != null && Options.Count > 1 ? Options[1] : ""; }
            set { 
                if (Options == null) Options = new List<string>();
                if (Options.Count > 1) Options[1] = value;
                else if (Options.Count == 1) Options.Add(value);
                else {
                    Options.Add(""); // Add placeholder for Option A
                    Options.Add(value);
                }
            }
        }
        
        [BsonElement("Option_C")]
        public string Option_C { 
            get { return Options != null && Options.Count > 2 ? Options[2] : ""; }
            set { 
                if (Options == null) Options = new List<string>();
                while (Options.Count < 2) Options.Add(""); // Add placeholders if needed
                if (Options.Count > 2) Options[2] = value;
                else Options.Add(value);
            }
        }
        
        [BsonElement("Option_D")]
        public string Option_D { 
            get { return Options != null && Options.Count > 3 ? Options[3] : ""; }
            set { 
                if (Options == null) Options = new List<string>();
                while (Options.Count < 3) Options.Add(""); // Add placeholders if needed
                if (Options.Count > 3) Options[3] = value;
                else Options.Add(value);
            }
        }
        
        [BsonElement("ExerciseType")]
        public string ExerciseType { get; set; } = null!; // MultipleChoice, FillBlank, WordOrdering
        
        [BsonElement("Level")]
        public string Level { get; set; } = null!;
        
        [BsonElement("ID_CD")]
        public int ID_CD { get; set; }
        
        [BsonElement("Explanation")]
        public string? Explanation { get; set; }
        
        [BsonElement("Explanation_BT")]
        public string? Explanation_BT { 
            get { return Explanation; }
            set { Explanation = value; }
        }

        [BsonElement("TopicName")]
        public string TopicName { get; set; } = string.Empty;
    }
}
