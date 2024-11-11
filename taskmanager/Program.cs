using System;
using Microsoft.Data.SqlClient;

namespace TaskManager
{
    class Program
    {

        static string connectionString = "Server=Your_server_name;Database=TaskManagerDB;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;";

        static void Main(string[] args)
        {

            if (TestDatabaseConnection())
            {
                Console.WriteLine("Database connection successful.");
            }
            else
            {
                Console.WriteLine("Failed to connect to the database.");
                return;
            }


            while (true)
            {
                Console.Clear();
                Console.WriteLine("Task Manager");
                Console.WriteLine("1. Add Task");
                Console.WriteLine("2. View Tasks");
                Console.WriteLine("3. Mark Task as Complete");
                Console.WriteLine("4. Remove Task");
                System.Console.WriteLine("5- Search/Filter Task");
                Console.WriteLine("6. Exit");
                Console.Write("Choose an option: ");

                string choice = Console.ReadLine();

                // Handle user input
                switch (choice)
                {
                    case "1":
                        AddTask();
                        break;
                    case "2":
                        ViewTasks();
                        break;
                    case "3":
                        MarkTaskAsComplete();
                        break;
                    case "4":
                        RemoveTask();
                        break;
                    case "5":
                        FilterTasks();
                        break;
                    case "6":
                        return;

                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }

                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
            }
        }


        static bool TestDatabaseConnection()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to database: {ex.Message}");  // Print the error message
                return false;
            }
        }


        static void AddTask()
        {
            Console.Write("Enter task description: ");
            string description = Console.ReadLine();

            Console.Write("Enter task priority (Low, Medium, High): ");
            string priority = Console.ReadLine();

            Console.Write("Enter due date (yyyy-mm-dd): ");
            DateTime dueDate;
            if (!DateTime.TryParse(Console.ReadLine(), out dueDate))
            {
                Console.WriteLine("Invalid date format.");
                return;
            }

            if (dueDate < DateTime.Now)
            {
                Console.WriteLine("The due date must be in the future.");
                return;
            }


            string query = "INSERT INTO Tasks (Description, DueDate, Priority, IsComplete) VALUES (@Description, @DueDate, @Priority, 0)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Description", description);
                command.Parameters.AddWithValue("@DueDate", dueDate);
                command.Parameters.AddWithValue("@Priority", priority);

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    Console.WriteLine("Task added.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error adding task: {ex.Message}");
                }
            }
        }

        static void ViewTasks()
        {
            string query = "SELECT TaskId, Description, DueDate, IsComplete FROM Tasks";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    Console.WriteLine("\nID\tDescription\tDue Date\tComplete");

                    int idWidth = 8;
                    int descriptionWidth = 17;
                    int dateWidth = 17;
                    int completeWidth = 10;

                    while (reader.Read())
                    {
                        int taskId = reader.GetInt32(0);
                        string description = reader.GetString(1);
                        DateTime dueDate = reader.GetDateTime(2);
                        bool isComplete = reader.GetBoolean(3);
                        Console.WriteLine(
                        $"{taskId.ToString().PadRight(idWidth)}" +
                        $"{description.PadRight(descriptionWidth)}" +
                        $"{dueDate.ToShortDateString().PadRight(dateWidth)}" +
                        $"{(isComplete ? "Yes" : "No").PadRight(completeWidth)}"
                );
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error fetching tasks: {ex.Message}");
                }
            }
        }

       static void MarkTaskAsComplete()
{
    int taskId;

    
    while (true)
    {
        Console.Write("Enter task ID to mark as complete: ");
        string taskIdString = Console.ReadLine();

        // Validate the input for the task ID
        if (int.TryParse(taskIdString, out taskId))
        {
            break; 
        }
        else
        {
            Console.WriteLine("Invalid ID. Please enter a valid numeric task ID.");
        }
    }

 
    string query = "SELECT IsComplete FROM Tasks WHERE TaskId = @TaskId";

    using (SqlConnection connection = new SqlConnection(connectionString))
    {
        SqlCommand command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@TaskId", taskId);

        try
        {
            connection.Open();
            SqlDataReader reader = command.ExecuteReader();

           
            if (reader.Read())
            {
                bool isComplete = reader.GetBoolean(0); 

                
                if (isComplete)
                {
                    Console.WriteLine("This task is already marked as complete.");
                }
                else
                {
                    reader.Close();

                    string updateQuery = "UPDATE Tasks SET IsComplete = 1 WHERE TaskId = @TaskId";
                    SqlCommand updateCommand = new SqlCommand(updateQuery, connection);
                    updateCommand.Parameters.AddWithValue("@TaskId", taskId);

                    int rowsAffected = updateCommand.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        Console.WriteLine("Task marked as complete.");
                    }
                    else
                    {
                        Console.WriteLine("Failed to mark task as complete. Task not found.");
                    }
                }
            }
            else
            {
                Console.WriteLine("Task not found.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating task: {ex.Message}");
        }
    }
}

        static void RemoveTask()
        {
            Console.Write("Enter task ID to remove: ");
            string taskIdString = Console.ReadLine();

            if (int.TryParse(taskIdString, out int taskId))
            {

                string query = "DELETE FROM Tasks WHERE TaskId = @TaskId";  // Correct column name is TaskId

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@TaskId", taskId);

                    try
                    {
                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                            Console.WriteLine("Task removed.");
                        else
                            Console.WriteLine("Task not found.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error removing task: {ex.Message}");
                    }
                }
            }
            else
            {
                Console.WriteLine("Invalid task ID.");
            }
        }

        static void FilterTasks()
        {
            Console.Write("Enter a keyword to filter tasks by description: ");
            string keyword = Console.ReadLine();

            string query = "SELECT TaskId, Description, DueDate, IsComplete FROM Tasks WHERE Description LIKE @Keyword";


            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Keyword", "%" + keyword + "%");

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    Console.WriteLine("\nID\tDescription\tDue Date\tComplete");

                    while (reader.Read())
                    {
                        int taskId = reader.GetInt32(0);
                        string description = reader.GetString(1);
                        DateTime dueDate = reader.GetDateTime(2);
                        bool isComplete = reader.GetBoolean(3);
                        Console.WriteLine($"{taskId}\t{description.PadRight(25)}{dueDate.ToShortDateString().PadRight(12)}{(isComplete ? "Yes" : "No")}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error fetching tasks: {ex.Message}");
                }
            }


        }
    }
}
