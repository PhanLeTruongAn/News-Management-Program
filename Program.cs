using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Xml.Serialization;

namespace NMP
{
    public abstract class Person
    {
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }

        public Person() { }

        public Person(string name, string phoneNumber, string email)
        {
            Name = name;
            PhoneNumber = phoneNumber;
            Email = email;
        }

        public virtual void DisplayInfo()
        {
            Console.WriteLine($"Name: {Name}, Phone: {PhoneNumber}, Email: {Email}");
        }
    }

    public class Editor : Person
    {
        public string Department { get; set; }

        public Editor() { }

        public Editor(string name, string phoneNumber, string email, string department)
            : base(name, phoneNumber, email)
        {
            Department = department;
        }

        public override void DisplayInfo()
        {
            Console.WriteLine($"Dept: {Department}");
        }
    }

    public class Category
    {
        public string Name { get; set; }

        public Category() { }

        public Category(string name)
        {
            Name = name;
        }
    }

    public class News
    {
        public string Title { get; set; }
        public string Summary { get; set; }
        public DateTime BroadcastTime { get; set; }
        public Editor Editor { get; set; }
        public Category Category { get; set; }
        public News() { }

        public News(string title, string summary, DateTime broadcastTime, Editor editor)
        {
            Title = title;
            Summary = summary;
            BroadcastTime = broadcastTime;
            Editor = editor;
            Category = new Category(editor.Department); // Automatically set the category based on editor's department
        }


        public void DisplayInfo()
        {
            Console.WriteLine($"Title: {Title}, Time: {BroadcastTime}, Editor: {Editor.Name}, Category: {Category.Name}");
        }
    }

    public class Schedule
    {
        public DateTime Time { get; set; }
        public string Content { get; set; }
        public Editor Editor { get; set; }

        public Schedule() { }

        public Schedule(DateTime time, string content, Editor editor)
        {
            Time = time;
            Content = content;
            Editor = editor;
        }

        public void DisplayInfo()
        {
            Console.WriteLine($"Time: {Time}, Content: {Content}, Editor: {Editor.Name}");
        }
    }

    public class ScheduleManager
    {
        public List<Schedule> Schedules { get; set; }

        public ScheduleManager()
        {
            Schedules = new List<Schedule>();
        }

        public void AddSchedule(Schedule schedule)
        {
            Schedules.Add(schedule);
        }

        public void DisplaySchedules()
        {
            for (int i = 0; i < Schedules.Count; i++)
            {
                Schedule schedule = Schedules[i];
                schedule.DisplayInfo();
            }
        }
    }

    public class EditorManager
    {
        public List<Editor> EditorList { get; set; }

        public EditorManager()
        {
            EditorList = new List<Editor>();
        }

        public void AddEditor(Editor editor)
        {
            EditorList.Add(editor);
        }

        public void DisplayEditors()
        {
            foreach (Editor editor in EditorList)
            {
                editor.DisplayInfo();
            }
        }

        public Editor GetEditor(string name, string email)
        {
            foreach (Editor editor in EditorList)
            {
                if (editor.Name == name && editor.Email == email)
                {
                    return editor;
                }
            }
            return null;
        }
    }
    public delegate void NewsAddedEventHandler(News news);

    public delegate void NewsRemovedEventHandler(News news);

    public class NewsManager
    {
        public List<News> NewsList { get; set; }

        public NewsManager()
        {
            NewsList = new List<News>();
        }

        public void AddNews(News news)
        {
            NewsList.Add(news);
        }

        public void DisplayNews()
        {
            foreach (News news in NewsList)
            {
                news.DisplayInfo();
            }
        }

        public News FindNewsByTitle(string title)
        {
            foreach (News news in NewsList)
            {
                if (news.Title == title)
                {
                    return news;
                }
            }
            return null;
        }

        public bool EditNews(string title, string newSummary, DateTime newBroadcastTime)
        {
            News news = FindNewsByTitle(title);
            if (news != null)
            {
                news.Summary = newSummary;
                news.BroadcastTime = newBroadcastTime;
                return true;
            }
            return false;
        }

        public bool DeleteNews(string title)
        {
            News news = FindNewsByTitle(title);
            if (news != null)
            {
                NewsList.Remove(news);
                return true;
            }
            return false;
        }
    }

    public class FileHandler<T>
    {
        public static void SaveToFile(string filePath, T data)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (FileStream stream = new FileStream(filePath, FileMode.Create))
            {
                serializer.Serialize(stream, data);
            }
        }

        public static T LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath)) return default;
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (FileStream stream = new FileStream(filePath, FileMode.Open))
            {
                return (T)serializer.Deserialize(stream);
            }
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            Console.CursorVisible = false;

            EditorManager editorManager = FileHandler<EditorManager>.LoadFromFile("editors.xml") ?? new EditorManager();
            NewsManager newsManager = FileHandler<NewsManager>.LoadFromFile("news.xml") ?? new NewsManager();
            ScheduleManager scheduleManager = FileHandler<ScheduleManager>.LoadFromFile("schedules.xml") ?? new ScheduleManager();

            Editor currentEditor = null;

            while (true)
            {
                Console.Clear();
                DisplayLayout(currentEditor);

                char choiceChar = Console.ReadKey(true).KeyChar;

                if (int.TryParse(choiceChar.ToString(), out int choice))
                {
                    if (choice == 8)
                    {
                        FileHandler<EditorManager>.SaveToFile("editors.xml", editorManager);
                        FileHandler<NewsManager>.SaveToFile("news.xml", newsManager);
                        FileHandler<ScheduleManager>.SaveToFile("schedules.xml", scheduleManager);
                        Console.WriteLine("Data saved. Exiting...");
                        break;
                    }
                    HandleChoice(choice, ref currentEditor, editorManager, newsManager, scheduleManager);
                }
            }
        }

        static void DisplayNewsByCategory(NewsManager newsManager, string category)
        {
            if (newsManager == null || newsManager.NewsList == null)
            {
                Console.WriteLine("News manager or news list is not initialized.");
                return;
            }

            Console.Clear();
            Console.WriteLine($"=== {category} News ===\n");

            for (int i = 0; i < newsManager.NewsList.Count; i++)
            {
                News news = newsManager.NewsList[i];
                if (news.Category != null && news.Category.Name == category)
                {
                    Console.WriteLine("Title: {0}\nSummary: {1}\nBroadcast Time: {2}\n",
                        news.Title, news.Summary, news.BroadcastTime);
                }
            }
            Console.ReadKey();
        }

        static void DisplayLayout(Editor currentEditor)
        {
            Console.Clear();

            if (currentEditor != null)
            {
                Console.SetCursorPosition(0, 0);
                Console.WriteLine($"It's good to see you again {currentEditor.Name} of {currentEditor.Department} department!");
            }

            Console.SetCursorPosition(Console.WindowWidth - 25, 0);
            if (currentEditor == null)
            {
                Console.WriteLine("[1] Sign In   [2] Sign Up");
            }
            else
            {
                Console.WriteLine("[1] Sign Out    [0] Back");
            }

            Console.SetCursorPosition(Console.WindowWidth / 2 - 4, Console.WindowHeight / 2 - 5);
            Console.WriteLine("ALAT NEWS");

            Console.SetCursorPosition(Console.WindowWidth / 2 - 20, Console.WindowHeight / 2 - 2);
            Console.WriteLine("[3] Entertainment  [4] Sports  [5] Politics");

            if (currentEditor != null)
            {
                Console.SetCursorPosition(Console.WindowWidth / 2 - 16, Console.WindowHeight / 2 + 2);
                Console.WriteLine("[6] News Editor  [7] Schedule Editor");
            }

            Console.SetCursorPosition(Console.WindowWidth - 15, Console.WindowHeight - 2);
            Console.WriteLine("[8] Exit & Save");
        }

        static void HandleChoice(int choice, ref Editor currentEditor, EditorManager editorManager, NewsManager newsManager, ScheduleManager scheduleManager)
        {
            switch (choice)
            {
                case 1:
                    if (currentEditor == null)
                    {
                        currentEditor = SignInWindow(editorManager);
                    }
                    else
                    {
                        Console.WriteLine("Signing out...");
                        currentEditor = null;
                        Console.ReadKey();
                    }
                    break;

                case 2:
                    if (currentEditor == null)
                    {
                        SignUpWindow(editorManager);
                    }
                    else
                    {
                        Console.WriteLine("Already signed in.");
                        Console.ReadKey();
                    }
                    break;

                case 3:
                    DisplayNewsByCategory(newsManager, "Entertainment");
                    break;

                case 4:
                    DisplayNewsByCategory(newsManager, "Sports");
                    break;

                case 5:
                    DisplayNewsByCategory(newsManager, "Politics");
                    break;

                case 6:
                    if (currentEditor != null)
                    {
                        DisplayNewsEditor(newsManager, currentEditor);
                    }
                    break;

                case 7:
                    if (currentEditor != null)
                    {
                        DisplayScheduleEditor(scheduleManager, currentEditor);
                    }
                    break;
                case 0:
                    Console.WriteLine("Returning to previous window...");
                    currentEditor = null;
                    break;

                default:
                    Console.WriteLine("Invalid choice.");
                    Console.ReadKey();
                    break;
            }
        }

        static Editor SignInWindow(EditorManager editorManager)
        {
            Console.Clear();
            Console.SetCursorPosition(Console.WindowWidth / 2 - 4, Console.WindowHeight / 8);
            Console.WriteLine("=== Sign In ===");
            Console.Write("\nEnter your name: ");
            string name = Console.ReadLine();
            Console.Write("\nEnter your email: ");
            string email = Console.ReadLine();

            Editor editor = editorManager.GetEditor(name, email);
            if (editor != null)
            {
                Console.SetCursorPosition(Console.WindowWidth / 2 - 4, Console.WindowHeight / 2);
                Console.WriteLine($"\nWelcome, Editor {name}!");
                Console.ReadKey();
                return editor;
            }
            Console.SetCursorPosition(Console.WindowWidth / 2 - 4, Console.WindowHeight / 2);
            Console.WriteLine("\nEditor not found. Please sign up!");
            Console.ReadKey();
            return null;
        }

        static void SignUpWindow(EditorManager editorManager)
        {
            Console.Clear();
            Console.SetCursorPosition(Console.WindowWidth / 2 - 4, Console.WindowHeight / 8);
            Console.WriteLine("=== Editor Sign Up ===");
            Console.Write("\nEnter your name: ");
            string name = Console.ReadLine();
            Console.Write("\nEnter your phone: ");
            string phoneNumber = Console.ReadLine();
            Console.Write("\nEnter your email: ");
            string email = Console.ReadLine();
            Console.Write("\nEnter your department: ");
            string department = Console.ReadLine();

            Editor newEditor = new Editor(name, phoneNumber, email, department);
            editorManager.AddEditor(newEditor);
            Console.SetCursorPosition(Console.WindowWidth / 2 - 4, Console.WindowHeight / 2);
            Console.WriteLine("\nSign-up successful!");
            Console.ReadKey();
        }

        static void DisplayNewsEditor(NewsManager newsManager, Editor currentEditor)
        {
            // Clear the console
            Console.Clear();
            Console.SetCursorPosition(Console.WindowWidth / 3 + 8, Console.WindowHeight / 12);
            Console.WriteLine("=== News Editor ===");
            // Set cursor position for the news table display
            int tableX = Console.WindowWidth / 8; // Adjust for wider title section
            int tableY = Console.WindowHeight / 8; // Adjust vertical position

            Console.SetCursorPosition(tableX, tableY);
            DisplayNewsTable(newsManager); // Display the news table

            // Move the cursor to a new position below the table for options
            int optionsX = Console.WindowWidth / 3 - 2;
            int optionsY = tableY + newsManager.NewsList.Count + 6; // Adjust for table height
            Console.SetCursorPosition(optionsX, optionsY);

            // Display editor options
            Console.WriteLine("1. Add News  2. Edit News  3. Delete News");

            // Read and handle the user's input
            ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);
            int choice;
            if (int.TryParse(keyInfo.KeyChar.ToString(), out choice))
            {
                switch (choice)
                {
                    case 1:
                        AddNews(newsManager, currentEditor);
                        break;
                    case 2:
                        EditNews(newsManager, currentEditor);
                        break;
                    case 3:
                        DeleteNews(newsManager, currentEditor);
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
        }

        static void DisplayNewsTable(NewsManager newsManager)
        {
            // Draw the table header with adjusted column widths
            Console.WriteLine("Current News List:");
            Console.WriteLine("------------------------------------------------------------------------------------------------");
            Console.WriteLine("{0,-40} | {1,-20} | {2,-15} | {3}", "Title", "Broadcast Time", "Category", "Editor");
            Console.WriteLine("------------------------------------------------------------------------------------------------");

            if (newsManager != null && newsManager.NewsList != null)
            {
                for (int i = 0; i < newsManager.NewsList.Count; i++)
                {
                    News news = newsManager.NewsList[i];

                    string title = news.Title != null ? news.Title : "N/A";
                    string broadcastTime = news.BroadcastTime.ToString("yyyy-MM-dd HH:mm");
                    string category = news.Category != null && news.Category.Name != null ? news.Category.Name : "N/A";
                    string editorName = news.Editor != null && news.Editor.Name != null ? news.Editor.Name : "N/A";

                    // Print the news information with the wider title column
                    Console.WriteLine("{0,-40} | {1,-20} | {2,-15} | {3}", title, broadcastTime, category, editorName);
                }
            }

            Console.WriteLine("------------------------------------------------------------------------------------------------");
        }

        static void AddNews(NewsManager newsManager, Editor currentEditor)
        {
            Console.Clear();
            Console.SetCursorPosition(Console.WindowWidth / 4, Console.WindowHeight / 8 + 6);
            Console.Write("TITLE:   ");
            string title = Console.ReadLine();
            Console.SetCursorPosition(Console.WindowWidth / 4, Console.WindowHeight / 8 + 8);
            Console.Write("SUMMARY:   ");
            string summary = Console.ReadLine();
            Console.SetCursorPosition(Console.WindowWidth / 4, Console.WindowHeight / 8 + 10);
            Console.Write("Broadcast Time (yyyy-mm-dd hh:mm):   ");
            DateTime broadcastTime;
            DateTime.TryParse(Console.ReadLine(), out broadcastTime);

            // Automatically set the category to the editor's department
            News news = new News(title, summary, broadcastTime, currentEditor);
            newsManager.AddNews(news);

            Console.SetCursorPosition(Console.WindowWidth / 3 + 8, Console.WindowHeight / 8 + 12);
            Console.WriteLine("NEWS ADDED SUCCESSFULLY!");
            Console.ReadKey();
        }

        static void EditNews(NewsManager newsManager, Editor currentEditor)
        {
            Console.Clear();
            Console.WriteLine("Editing News...");
            Console.Write("Enter the title of the news to edit: ");
            string title = Console.ReadLine();

            News newsToEdit = null;
            foreach (News news in newsManager.NewsList)
            {
                if (news.Title == title && news.Editor.Email == currentEditor.Email && news.Category.Name == currentEditor.Department)
                {
                    newsToEdit = news;
                    break;
                }
            }

            if (newsToEdit != null)
            {
                Console.Write("Enter new title: ");
                newsToEdit.Title = Console.ReadLine();
                Console.Write("Enter new summary: ");
                newsToEdit.Summary = Console.ReadLine();

                Console.Write("Do you want to edit the broadcast time? (yes/no): ");
                string editBroadcastResponse = Console.ReadLine().Trim().ToLower();

                if (editBroadcastResponse == "yes")
                {
                    Console.Write("Enter new broadcast time (yyyy-MM-dd): ");
                    DateTime broadcastTime;
                    if (DateTime.TryParse(Console.ReadLine(), out broadcastTime))
                    {
                        newsToEdit.BroadcastTime = broadcastTime;
                        Console.WriteLine("Broadcast time updated.");
                    }
                    else
                    {
                        Console.WriteLine("Invalid date format.");
                    }
                }
            }
            else
            {
                Console.WriteLine("News not found or you do not have permission to edit this news.");
            }
            Console.ReadKey();
        }

        static void DeleteNews(NewsManager newsManager, Editor currentEditor)
        {
            Console.Clear();
            Console.WriteLine("Deleting News...");
            Console.Write("Enter the title of the news to delete: ");
            string title = Console.ReadLine();

            News newsToDelete = null;
            foreach (News news in newsManager.NewsList)
            {
                if (news.Title == title && news.Editor.Email == currentEditor.Email && news.Category.Name == currentEditor.Department)
                {
                    newsToDelete = news;
                    break;
                }
            }

            if (newsToDelete != null)
            {
                newsManager.NewsList.Remove(newsToDelete);
                Console.WriteLine("News deleted.");
            }
            else
            {
                Console.WriteLine("News not found or you do not have permission to delete this news.");
            }
            Console.ReadKey();
        }

        static void DisplayScheduleEditor(ScheduleManager scheduleManager, Editor currentEditor)
        {
            Console.Clear();
            Console.SetCursorPosition(Console.WindowWidth / 2 - 4, Console.WindowHeight / 8);
            Console.WriteLine("=== Schedule Editor ===");

            Console.WriteLine("\nCurrent Schedules:");
            Console.WriteLine("---------------------------------------------------");
            Console.WriteLine("{0,-20} | {1,-10} | {2}", "Time", "Content", "Editor");
            Console.WriteLine("---------------------------------------------------");

            for (int i = 0; i < scheduleManager.Schedules.Count; i++)
            {
                Schedule schedule = scheduleManager.Schedules[i];
                Console.WriteLine("{0,-20} | {1,-10} | {2}",
                    schedule.Time.ToString("yyyy-MM-dd HH:mm"),
                    schedule.Content,
                    schedule.Editor.Name);
            }

            Console.WriteLine("---------------------------------------------------");
            Console.WriteLine("\nOptions:");
            Console.WriteLine("1. Add Schedule");
            Console.WriteLine("2. Edit Schedule");
            Console.WriteLine("3. Delete Schedule");
            Console.Write("Choose an option: ");

            int choice;
            if (int.TryParse(Console.ReadLine(), out choice))
            {
                switch (choice)
                {
                    case 1:
                        AddSchedule(scheduleManager, currentEditor);
                        break;

                    case 2:
                        EditSchedule(scheduleManager, currentEditor);
                        break;

                    case 3:
                        DeleteSchedule(scheduleManager, currentEditor);
                        break;

                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
        }

        static void AddSchedule(ScheduleManager scheduleManager, Editor currentEditor)
        {
            Console.Write("Enter schedule time (yyyy-MM-dd): ");
            DateTime time = DateTime.Parse(Console.ReadLine());
            Console.Write("Enter content: ");
            string content = Console.ReadLine();

            Schedule schedule = new Schedule(time, content, currentEditor);
            scheduleManager.AddSchedule(schedule);
            Console.WriteLine("Schedule added.");
        }

        static void EditSchedule(ScheduleManager scheduleManager, Editor currentEditor)
        {
            Console.Clear();
            Console.WriteLine("=== Edit Schedule ===");
            scheduleManager.DisplaySchedules();

            Console.Write("\nEnter the time of the schedule you want to edit. (yyyy-MM-dd HH:mm): ");
            if (DateTime.TryParse(Console.ReadLine(), out DateTime selectedTime))
            {
                Schedule scheduleToEdit = null;
                foreach (Schedule schedule in scheduleManager.Schedules)
                {
                    if (schedule.Time == selectedTime && schedule.Editor.Email == currentEditor.Email)
                    {
                        scheduleToEdit = schedule;
                        break;
                    }
                }

                    if (scheduleToEdit != null)
                {
                    Console.Write("Enter new time (yyyy-MM-dd HH:mm): ");
                    DateTime newTime;
                    if (DateTime.TryParse(Console.ReadLine(), out newTime))
                    {
                        scheduleToEdit.Time = newTime;
                    }

                    Console.Write("Enter new context: ");
                    scheduleToEdit.Content = Console.ReadLine();

                    Console.WriteLine("Schedule update successfully");
                }
                else
                {
                    Console.WriteLine("\r\nNot found the schedule or you do not have permission to edit this schedule.");
                }
            }
            else
            {
                Console.WriteLine("Invalid date format.");
            }
            Console.ReadKey();
        }

        static void DeleteSchedule(ScheduleManager scheduleManager, Editor currentEditor)
        {
            Console.Clear();
            Console.WriteLine("=== Delete Schedule ===");
            scheduleManager.DisplaySchedules();

            Console.Write("\nEnter the time of the calendar you want to delete. (yyyy-MM-dd ): ");
            if (DateTime.TryParse(Console.ReadLine(), out DateTime selectedTime))
            {
                Schedule scheduleToDelete = null;
                foreach (Schedule schedule in scheduleManager.Schedules)
                {
                    if (schedule.Time == selectedTime && schedule.Editor.Email == currentEditor.Email)
                    {
                        scheduleToDelete = schedule;
                        break;
                    }
                }

                if (scheduleToDelete != null)
                {
                    scheduleManager.Schedules.Remove(scheduleToDelete);
                    Console.WriteLine("The schedule has been successfully deleted.");
                }
                else
                {
                    Console.WriteLine("No screening schedule found or you do not have permission to delete this schedule.");
                }
            }
            else
            {
                Console.WriteLine("Invalid date format.");
            }
            Console.ReadKey();
        }

    }

}

