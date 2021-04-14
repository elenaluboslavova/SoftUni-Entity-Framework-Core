namespace TeisterMask.DataProcessor
{
    using System;
    using System.Collections.Generic;

    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Xml.Serialization;
    using Data;
    using TeisterMask.Data.Models;
    using TeisterMask.Data.Models.Enums;
    using TeisterMask.DataProcessor.ImportDto;
    using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfullyImportedProject
            = "Successfully imported project - {0} with {1} tasks.";

        private const string SuccessfullyImportedEmployee
            = "Successfully imported employee - {0} with {1} tasks.";

        public static string ImportProjects(TeisterMaskContext context, string xmlString)
        {
            var sb = new StringBuilder(); 
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ProjectInputModel[]), new XmlRootAttribute("Projects"));

            using StringReader stringReader = new StringReader(xmlString);

            ProjectInputModel[] projectsDto = (ProjectInputModel[])xmlSerializer.Deserialize(stringReader);

            List<Project> list = new List<Project>();

            foreach (var currentProject in projectsDto)
            {
                if (!IsValid(currentProject))
                {
                    sb.AppendLine("Invalid Data!");
                    continue;
                }
                DateTime openDate;
                bool isOpenDateValid = DateTime.TryParseExact(currentProject.OpenDate, "dd/MM/yyyy",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out openDate);

                DateTime dueDate;
                bool isDueDateValid = DateTime.TryParseExact(currentProject.DueDate, "dd/MM/yyyy",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out dueDate);

                if (!isOpenDateValid || !isDueDateValid)
                {
                    sb.AppendLine("Invalid Data!");
                    continue;
                }

                var project = new Project
                {
                    Name = currentProject.Name,
                    DueDate = dueDate,
                    OpenDate = openDate
                };
                foreach (var currentTask in currentProject.Tasks)
                {
                    DateTime taskDueDate = DateTime.ParseExact(currentTask.DueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    DateTime taskOpenDate = DateTime.ParseExact(currentTask.OpenDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    if (taskOpenDate < openDate || taskDueDate > dueDate)
                    {
                        sb.AppendLine("Invalid Data!");
                        continue;
                    }
                    
                    var task = new Task
                    {
                        Name = currentTask.Name,
                        DueDate = DateTime.ParseExact(currentTask.DueDate.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture),
                        OpenDate = DateTime.ParseExact(currentTask.OpenDate.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture),
                        ExecutionType = (ExecutionType)currentTask.ExecutionType,
                        LabelType = (LabelType)currentTask.LabelType
                    };
                    project.Tasks.Add(task);
                }
                sb.AppendLine($"Successfully imported project - {project.Name} with {project.Tasks.Count} tasks.");
                list.Add(project);
                context.Projects.AddRange(list);
                context.SaveChanges();
            }
            return sb.ToString();
        }

        public static string ImportEmployees(TeisterMaskContext context, string jsonString)
        {
            return "TODO";
        }

        private static bool IsValid(object dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResult = new List<ValidationResult>();

            return Validator.TryValidateObject(dto, validationContext, validationResult, true);
        }
    }
}