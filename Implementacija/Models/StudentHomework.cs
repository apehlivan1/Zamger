﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ooadproject.Models
{
    public class StudentHomework: IActivity
    {
        [Key]
        public int ID { get; set; }

        [ForeignKey("StudentCourse")]
        public int CourseID { get; set; }
        public StudentCourse? Course { get; set; }

        [ForeignKey("Homework")]
        public int HomeworkID { get; set; }
        public Homework? Homework { get; set; }

        public double PointsScored { get; set; }
        public string Comment { get; set; }

        public StudentHomework() { }

        public double GetPointsScored()
        {
            return PointsScored;
        }

        public double GetTotalPoints()
        {
            return Homework.TotalPoints;
        }

        public DateTime GetActivityDate()
        {
            return Homework.Deadline.Date;
        }

        public string GetActivityName()
        {
            throw new NotImplementedException();
        }

        public string GetActivityType()
        {
            return "Homework";
        }
    }
}
